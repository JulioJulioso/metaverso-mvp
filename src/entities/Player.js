import {
  MeshBuilder,
  PBRMaterial,
  Color3,
  Vector3,
  TransformNode,
} from '@babylonjs/core';

/**
 * Controllable visitor capsule with jump / step-limited platforms.
 */
export class Player {
  /**
   * @param {import('@babylonjs/core').Scene} scene
   * @param {object} config from levelConfig.player
   * @param {import('./Platform.js').Platform[]} platforms
   * @param {number} groundY
   * @param {import('@babylonjs/core').ShadowGenerator|null} [shadowGen]
   */
  constructor(scene, config, platforms, groundY = 0, shadowGen = null) {
    this.scene = scene;
    this.config = config;
    this.platforms = platforms;
    this.groundY = groundY;
    this.heldObject = null;
    this.velocityY = 0;
    this.grounded = true;
    this.touchedJumpPlatform = false;

    const h = config.height;
    const r = config.radius;

    this.root = new TransformNode('playerRoot', scene);
    this.root.position = new Vector3(
      config.startPosition.x,
      config.startPosition.y + h / 2,
      config.startPosition.z
    );

    this.mesh = MeshBuilder.CreateCylinder(
      'player',
      { height: h, diameter: r * 2, tessellation: 20 },
      scene
    );
    this.mesh.parent = this.root;
    this.mesh.position = Vector3.Zero();

    const mat = new PBRMaterial('playerMat', scene);
    mat.albedoColor = new Color3(0.22, 0.28, 0.32);
    mat.metallic = 0.35;
    mat.roughness = 0.45;
    this.mesh.material = mat;
    this.mesh.isPickable = true;
    this.mesh.metadata = { bimId: 'local-player', label: 'Visitante' };
    if (shadowGen) shadowGen.addShadowCaster(this.mesh);

    this.hand = new TransformNode('playerHand', scene);
    this.hand.parent = this.root;
    this.hand.position = new Vector3(0.4, 0.15, 0.4);
  }

  get position() {
    return this.root.position;
  }

  getPosition() {
    return this.root.position.clone();
  }

  getFeetY() {
    return this.root.position.y - this.config.height / 2;
  }

  /**
   * @param {number} delta
   * @param {{ forward:boolean, backward:boolean, left:boolean, right:boolean, jump?:boolean }} input
   */
  update(delta, input) {
    const speed = this.config.moveSpeed;
    let mx = 0;
    let mz = 0;
    if (input.forward) mz += 1;
    if (input.backward) mz -= 1;
    if (input.left) mx -= 1;
    if (input.right) mx += 1;

    if (mx !== 0 || mz !== 0) {
      const len = Math.hypot(mx, mz);
      this.root.position.x += (mx / len) * speed * delta;
      this.root.position.z += (mz / len) * speed * delta;
    }

    if (input.jump && this.grounded) {
      this.velocityY = this.config.jumpSpeed;
      this.grounded = false;
    }

    this.velocityY -= this.config.gravity * delta;
    this.root.position.y += this.velocityY * delta;

    const support = this._resolveSupportHeight(this.getFeetY());
    const targetY = support + this.config.height / 2;

    if (this.velocityY <= 0 && this.root.position.y <= targetY + 0.02) {
      this.root.position.y = targetY;
      this.velocityY = 0;
      this.grounded = true;
    } else if (this.root.position.y > targetY + 0.05) {
      this.grounded = false;
    }

    if (this.grounded) {
      for (const p of this.platforms) {
        if (!p.requiresJump) continue;
        const d = p.getCollisionData();
        const pos = this.root.position;
        if (
          pos.x >= d.minX &&
          pos.x <= d.maxX &&
          pos.z >= d.minZ &&
          pos.z <= d.maxZ &&
          Math.abs(this.getFeetY() - d.topY) < 0.25
        ) {
          this.touchedJumpPlatform = true;
        }
      }
    }
  }

  /**
   * Highest surface the player can stand on given current feet height.
   * Tall ledges (> maxStepHeight) only work when feet are already near the top (after jump).
   */
  _resolveSupportHeight(feetY) {
    const maxStep = this.config.maxStepHeight;
    let support = this.groundY;
    const x = this.root.position.x;
    const z = this.root.position.z;
    const margin = 0.05;

    for (const p of this.platforms) {
      const d = p.getCollisionData();
      if (
        x < d.minX - margin ||
        x > d.maxX + margin ||
        z < d.minZ - margin ||
        z > d.maxZ + margin
      ) {
        continue;
      }

      const top = d.topY;
      const rise = top - feetY;

      // Landing / standing near top surface
      if (feetY >= top - 0.35 && feetY <= top + 0.5 && this.velocityY <= 0.8) {
        if (top > support) support = top;
        continue;
      }

      // Walk-up onto small steps only
      if (this.grounded && rise > 0 && rise <= maxStep) {
        if (top > support) support = top;
      }
    }

    return support;
  }

  /**
   * @param {{ mesh: import('@babylonjs/core').Mesh, setHeld: (v: boolean) => void }} object
   */
  pickUp(object) {
    if (this.heldObject) return false;
    object.mesh.setParent(this.hand);
    object.mesh.position = Vector3.Zero();
    object.setHeld(true);
    this.heldObject = object;
    return true;
  }

  drop() {
    if (!this.heldObject) return null;
    const obj = this.heldObject;
    const world = obj.mesh.getAbsolutePosition().clone();
    obj.mesh.setParent(null);
    obj.mesh.position = world;
    obj.mesh.position.y = Math.max(obj.mesh.position.y, this.getFeetY() + 0.35);
    obj.setHeld(false);
    this.heldObject = null;
    return obj;
  }

  dispose() {
    this.mesh.dispose();
    this.hand.dispose();
    this.root.dispose();
  }
}

export default Player;
