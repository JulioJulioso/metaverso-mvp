import { Vector3 } from '@babylonjs/core';
import { getLookBasisXZ } from './LocomotionMath.js';

/**
 * Third-person orbit camera (yaw/pitch). Disabled in XR.
 * Movement facing comes from getFaceYaw() → LocomotionMath.
 */
export class CameraRigSystem {
  /**
   * @param {import('@babylonjs/core').UniversalCamera|import('@babylonjs/core').Camera} camera
   * @param {{
   *   distance?: number,
   *   height?: number,
   *   lerp?: number,
   *   minPitch?: number,
   *   maxPitch?: number,
   *   initialYaw?: number,
   *   initialPitch?: number,
   * }} [options]
   */
  constructor(camera, options = {}) {
    this.camera = camera;
    this.distance = options.distance ?? 7.2;
    this.heightBias = options.height ?? 1.8;
    this.lerp = options.lerp ?? 10;
    this.minPitch = options.minPitch ?? 0.12;
    this.maxPitch = options.maxPitch ?? 1.25;
    /** Orbit yaw: 0 → camera on -Z looking +Z */
    this.yaw = options.initialYaw ?? 0;
    this.pitch = options.initialPitch ?? 0.45;
    this.enabled = true;
  }

  setEnabled(value) {
    this.enabled = value;
  }

  /**
   * @param {number} deltaYaw radians
   * @param {number} deltaPitch radians
   */
  addLook(deltaYaw, deltaPitch) {
    this.yaw += deltaYaw;
    this.pitch = Math.min(
      this.maxPitch,
      Math.max(this.minPitch, this.pitch + deltaPitch)
    );
  }

  /** Same yaw used by LocomotionMath.localMoveToWorldXZ */
  getFaceYaw() {
    return this.yaw;
  }

  /** Debug / XR helpers */
  getLookForwardXZ() {
    return getLookBasisXZ(this.yaw);
  }

  /**
   * @param {number} delta
   * @param {import('@babylonjs/core').Vector3|{x:number,y:number,z:number}} playerPosition
   */
  update(delta, playerPosition) {
    if (!this.enabled || !this.camera) return;

    const px = playerPosition.x;
    const py = playerPosition.y;
    const pz = playerPosition.z;

    // Place camera opposite look direction (behind the player)
    const horizontal = Math.cos(this.pitch) * this.distance;
    const { fx, fz } = getLookBasisXZ(this.yaw);
    const offsetX = -fx * horizontal;
    const offsetZ = -fz * horizontal;
    const offsetY = Math.sin(this.pitch) * this.distance + this.heightBias * 0.35;

    const targetPos = new Vector3(px + offsetX, py + offsetY, pz + offsetZ);
    const t = 1 - Math.exp(-this.lerp * delta);
    this.camera.position = Vector3.Lerp(this.camera.position, targetPos, t);

    this.camera.setTarget(new Vector3(px, py + 0.95, pz));
  }
}

export default CameraRigSystem;
