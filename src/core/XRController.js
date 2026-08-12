import {
  WebXRFeatureName,
  WebXRState,
} from '@babylonjs/core';

/**
 * Isolated WebXR session + locomotion.
 *
 * Desktop WASD is handled elsewhere. In VR, moving only the Player mesh does NOT
 * move the headset camera — Babylon owns the XR camera. Industry approach:
 * enable WebXR MOVEMENT feature (thumbstick → XR camera), then sync Player to it.
 */
export class XRController {
  /**
   * @param {import('@babylonjs/core').Scene} scene
   * @param {{
   *   floorMeshes?: import('@babylonjs/core').AbstractMesh[],
   *   movementSpeed?: number,
   *   rotationSpeed?: number,
   * }} [options]
   */
  constructor(scene, options = {}) {
    this.scene = scene;
    this.options = options;
    this.isInXR = false;
    this.xrHelper = null;
    this.movementFeature = null;
    this._listeners = new Set();
  }

  /**
   * @param {(active: boolean) => void} cb
   */
  onSessionChange(cb) {
    this._listeners.add(cb);
    return () => this._listeners.delete(cb);
  }

  _emit() {
    for (const cb of this._listeners) cb(this.isInXR);
  }

  async init() {
    if (!navigator.xr) {
      console.warn('[XRController] WebXR not available in this browser.');
      return null;
    }

    try {
      this.xrHelper = await this.scene.createDefaultXRExperienceAsync({
        uiOptions: {
          sessionMode: 'immersive-vr',
        },
        // Prefer stick locomotion over teleport for this MVP
        disableTeleportation: true,
        floorMeshes: this.options.floorMeshes ?? [],
      });

      const base = this.xrHelper.baseExperience;
      base.onStateChangedObservable.add((state) => {
        this.isInXR = state === WebXRState.IN_XR;
        this._emit();
      });

      this._enableMovementFeature();

      return this.xrHelper;
    } catch (err) {
      console.warn('[XRController] Failed to start XR experience:', err);
      this.xrHelper = null;
      return null;
    }
  }

  _enableMovementFeature() {
    if (!this.xrHelper) return;

    try {
      const fm = this.xrHelper.baseExperience.featuresManager;
      // Disable conflicting features if present
      try {
        fm.disableFeature(WebXRFeatureName.TELEPORTATION);
      } catch {
        /* optional */
      }

      this.movementFeature = fm.enableFeature(
        WebXRFeatureName.MOVEMENT,
        'latest',
        {
          xrInput: this.xrHelper.input,
          // Stick forward follows headset facing (industry default)
          movementOrientationFollowsViewerPose: true,
          movementSpeed: this.options.movementSpeed ?? 0.35,
          rotationSpeed: this.options.rotationSpeed ?? 0.25,
        }
      );
      console.info('[XRController] WebXR MOVEMENT feature enabled (thumbstick locomotion).');
    } catch (err) {
      console.warn(
        '[XRController] MOVEMENT feature unavailable — falling back to manual stick read:',
        err
      );
      this.movementFeature = null;
    }
  }

  /**
   * XR camera world position (viewer). Used to keep Player synced for gameplay.
   * @returns {{ x:number, y:number, z:number }|null}
   */
  getViewerPosition() {
    if (!this.isInXR || !this.xrHelper) return null;
    const cam = this.xrHelper.baseExperience.camera;
    if (!cam) return null;
    return {
      x: cam.position.x,
      y: cam.position.y,
      z: cam.position.z,
    };
  }

  /**
   * Manual stick axes fallback if MOVEMENT feature failed.
   * Prefer Babylon MOVEMENT; this is only a backup for Player-driven move.
   * @returns {{ moveX: number, moveZ: number, interact: boolean, available: boolean }}
   */
  getThumbstickAxes() {
    const empty = { moveX: 0, moveZ: 0, interact: false, available: false };
    if (!this.isInXR || !this.xrHelper) return empty;
    // When Babylon MOVEMENT is active, don't also move the player via sticks
    if (this.movementFeature) {
      return { ...empty, available: true };
    }

    const controllers = this.xrHelper.input?.controllers ?? [];
    let ax = 0;
    let ay = 0;
    let interact = false;
    let found = false;

    for (const ctrl of controllers) {
      const mc = ctrl.motionController;
      if (!mc) continue;

      const stick =
        mc.getComponent('xr-standard-thumbstick') ||
        mc.getComponent('thumbstick') ||
        mc.getComponent('xr-standard-touchpad');
      if (stick) {
        // Babylon WebXRControllerComponent: axes.x / axes.y
        const sx = stick.axes?.x ?? stick.axisValues?.[0] ?? 0;
        const sy = stick.axes?.y ?? stick.axisValues?.[1] ?? 0;
        ax += sx;
        ay += sy;
        found = true;
      }

      const trigger =
        mc.getComponent('xr-standard-trigger') || mc.getComponent('trigger');
      if (trigger?.pressed || trigger?.value > 0.7) interact = true;
    }

    // Quest: typically Y- = forward
    return {
      moveX: ax,
      moveZ: -ay,
      interact,
      available: found,
    };
  }

  /**
   * Legacy boolean shape (kept for callers). Prefer getThumbstickAxes / MOVEMENT feature.
   */
  getMoveState() {
    const axes = this.getThumbstickAxes();
    const dead = 0.25;
    return {
      forward: axes.moveZ > dead,
      backward: axes.moveZ < -dead,
      left: axes.moveX < -dead,
      right: axes.moveX > dead,
      interact: axes.interact,
      drop: false,
      moveX: axes.moveX,
      moveZ: axes.moveZ,
    };
  }

  usesNativeMovement() {
    return !!this.movementFeature;
  }

  dispose() {
    this._listeners.clear();
  }
}

export default XRController;
