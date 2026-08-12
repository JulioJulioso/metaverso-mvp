import { SceneManager } from './core/SceneManager.js';
import { InputController } from './core/InputController.js';
import { XRController } from './core/XRController.js';
import { levelConfig } from './config/levelConfig.js';
import { Player } from './entities/Player.js';
import { Coin } from './entities/Coin.js';
import { PickupSphere } from './entities/PickupSphere.js';
import { Platform } from './entities/Platform.js';
import { WallAssembly } from './entities/WallAssembly.js';
import { DeliveryZone } from './entities/DeliveryZone.js';
import { MediaScreen } from './entities/MediaScreen.js';
import { InteractionSystem } from './systems/InteractionSystem.js';
import { AchievementSystem } from './systems/AchievementSystem.js';
import { CircuitSystem } from './systems/CircuitSystem.js';
import { CameraRigSystem } from './systems/CameraRigSystem.js';
import { BimIndexStub } from './systems/BimIndexStub.js';
import { PickingSystem } from './systems/PickingSystem.js';
import { NetworkStub } from './systems/NetworkStub.js';
import { isNearXZ } from './systems/CollisionSystem.js';
import { HUD } from './ui/HUD.js';
import { XRHud } from './ui/XRHud.js';
import { BUILD_INFO } from './config/buildInfo.js';

import '@babylonjs/loaders/glTF';

