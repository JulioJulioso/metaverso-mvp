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
import { HUD } from './ui/HUD.js';

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
    movementSpeed: 0.4,
    rotationSpeed: 0.3,
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
  const achievements = new AchievementSystem(coins.length, {
    onAllCollected: () => {
      circuit.complete('coins_all', '¡Todos los marcadores recogidos!');
      hudRef?.showMessage(
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
      hudRef?.showMessage('Levantamiento de muros en curso…');
      return true;
    }
    if (walls.isRiseDone()) {
      hudRef?.showMessage('Los muros ya están levantados');
    } else if (walls.isRiseStarted()) {
      hudRef?.showMessage('Levantamiento ya en curso…');
    }
    return false;
  }

  function triggerExplodeWalls() {
    const started = walls.toggleExplode();
    if (started) {
      const opening = walls.getExplodeTarget() > 0.5;
      hudRef?.showMessage(
        opening ? 'Despiece de muros (visión explotada)' : 'Muros reensamblados'
      );
      if (opening) {
        circuit.complete('walls_explode', 'Visión explotada activada');
      }
      return true;
    }
    hudRef?.showMessage('Primero levanta los muros (botón o R)');
    return false;
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
    }
  );
  hudRef = hud;
  hud.updateCoins(achievements.getState());
  achievements.onChange((state) => hud.updateCoins(state));
  circuit.onChange((snap) => hud.updateCircuit(snap));

  xr.onSessionChange((active) => {
    cameraRig.setEnabled(!active);
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
      if (player.pickUp(sphere)) {
        hud.showMessage('Pelota recogida — llévala a los sitios de entrega (F soltar)');
      }
    },
  });

  let nextZoneIndex = 0;

  new PickingSystem(scene, camera, canvas, bimIndex, (info) => {
    if (!info?.element && !info?.meshName) return;

    // Click on 3D media unit → open YouTube overlay
    if (info.element?.entity === 'media' || info.element?.isMediaScreen) {
      hud.openVideo({
        url: media.getEmbedUrl(true),
        title: media.getTitle(),
      });
      return;
    }

    // Fallback: inspect mesh name from raw pick via bim / label
    if (info.meshName && media.isScreenMesh({ metadata: info.element })) {
      hud.openVideo({
        url: media.getEmbedUrl(true),
        title: media.getTitle(),
      });
      return;
    }

    if (!info?.element) return;
    const el = info.element;
    const props = el.properties
      ? Object.entries(el.properties)
          .map(([k, v]) => `${k}: ${v}`)
          .join(', ')
      : '';
    hud.showMessage(
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
        drop: desktop.drop,
        jump: desktop.jump,
        explode: desktop.explode,
        riseWalls: desktop.riseWalls,
        lookDeltaX: 0,
        lookDeltaY: 0,
        // When Babylon MOVEMENT drives the XR camera, don't also translate Player via sticks
        skipHorizontal: xr.usesNativeMovement(),
      };
    }
    return { ...desktop, skipHorizontal: false };
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
        hud.showMessage(`Siguiente: ${zones[nextZoneIndex].label}`);
      }
    }
  }

  sceneManager.setUpdateCallback((delta) => {
    const state = resolveInput();

    if (!xr.isInXR && (state.lookDeltaX || state.lookDeltaY)) {
      cameraRig.addLook(state.lookDeltaX, state.lookDeltaY);
    }

    if (state.riseWalls) {
      triggerRiseWalls();
    }

    if (state.explode) {
      triggerExplodeWalls();
    }

    if (state.drop && player.heldObject) {
      player.drop();
      hud.showMessage('Pelota soltada — cae al suelo');
    }

    player.update(delta, {
      ...state,
      faceYaw: xr.isInXR ? 0 : cameraRig.getFaceYaw(),
    });

    // Keep gameplay capsule under the headset when XR MOVEMENT moves the camera
    if (xr.isInXR && xr.usesNativeMovement()) {
      const viewer = xr.getViewerPosition();
      if (viewer) {
        player.setWorldPosition({
          x: viewer.x,
          y: viewer.y - levelConfig.player.height * 0.35,
          z: viewer.z,
        });
      }
    }

    sphere.update(delta);

    if (player.touchedJumpPlatform) {
      circuit.complete('jump_high', 'Plataforma alta alcanzada');
    }

    for (const coin of coins) coin.update(delta);

    if (walls.update(delta)) {
      circuit.complete('walls_rise', 'Levantamiento de muros finalizado');
      hud.markRiseDone();
    }

    const playerPos = player.getPosition();
    interactions.update(playerPos, { interact: state.interact });

    tryDeliveries();

    if (!xr.isInXR) {
      cameraRig.update(delta, playerPos);
    }
  });

  sceneManager.start();
  console.info('[Metaverso MVP] Circuit mode — meters, PBR, WebXR ready.');
}

boot().catch((err) => {
  console.error('[main] Boot failed:', err);
});
