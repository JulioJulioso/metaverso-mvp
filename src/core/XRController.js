import {
  WebXRState,
  Vector3,
  Axis,
  Quaternion,
  MeshBuilder,
  StandardMaterial,
  Color3,
  Ray,
} from '@babylonjs/core';

/**
 * WebXR session + Quest locomotion, buttons, hand attach, laser pick.
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
    /** @type {Record<string, boolean>} */
    this._prevButtons = {
      interact: false,
      jump: false,
      drop: false,
      rise: false,
      explode: false,
    };
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

  /** @returns {import('@babylonjs/core').WebXRCamera|null} */
  getXRCamera() {
    return this.xrHelper?.baseExperience?.camera ?? null;
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
        optionalFeatures: true,
      });

      const base = this.xrHelper.baseExperience;
      base.onStateChangedObservable.add((state) => {
        this.isInXR = state === WebXRState.IN_XR;
        if (this.isInXR) {
          console.info('[XRController] Entered immersive-vr');
        } else {
          this._clearFallbackGrips();
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

      // Only spawn fallback if model fails — avoids black box at world origin
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
        }, 2500);
      });
    });

    this.xrHelper.input.onControllerRemovedObservable?.add((controller) => {
      this._disposeFallbackGrip(controller.uniqueId);
    });
  }

  /**
   * @param {import('@babylonjs/core').WebXRInputSource} controller
   */
  _ensureFallbackGrip(controller) {
    const id = controller.uniqueId;
    if (this._fallbackGrips.has(id)) return;
    const parent = controller.grip || controller.pointer || null;
    if (!parent) return;

    const grip = MeshBuilder.CreateBox(
      `xrGrip_${id}`,
      { width: 0.035, height: 0.07, depth: 0.14 },
      this.scene
    );
    const mat = new StandardMaterial(`xrGripMat_${id}`, this.scene);
    mat.diffuseColor = new Color3(0.15, 0.15, 0.18);
    mat.emissiveColor = new Color3(0.05, 0.08, 0.12);
    grip.material = mat;
    grip.isPickable = false;
    grip.parent = parent;
    grip.position = new Vector3(0, 0, 0.02);

    this._fallbackGrips.set(id, grip);
  }

  _disposeFallbackGrip(id) {
    const mesh = this._fallbackGrips.get(id);
    if (mesh) {
      mesh.dispose();
      this._fallbackGrips.delete(id);
    }
  }

  _clearFallbackGrips() {
    for (const id of [...this._fallbackGrips.keys()]) {
      this._disposeFallbackGrip(id);
    }
  }

  /**
   * Prefer right grip for held objects (natural hand pose).
   * @returns {import('@babylonjs/core').TransformNode|null}
   */
  getPrimaryHandNode() {
    if (!this.isInXR || !this.xrHelper) return null;
    const controllers = this.xrHelper.input?.controllers ?? [];
    const right = controllers.find((c) => c.inputSource?.handedness === 'right');
    const left = controllers.find((c) => c.inputSource?.handedness === 'left');
    const pick = right || left || controllers[0];
    if (!pick) return null;
    return pick.grip || pick.pointer || null;
  }

  /**
   * World-space ray matching Babylon's laser.
   * @returns {{ origin: Vector3, direction: Vector3, controller: object }|null}
   */
  getPointerRay() {
    if (!this.isInXR || !this.xrHelper) return null;
    const controllers = this.xrHelper.input?.controllers ?? [];
    const ordered = [
      ...controllers.filter((c) => c.inputSource?.handedness === 'right'),
      ...controllers.filter((c) => c.inputSource?.handedness === 'left'),
      ...controllers,
    ];
    for (const ctrl of ordered) {
      if (typeof ctrl.getWorldPointerRayToRef === 'function') {
        const ray = new Ray(Vector3.Zero(), Vector3.Forward(), 1);
        ctrl.getWorldPointerRayToRef(ray);
        return {
          origin: ray.origin.clone(),
          direction: ray.direction.clone().normalize(),
          controller: ctrl,
        };
      }
      const pointer = ctrl.pointer;
      if (!pointer) continue;
      const origin = pointer.getAbsolutePosition().clone();
      let direction = pointer.getDirection
        ? pointer.getDirection(Axis.Z)
        : new Vector3(0, 0, 1);
      if (direction.lengthSquared() < 1e-6) direction = new Vector3(0, 0, 1);
      else direction.normalize();
      return { origin, direction, controller: ctrl };
    }
    return null;
  }

  /**
   * @param {number} [maxDistance]
   * @returns {import('@babylonjs/core').PickingInfo|null}
   */
  pickWithPointer(maxDistance = 12) {
    const rayInfo = this.getPointerRay();
    if (!rayInfo) return null;
    const ray = new Ray(rayInfo.origin, rayInfo.direction, maxDistance);
    return this.scene.pickWithRay(ray) || null;
  }

  /**
   * @param {number} feetY world Y of player feet
   */
  setRigFeetY(feetY) {
    if (!this.isInXR || !this.xrHelper) return;
    const cam = this.xrHelper.baseExperience.camera;
    const parent = cam?.cameraRigParent;
    if (parent) {
      parent.position.y = feetY;
    }
  }

  _componentPressed(mc, ids) {
    if (!mc) return false;
    for (const id of ids) {
      const c = mc.getComponent(id);
      if (c && (c.pressed || (c.value ?? 0) > 0.65)) return true;
    }
    return false;
  }

  _gamepadButton(gp, index) {
    const b = gp?.buttons?.[index];
    return !!(b && (b.pressed || (b.value ?? 0) > 0.65));
  }

  _readButtonsHeld() {
    const out = {
      moveX: 0,
      moveZ: 0,
      turnX: 0,
      interact: false,
      jump: false,
      drop: false,
      rise: false,
      explode: false,
    };
    if (!this.isInXR || !this.xrHelper) return out;

    const controllers = this.xrHelper.input?.controllers ?? [];
    for (const ctrl of controllers) {
      const handed = ctrl.inputSource?.handedness;
      const mc = ctrl.motionController;
      const gp = ctrl.inputSource?.gamepad;
      let ax = 0;
      let ay = 0;
      let got = false;

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
          if (stick.pressed) out.jump = true;
        }

        if (this._componentPressed(mc, ['xr-standard-trigger', 'trigger'])) {
          out.interact = true;
        }
        if (this._componentPressed(mc, ['xr-standard-squeeze', 'squeeze'])) {
          out.drop = true;
        }

        if (handed === 'right') {
          if (
            this._componentPressed(mc, [
              'a-button',
              'xr-standard-button-a',
              'button-a',
            ])
          ) {
            out.jump = true;
          }
          if (
            this._componentPressed(mc, [
              'b-button',
              'xr-standard-button-b',
              'button-b',
            ])
          ) {
            out.drop = true;
          }
        } else {
          if (
            this._componentPressed(mc, [
              'x-button',
              'xr-standard-button-x',
              'button-x',
            ])
          ) {
            out.explode = true;
          }
          if (
            this._componentPressed(mc, [
              'y-button',
              'xr-standard-button-y',
              'button-y',
            ])
          ) {
            out.rise = true;
          }
        }
      }

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
        if (this._gamepadButton(gp, 0)) out.interact = true;
        if (this._gamepadButton(gp, 1)) out.drop = true;
        if (this._gamepadButton(gp, 3)) out.jump = true;
        if (handed === 'right') {
          if (this._gamepadButton(gp, 4)) out.jump = true;
          if (this._gamepadButton(gp, 5)) out.drop = true;
        } else {
          if (this._gamepadButton(gp, 4)) out.explode = true;
          if (this._gamepadButton(gp, 5)) out.rise = true;
        }
      }

      if (got) {
        if (handed === 'right') {
          out.turnX += ax;
        } else {
          // Invert stick Y vs previous mapping (Quest push-forward was reversed)
          out.moveX += ax;
          out.moveZ += ay;
        }
      }
    }

    return out;
  }

  getMoveState() {
    const held = this._readButtonsHeld();
    const dead = 0.18;
    const edge = (key, now) => {
      const was = this._prevButtons[key];
      this._prevButtons[key] = now;
      return now && !was;
    };

    return {
      forward: held.moveZ > dead,
      backward: held.moveZ < -dead,
      left: held.moveX < -dead,
      right: held.moveX > dead,
      moveX: held.moveX,
      moveZ: held.moveZ,
      turnX: held.turnX,
      interactHeld: held.interact,
      interact: edge('interact', held.interact),
      jump: edge('jump', held.jump),
      drop: edge('drop', held.drop),
      rise: edge('rise', held.rise),
      explode: edge('explode', held.explode),
    };
  }

  getThumbstickAxes() {
    const s = this.getMoveState();
    return {
      moveX: s.moveX,
      moveZ: s.moveZ,
      turnX: s.turnX,
      interact: s.interactHeld,
    };
  }

  /**
   * @param {number} delta seconds
   */
  update(delta) {
    if (!this.isInXR || !this.xrHelper) return;

    const axes = this._readButtonsHeld();
    const dead = 0.18;
    const speed = this.options.movementSpeed ?? 2.8;
    const turnSpeed = this.options.rotationSpeed ?? 1.6;

    const cam = this.xrHelper.baseExperience.camera;
    if (!cam) return;

    const mx = Math.abs(axes.moveX) > dead ? axes.moveX : 0;
    const mz = Math.abs(axes.moveZ) > dead ? axes.moveZ : 0;
    const tx = Math.abs(axes.turnX) > dead ? axes.turnX : 0;

    if (mx !== 0 || mz !== 0) {
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

      const parent = cam.cameraRigParent;
      if (parent) {
        parent.position.x += offset.x;
        parent.position.z += offset.z;
      } else {
        cam.position.addInPlace(offset);
      }
    }

    if (tx !== 0) {
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

  getViewerPosition() {
    if (!this.isInXR || !this.xrHelper) return null;
    const cam = this.xrHelper.baseExperience.camera;
    if (!cam) return null;
    return { x: cam.position.x, y: cam.position.y, z: cam.position.z };
  }

  usesNativeMovement() {
    return false;
  }

  dispose() {
    this._clearFallbackGrips();
    this._listeners.clear();
  }
}

export default XRController;