async function boot() {
  const canvas = document.getElementById('renderCanvas');
  const hudRoot = document.getElementById('hud-root');
  if (!canvas || !hudRoot) {
    console.error('[main] Missing #renderCanvas or #hud-root');
    return;
  }

  void NetworkStub;

  const sceneManager = new SceneManager(canvas, {
    useLargeWorldRendering: false,
  });
  const scene = sceneManager.getScene();
  const camera = sceneManager.getCamera();
  const shadowGen = sceneManager.getShadowGenerator();

  const input = new InputController(canvas);
  const xr = new XRController(scene, {
    floorMeshes: [sceneManager.ground],
    // meters / second — Quest thumbstick locomotion
    movementSpeed: 3.2,
    rotationSpeed: 1.8,
  });
  await xr.init();

  const platforms = levelConfig.platforms.map(
    (p) => new Platform(scene, p, shadowGen)
  );
  const coins = levelConfig.coins.map((c) => new Coin(scene, c, shadowGen));
  const sphere = new PickupSphere(scene, levelConfig.pickupSphere, shadowGen);
  sphere.setWorldColliders(platforms, levelConfig.groundY);

  const walls = new WallAssembly(scene, levelConfig.walls, shadowGen);
  const zones = levelConfig.deliveryZones.map((z) => new DeliveryZone(scene, z));
  const media = new MediaScreen(scene, levelConfig.video, shadowGen);

  const player = new Player(
    scene,
    levelConfig.player,
    platforms,
    levelConfig.groundY,
    shadowGen
  );

  const circuit = new CircuitSystem(levelConfig.circuitSteps);
  /** @type {import('./ui/HUD.js').HUD|null} */
  let hudRef = null;
  /** @type {import('./ui/XRHud.js').XRHud|null} */
  let xrHudRef = null;

  function notify(msg, ms) {
    hudRef?.showMessage(msg, ms);
    xrHudRef?.showMessage(msg, ms);
  }

  const achievements = new AchievementSystem(coins.length, {
    onAllCollected: () => {
      circuit.complete('coins_all', '¡Todos los marcadores recogidos!');
      notify(
        'Ahora lleva la pelota a los sitios 1 → 2 → 3 (oeste, postes con anillo)'
      );
    },
  });

  const interactions = new InteractionSystem();
  const cameraRig = new CameraRigSystem(camera);

  const bimIndex = new BimIndexStub();
  bimIndex.loadMock(levelConfig.bimMockElements);

  function triggerRiseWalls() {
    if (walls.startRise()) {
      hudRef?.markRiseStarted();
      xrHudRef?.markRiseStarted();
      notify('Levantamiento de muros en curso…');
      return true;
    }
    if (walls.isRiseDone()) {
      notify('Los muros ya están levantados');
    } else if (walls.isRiseStarted()) {
      notify('Levantamiento ya en curso…');
    }
    return false;
  }

  function triggerExplodeWalls() {
    const started = walls.toggleExplode();
    if (started) {
      const opening = walls.getExplodeTarget() > 0.5;
      notify(
        opening ? 'Despiece de muros (visión explotada)' : 'Muros reensamblados'
      );
      if (opening) {
        circuit.complete('walls_explode', 'Visión explotada activada');
      }
      return true;
    }
    notify('Primero levanta los muros (botón / Y)');
    return false;
  }

  function openMediaPlayback() {
    media.playInWorld();
    if (xr.isInXR) {
      notify(`Video: ${media.getTitle()} (pantalla activa en VR)`, 3500);
    } else {
      hudRef?.openVideo({
        url: media.getEmbedUrl(true),
        title: media.getTitle(),
      });
    }
  }

  const hud = new HUD(
    hudRoot,
    {
      youtubeEmbedUrl: media.getEmbedUrl(),
      videoTitle: levelConfig.video.title,
    },
    {
      onRiseWalls: () => triggerRiseWalls(),
      onExplodeWalls: () => triggerExplodeWalls(),
    },
    { versionLabel: BUILD_INFO.label }
  );
  hudRef = hud;
  console.info('[boot] build', BUILD_INFO.label);

  const xrHud = new XRHud(scene, {
    versionLabel: BUILD_INFO.label,
    onRiseWalls: () => triggerRiseWalls(),
    onExplodeWalls: () => triggerExplodeWalls(),
  });
  xrHudRef = xrHud;

  hud.updateCoins(achievements.getState());
  xrHud.updateCoins(achievements.getState());
  achievements.onChange((state) => {
    hud.updateCoins(state);
    xrHud.updateCoins(state);
  });
  circuit.onChange((snap) => {
    hud.updateCircuit(snap);
    xrHud.updateCircuit(snap);
  });

  xr.onSessionChange((active) => {
    cameraRig.setEnabled(!active);
    if (active) {
      xrHud.attachToCamera(xr.getXRCamera());
    } else {
      xrHud.attachToCamera(null);
    }
    xrHud.setVisible(active);
    hudRoot.style.visibility = active ? 'hidden' : 'visible';
    player.mesh.isVisible = !active;
    if (active) {
      xrHud.updateCircuit(circuit.getSnapshot());
      xrHud.updateCoins(achievements.getState());
      notify(
        'Salto: botón A (derecho) o click del stick · HUD a ~2m delante',
        5000
      );
    }
  });

  for (const coin of coins) {
    interactions.register({
      id: coin.id,
      getPosition: () => coin.getPosition(),
      radius: levelConfig.coinCollectRadius,
      autoCollect: true,
      enabled: () => !coin.collected,
      onInteract: () => {
        coin.collect(() => achievements.registerCoinCollected());
      },
    });
  }

  interactions.register({
    id: sphere.id,
    getPosition: () => sphere.getPosition(),
    radius: levelConfig.pickupSphere.interactRadius,
    autoCollect: false,
    enabled: () => !sphere.isHeld(),
    onInteract: () => {
      const hand = xr.isInXR ? xr.getPrimaryHandNode() : null;
      if (player.pickUp(sphere, hand)) {
        notify(
          xr.isInXR
            ? 'Pelota en la mano — grip/B para soltar'
            : 'Pelota recogida — llévala a los sitios (F soltar)'
        );
      }
    },
  });

  let nextZoneIndex = 0;

  new PickingSystem(scene, camera, canvas, bimIndex, (info) => {
    if (!info?.element && !info?.meshName) return;

    if (info.element?.entity === 'media' || info.element?.isMediaScreen) {
      openMediaPlayback();
      return;
    }

    if (info.meshName && media.isScreenMesh({ metadata: info.element })) {
      openMediaPlayback();
      return;
    }

    if (!info?.element) return;
    const el = info.element;
    const props = el.properties
      ? Object.entries(el.properties)
          .map(([k, v]) => `${k}: ${v}`)
          .join(', ')
      : '';
    notify(
      `${el.name || el.globalId}${el.category ? ` · ${el.category}` : ''}${
        props ? ` · ${props}` : ''
      }`,
      2800
    );
  });

  function resolveInput() {
    const desktop = input.getState();
    if (xr.isInXR) {
      const xrMove = xr.getMoveState();
      return {
        forward: xrMove.forward,
        backward: xrMove.backward,
        left: xrMove.left,
        right: xrMove.right,
        moveX: xrMove.moveX ?? 0,
        moveZ: xrMove.moveZ ?? 0,
        interact: xrMove.interact || desktop.interact,
        drop: xrMove.drop || desktop.drop,
        jump: xrMove.jump || desktop.jump,
        rise: xrMove.rise,
        explode: xrMove.explode || desktop.explode,
        lookDeltaX: 0,
        lookDeltaY: 0,
        skipHorizontal: true,
      };
    }
    return { ...desktop, rise: false, skipHorizontal: false };
  }

  function tryDeliveries() {
    if (!achievements.completed) return;
    if (nextZoneIndex >= zones.length) return;
    if (sphere.isHeld()) return;
    if (!sphere.grounded) return;

    const zone = zones[nextZoneIndex];
    const ballPos = sphere.getPosition();
    if (zone.tryDeliver(ballPos, sphere.isHeld())) {
      const stepId = `ball_zone_${nextZoneIndex + 1}`;
      circuit.complete(stepId, `${zone.label} completado`);
      nextZoneIndex += 1;
      if (nextZoneIndex < zones.length) {
        notify(`Siguiente: ${zones[nextZoneIndex].label}`);
      }
    }
  }

  function tryXrMediaPick(interactEdge) {
    if (!interactEdge || !xr.isInXR) return;
    const hit = xr.pickWithPointer(14);
    const mesh = hit?.pickedMesh;
    if (mesh && media.isScreenMesh(mesh)) {
      openMediaPlayback();
    }
  }

  sceneManager.setUpdateCallback((delta) => {
    const state = resolveInput();

    if (xr.isInXR) {
      xr.update(delta);
    }

    if (!xr.isInXR && (state.lookDeltaX || state.lookDeltaY)) {
      cameraRig.addLook(state.lookDeltaX, state.lookDeltaY);
    }

    // XR: capsule XZ under headset; Y from jump/platform physics (not headset)
    if (xr.isInXR) {
      const viewer = xr.getViewerPosition();
      if (viewer) {
        player.setWorldXZ(viewer.x, viewer.z);
      }
      const wasGrounded = player.grounded;
      player.update(delta, {
        ...state,
        skipHorizontal: true,
        faceYaw: 0,
      });
      xr.setRigFeetY(player.getFeetY());
      if (state.jump && wasGrounded) {
        notify('¡Salto!', 800);
      }
    } else {
      player.update(delta, {
        ...state,
        faceYaw: cameraRig.getFaceYaw(),
      });
    }

    const playerPos = player.getPosition();
    const nearWalls = isNearXZ(
      playerPos,
      walls.getInteractionPoint(),
      levelConfig.walls.promptRadius ?? 5.5
    );
    hud.setWallActionsVisible(nearWalls);
    xrHud.setWallActionsVisible(nearWalls);

    if (nearWalls && state.rise) {
      triggerRiseWalls();
    }
    if (nearWalls && state.explode) {
      triggerExplodeWalls();
    }

    if (state.drop && player.heldObject) {
      player.drop();
      notify('Pelota soltada — cae al suelo');
    }

    tryXrMediaPick(state.interact);

    sphere.update(delta);

    if (player.touchedJumpPlatform) {
      circuit.complete('jump_high', 'Plataforma alta alcanzada');
    }

    for (const coin of coins) coin.update(delta);

    if (walls.update(delta)) {
      circuit.complete('walls_rise', 'Levantamiento de muros finalizado');
      hud.markRiseDone();
      xrHud.markRiseDone();
    }

    interactions.update(playerPos, { interact: state.interact });

    tryDeliveries();

    if (!xr.isInXR) {
      cameraRig.update(delta, player.getPosition());
    }
  });

  sceneManager.start();
  console.info('[Metaverso MVP] Circuit mode — meters, PBR, WebXR ready.');
}

boot().catch((err) => {
  console.error('[main] Boot failed:', err);
});
