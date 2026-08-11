import { Vector3, Matrix, Quaternion } from '@babylonjs/core';

/**
 * Third-person orbit camera (yaw/pitch). Disabled in XR.
 * WASD is relative to this yaw when Player receives faceYaw.
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
   * }} [options]
   */
  constructor(camera, options = {}) {
    this.camera = camera;
    this.distance = options.distance ?? 7.2;
    this.heightBias = options.height ?? 1.8;
    this.lerp = options.lerp ?? 10;
    this.minPitch = options.minPitch ?? 0.12;
    this.maxPitch = options.maxPitch ?? 1.25;
    /** Yaw around player (radians). 0 = looking along +Z from -Z. */
    this.yaw = options.initialYaw ?? 0;
    /** Pitch from horizontal plane (radians). */
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

  /** Horizontal facing used to orient player movement (XZ). */
  getFaceYaw() {
    return this.yaw;
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

    // Orbit: camera sits opposite the look direction
    const horizontal = Math.cos(this.pitch) * this.distance;
    const offsetX = Math.sin(this.yaw) * horizontal;
    const offsetZ = -Math.cos(this.yaw) * horizontal;
    const offsetY = Math.sin(this.pitch) * this.distance + this.heightBias * 0.35;

    const targetPos = new Vector3(px + offsetX, py + offsetY, pz + offsetZ);
    const t = 1 - Math.exp(-this.lerp * delta);
    this.camera.position = Vector3.Lerp(this.camera.position, targetPos, t);

    const lookAt = new Vector3(px, py + 0.95, pz);
    this.camera.setTarget(lookAt);
  }
}

export default CameraRigSystem;
