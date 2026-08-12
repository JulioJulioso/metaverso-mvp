import {
  WebXRState,
  Vector3,
  Axis,
  Quaternion,
  MeshBuilder,
  StandardMaterial,
  Color3,
} from '@babylonjs/core';

/**
 * Isolated WebXR session + Quest-friendly locomotion.
 *
 * - Moves the XR camera (not only the Player mesh).
 * - Reads Quest thumbsticks via motionController + gamepad axes fallbacks.
 * - Keeps pointer lasers; adds fallback grip meshes if controller models fail to load.
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
    this._listeners = new Set();
    /** @type {Map<string, import('@babylonjs/core').Mesh>} */
    this._fallbackGrips = new Map();
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
        floorMeshes: this.options.floorMeshes ?? [],
        inputOptions: {
          doNotLoadControllerMeshes: false,
        },
      });

      const base = this.xrHelper.baseExperience;
      base.onStateChangedObservable.add((state) => {
        this.isInXR = state === WebXRState.IN_XR;
        if (this.isInXR) {
          console.info('[XRController] Entered immersive-vr');
        }
        this._emit();
      });

      this._setupControllerVisuals();
      return this.xrHelper;
    } catch (err) {
      console.warn('[XRController] Failed to start XR experience:', err);
      this.xrHelper = null;
      return null;
    }
  }

  _setupControllerVisuals() {
    if (!this.xrHelper) return;

    this.xrHelper.input.onControllerAddedObservable.add((controller) => {
      console.info(
        '[XRController] Controller added:',
        controller.uniqueId,
        controller.inputSource?.handedness
      );

      // Immediate visible grip; replaced if the official model loads
      this._ensureFallbackGrip(controller);

      controller.onMotionControllerInitObservable.add((motionController) => {
        motionController.onModelLoadedObservable.add(() => {
          console.info('[XRController] Controller model loaded:', controller.uniqueId);
          this._disposeFallbackGrip(controller.uniqueId);
        });

        window.setTimeout(() => {
          if (!this.isInXR) return;
          const root = motionController.rootMesh;
          const hasChildren = !!(root && root.getChildMeshes(true).length > 0);
          if (!hasChildren) {
            this._ensureFallbackGrip(controller);
          }
        }, 2000);
      });
    });
  }

  /**
   * @param {import('@babylonjs/core').WebXRInputSource} controller
   */
  _ensureFallbackGrip(controller) {
    const id = controller.uniqueId;
    if (this._fallbackGrips.has(id)) return;

    const grip = MeshBuilder.CreateBox(
      `xrGrip_${id}`,
      { width: 0.04, height: 0.08, depth: 0.12 },
      this.scene
    );
    const mat = new StandardMaterial(`xrGripMat_${id}`, this.scene);
    mat.diffuseColor = new Color3(0.15, 0.15, 0.18);
    mat.emissiveColor = new Color3(0.05, 0.08, 0.12);
    grip.material = mat;
    grip.isPickable = false;

    const parent = controller.grip || controller.pointer || null;
    if (parent) {
      grip.parent = parent;
      grip.position = new Vector3(0, 0, 0.02);
    }

    this._fallbackGrips.set(id, grip);
    console.info('[XRController] Fallback grip mesh created for', id);
  }

  _disposeFallbackGrip(id) {
    const mesh = this._fallbackGrips.get(id);
    if (mesh) {
      mesh.dispose();
      this._fallbackGrips.delete(id);
    }
  }

  /**
   * @returns {{ moveX: number, moveZ: number, turnX: number, interact: boolean }}
   */
  getThumbstickAxes() {
    const empty = { moveX: 0, moveZ: 0, turnX: 0, interact: false };
    if (!this.isInXR || !this.xrHelper) return empty;

    const controllers = this.xrHelper.input?.controllers ?? [];
    let moveX = 0;
    let moveZ = 0;
    let turnX = 0;
    let interact = false;

    for (const ctrl of controllers) {
      const handed = ctrl.inputSource?.handedness;
      let ax = 0;
      let ay = 0;
      let got = false;

      const mc = ctrl.motionController;
      if (mc) {
        const stick =
          mc.getComponent('xr-standard-thumbstick') ||
          mc.getComponent('thumbstick') ||
          mc.getComponent('xr-standard-touchpad') ||
          mc.getComponent('touchpad');
        if (stick) {
          ax = stick.axes?.x ?? 0;
          ay = stick.axes?.y ?? 0;
          got = true;
        }

        const trigger =
          mc.getComponent('xr-standard-trigger') || mc.getComponent('trigger');
        if (trigger && (trigger.pressed || (trigger.value ?? 0) > 0.65)) {
          interact = true;
        }
        const squeeze =
          mc.getComponent('xr-standard-squeeze') || mc.getComponent('squeeze');
        if (squeeze && (squeeze.pressed || (squeeze.value ?? 0) > 0.65)) {
          interact = true;
        }
      }

      const gp = ctrl.inputSource?.gamepad;
      if (gp?.axes?.length) {
        if (gp.axes.length >= 4) {
          const gx = gp.axes[2] ?? 0;
          const gy = gp.axes[3] ?? 0;
          if (Math.abs(gx) + Math.abs(gy) >= Math.abs(ax) + Math.abs(ay)) {
            ax = gx;
            ay = gy;
            got = true;
          }
        } else if (gp.axes.length >= 2 && !got) {
          ax = gp.axes[0] ?? 0;
          ay = gp.axes[1] ?? 0;
          got = true;
        }
      }

      if (!got) continue;

      if (handed === 'right') {
        turnX += ax;
      } else {
        moveX += ax;
        moveZ += -ay;
      }
    }

    return { moveX, moveZ, turnX, interact };
  }

  /**
   * Apply stick locomotion to the XR camera every frame while in VR.
   * @param {number} delta seconds
   */
  update(delta) {
    if (!this.isInXR || !this.xrHelper) return;

    const axes = this.getThumbstickAxes();
    const dead = 0.18;
    const speed = this.options.movementSpeed ?? 2.8;
    const turnSpeed = this.options.rotationSpeed ?? 1.6;

    const cam = this.xrHelper.baseExperience.camera;
    if (!cam) return;

    const mx = Math.abs(axes.moveX) > dead ? axes.moveX : 0;
    const mz = Math.abs(axes.moveZ) > dead ? axes.moveZ : 0;
    const tx = Math.abs(axes.turnX) > dead ? axes.turnX : 0;

    if (mx !== 0 || mz !== 0) {
      // Prefer ray forward (works for WebXRCamera on Quest)
      let forward = cam.getForwardRay?.(1)?.direction?.clone?.();
      if (!forward) {
        forward = cam.getDirection(Axis.Z);
      }
      forward.y = 0;
      if (forward.lengthSquared() > 1e-6) forward.normalize();
      else forward.copyFromFloats(0, 0, 1);

      const right = Vector3.Cross(Vector3.Up(), forward);
      if (right.lengthSquared() > 1e-6) right.normalize();
      else right.copyFromFloats(1, 0, 0);

      const step = speed * delta;
      const offset = right.scale(mx * step).add(forward.scale(mz * step));

      // Must move the rig parent; XR overwrites camera.position from headset pose
      const parent = cam.cameraRigParent;
      if (parent) {
        parent.position.addInPlace(offset);
      } else {
        cam.position.addInPlace(offset);
        console.warn('[XRController] No cameraRigParent — locomotion may reset');
      }
    }

    if (tx !== 0) {
      // Rotate the rig parent — headset pose overwrites camera.rotation each frame
      const yaw = tx * turnSpeed * delta;
      const parent = cam.cameraRigParent;
      const target = parent || cam;
      if (target.rotationQuaternion) {
        const deltaQ = Quaternion.RotationAxis(Axis.Y, -yaw);
        target.rotationQuaternion = deltaQ.multiply(target.rotationQuaternion);
      } else {
        target.rotation.y -= yaw;
      }
    }
  }

  /**
   * @returns {{ x:number, y:number, z:number }|null}
   */
  getViewerPosition() {
    if (!this.isInXR || !this.xrHelper) return null;
    const cam = this.xrHelper.baseExperience.camera;
    if (!cam) return null;
    return { x: cam.position.x, y: cam.position.y, z: cam.position.z };
  }

  getMoveState() {
    const axes = this.getThumbstickAxes();
    const dead = 0.18;
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

  /** Manual XR camera locomotion is always used. */
  usesNativeMovement() {
    return false;
  }

  dispose() {
    for (const mesh of this._fallbackGrips.values()) mesh.dispose();
    this._fallbackGrips.clear();
    this._listeners.clear();
  }
}

export default XRController;
