import {
  MeshBuilder,
  PBRMaterial,
  Color3,
  Vector3,
} from '@babylonjs/core';

/**
 * Work ball: falls with gravity onto ground/platforms when not held.
 */
export class PickupSphere {
  /**
   * @param {import('@babylonjs/core').Scene} scene
   * @param {object} def
   * @param {import('@babylonjs/core').ShadowGenerator|null} [shadowGen]
   */
  constructor(scene, def, shadowGen = null) {
    this.id = def.id;
    this.globalId = def.globalId ?? def.id;
    this.held = false;
    this.radius = def.radius ?? 0.32;
    this.gravity = def.gravity ?? 16;
    this.velocityY = 0;
    this.grounded = false;
    /** @type {import('./Platform.js').Platform[]} */
    this._platforms = [];
    this._groundY = 0;

    this.mesh = MeshBuilder.CreateSphere(
      def.id,
      { diameter: this.radius * 2, segments: 24 },
      scene
    );
    this.mesh.position = new Vector3(
      def.position.x,
      def.position.y,
      def.position.z
    );

    const mat = new PBRMaterial(`${def.id}-mat`, scene);
    mat.albedoColor = new Color3(0.55, 0.12, 0.1);
    mat.metallic = 0.15;
    mat.roughness = 0.35;
    mat.emissiveColor = new Color3(0.04, 0, 0);
    this.mesh.material = mat;
    this.mesh.isPickable = true;
    this.mesh.metadata = {
      bimId: this.globalId,
      label: 'Pelota de trabajo',
      entity: 'pickup',
    };
    if (shadowGen) shadowGen.addShadowCaster(this.mesh);
  }

  /**
   * @param {import('./Platform.js').Platform[]} platforms
   * @param {number} groundY
   */
  setWorldColliders(platforms, groundY = 0) {
    this._platforms = platforms;
    this._groundY = groundY;
  }

  getPosition() {
    return this.mesh.getAbsolutePosition().clone();
  }

  setHeld(value) {
    this.held = value;
    if (value) {
      this.velocityY = 0;
      this.grounded = false;
    }
  }

  isHeld() {
    return this.held;
  }

  /** Call after unparenting so world position is free to fall. */
  releaseToWorld() {
    this.held = false;
    this.velocityY = 0;
    this.grounded = false;
  }

  /**
   * Simple vertical physics when free.
   * @param {number} delta
   */
  update(delta) {
    if (this.held) return;

    this.velocityY -= this.gravity * delta;
    this.mesh.position.y += this.velocityY * delta;

    const support = this._supportHeight();
    const restY = support + this.radius;

    if (this.velocityY <= 0 && this.mesh.position.y <= restY) {
      this.mesh.position.y = restY;
      this.velocityY = 0;
      this.grounded = true;
    } else {
      this.grounded = false;
    }
  }

  _supportHeight() {
    const pos = this.mesh.position;
    let support = this._groundY;
    const margin = this.radius * 0.35;
    const bottom = pos.y - this.radius;

    for (const p of this._platforms) {
      const d = p.getCollisionData();
      if (
        pos.x < d.minX - margin ||
        pos.x > d.maxX + margin ||
        pos.z < d.minZ - margin ||
        pos.z > d.maxZ + margin
      ) {
        continue;
      }
      // Only rest on tops the ball has reached (from above or already close)
      if (bottom <= d.topY + 0.08 && pos.y >= d.topY - 0.05) {
        if (d.topY > support) support = d.topY;
      }
    }
    return support;
  }

  dispose() {
    this.mesh.dispose();
  }
}

export default PickupSphere;
