import { Vector3 } from '@babylonjs/core';

/**
 * Third-person camera follow for desktop. Disabled while XR session is active.
 */
export class CameraRigSystem {
  /**
   * @param {import('@babylonjs/core').UniversalCamera|import('@babylonjs/core').Camera} camera
   * @param {{ offset?: Vector3, lerp?: number }} [options]
   */
  constructor(camera, options = {}) {
    this.camera = camera;
    this.offset = options.offset ?? new Vector3(0, 3.2, -6.5);
    this.lerp = options.lerp ?? 8;
    this.enabled = true;
  }

  setEnabled(value) {
    this.enabled = value;
  }

  /**
   * @param {number} delta
   * @param {import('@babylonjs/core').Vector3|{x:number,y:number,z:number}} playerPosition
   */
  update(delta, playerPosition) {
    if (!this.enabled || !this.camera) return;

    const targetPos = new Vector3(
      playerPosition.x + this.offset.x,
      playerPosition.y + this.offset.y,
      playerPosition.z + this.offset.z
    );

    const t = 1 - Math.exp(-this.lerp * delta);
    this.camera.position = Vector3.Lerp(this.camera.position, targetPos, t);

    const lookAt = new Vector3(
      playerPosition.x,
      playerPosition.y + 0.9,
      playerPosition.z
    );
    this.camera.setTarget(lookAt);
  }
}

export default CameraRigSystem;
