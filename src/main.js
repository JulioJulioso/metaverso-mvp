import { SceneManager } from './core/SceneManager.js';
import { InputController } from './core/InputController.js';
import { XRController } from './core/XRController.js';
import { levelConfig } from './config/levelConfig.js';
import { Player } from './entities/Player.js';
import { Coin } from './entities/Coin.js';
import { PickupSphere } from './entities/PickupSphere.js';
import { Platform } from './entities/Platform.js';
import { InteractionSystem } from './systems/InteractionSystem.js';
import { AchievementSystem } from './systems/AchievementSystem.js';
import { CameraRigSystem } from './systems/CameraRigSystem.js';
import { BimIndexStub } from './systems/BimIndexStub.js';
import { PickingSystem } from './systems/PickingSystem.js';
import { NetworkStub } from './systems/NetworkStub.js';
import { HUD } from './ui/HUD.js';

// Register glTF loaders for future AssetLoader use
import '@babylonjs/loaders/glTF';

async function boot() {
  const canvas = document.getElementById('renderCanvas');
  const hudRoot = document.getElementById('hud-root');
  if (!canvas || !hudRoot) {
    console.error('[main] Missing #renderCanvas or #hud-root');
    return;
  }

  // Presence stub imported so future systems can couple to the same interface
  void NetworkStub;

  const sceneManager = new SceneManager(canvas, {
    useLargeWorldRendering: false,
  });
  const scene = sceneManager.getScene();
  const camera = sceneManager.getCamera();

  const input = new InputController();
  const xr = new XRController(scene);
  await xr.init();

  const platforms = levelConfig.platforms.map((p) => new Platform(scene, p));
  const coins = levelConfig.coins.map((c) => new Coin(scene, c));
  const sphere = new PickupSphere(scene, levelConfig.pickupSphere);
  const player = new Player(
    scene,
    levelConfig.player,
    platforms,
    levelConfig.groundY
  );

  const achievements = new AchievementSystem(coins.length);
  const interactions = new InteractionSystem();
  const cameraRig = new CameraRigSystem(camera);
  const bimIndex = new BimIndexStub();
  bimIndex.loadMock(levelConfig.bimMockElements);

  const hud = new HUD(hudRoot);
  hud.updateCoins(achievements.getState());
  achievements.onChange((state) => hud.updateCoins(state));

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
        hud.showMessage('Objeto recogido (E/F o trigger XR)');
      }
    },
  });

  new PickingSystem(scene, camera, canvas, bimIndex, (info) => {
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

  /** Merge desktop axes with action edges; in XR movement from stick, E still from keyboard. */
  function resolveInput() {
    const desktop = input.getState();
    if (xr.isInXR) {
      const xrMove = xr.getMoveState();
      return {
        forward: xrMove.forward,
        backward: xrMove.backward,
        left: xrMove.left,
        right: xrMove.right,
        interact: xrMove.interact || desktop.interact,
        drop: desktop.drop,
      };
    }
    return desktop;
  }

  sceneManager.setUpdateCallback((delta) => {
    const state = resolveInput();

    if (state.drop && player.heldObject) {
      player.drop();
      hud.showMessage('Objeto soltado');
    }

    player.update(delta, state);

    for (const coin of coins) coin.update(delta);

    const playerPos = player.getPosition();
    interactions.update(playerPos, {
      interact: state.interact,
    });

    // Interact also tries pick up if near and free, handled by InteractionSystem

    if (!xr.isInXR) {
      cameraRig.update(delta, playerPos);
    }
  });

  sceneManager.start();
  console.info(
    '[Metaverso MVP] Running. Units: meters. AEC pipeline docs: docs/AEC_PIPELINE.md'
  );
}

boot().catch((err) => {
  console.error('[main] Boot failed:', err);
});
