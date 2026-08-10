/**
 * WebXR immersive-vr via Babylon default experience.
 * Basic teleport/comfort omitted — enter VR + thumbstick move mapping for MVP.
 */
export class XRController {
  /**
   * @param {import('@babylonjs/core').Scene} scene
   */
  constructor(scene) {
    this.scene = scene;
    this.isInXR = false;
    this.xrHelper = null;
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
        disableTeleportation: true,
      });

      const base = this.xrHelper.baseExperience;
      base.onStateChangedObservable.add((state) => {
        // XRState: 0 ENTERING_XR, 1 IN_XR, 2 EXITING_XR, 3 NOT_IN_XR
        this.isInXR = state === 1;
        this._emit();
      });

      return this.xrHelper;
    } catch (err) {
      console.warn('[XRController] Failed to start XR experience:', err);
      this.xrHelper = null;
      return null;
    }
  }

  /**
   * Normalized move state from first available XR thumbstick (same shape as InputController).
   * @returns {{ forward: boolean, backward: boolean, left: boolean, right: boolean, interact: boolean, drop: boolean }}
   */
  getMoveState() {
    const empty = {
      forward: false,
      backward: false,
      left: false,
      right: false,
      interact: false,
      drop: false,
    };

    if (!this.isInXR || !this.xrHelper) return empty;

    const controllers = this.xrHelper.input?.controllers ?? [];
    let ax = 0;
    let ay = 0;
    let interact = false;

    for (const ctrl of controllers) {
      const mc = ctrl.motionController;
      if (!mc) continue;

      const stick =
        mc.getComponent('xr-standard-thumbstick') ||
        mc.getComponent('thumbstick');
      if (stick) {
        ax += stick.axes?.x ?? 0;
        ay += stick.axes?.y ?? 0;
      }

      const trigger =
        mc.getComponent('xr-standard-trigger') || mc.getComponent('trigger');
      if (trigger?.pressed) interact = true;
    }

    const dead = 0.25;
    return {
      forward: ay < -dead,
      backward: ay > dead,
      left: ax < -dead,
      right: ax > dead,
      interact,
      drop: false,
    };
  }

  dispose() {
    this._listeners.clear();
    // XR helper disposed with scene
  }
}

export default XRController;
