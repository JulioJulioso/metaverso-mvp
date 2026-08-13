import {
  MeshBuilder,
  PBRMaterial,
  Color3,
  Vector3,
  TransformNode,
} from '@babylonjs/core';
import {
  localMoveToWorldXZ,
  normalizeXZ,
  yawFromVelocityXZ,
} from '../systems/LocomotionMath.js';
import {
  getGroundHeightAt,
  resolveSolidCapsule,
  tryStepUp,
} from '../systems/CollisionSystem.js';

/**
 * Controllable visitor capsule with solid platform colliders + step/jump.
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
    /** @type {(() => object[])|object[]} */
    this._extraColliders = [];

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

  _platformColliders() {
    const fromPlatforms = this.platforms.map((p) => p.getCollisionData());
    const extra =
      typeof this._extraColliders === 'function'
        ? this._extraColliders()
        : this._extraColliders;
    return fromPlatforms.concat(extra || []);
  }

  /**
   * Extra solid AABBs (walls, media screen, etc.). Can be a getter for moving meshes.
   * @param {(() => object[])|object[]} boxesOrGetter
   */
  setExtraColliders(boxesOrGetter) {
    this._extraColliders = boxesOrGetter;
  }

  /**
   * @param {{ x:number, y?:number, z:number }} worldPos
   * @param {{ syncY?: boolean }} [opts]
   */
  setWorldPosition(worldPos, opts = {}) {
    this.root.position.x = worldPos.x;
    this.root.position.z = worldPos.z;
    if (opts.syncY !== false && worldPos.y != null) {
      this.root.position.y = worldPos.y;
    }
  }

  /**
   * @param {number} x
   * @param {number} z
   */
  setWorldXZ(x, z) {
    this.root.position.x = x;
    this.root.position.z = z;
  }

  /**
   * @param {number} delta
   * @param {{
   *   forward?: boolean, backward?: boolean, left?: boolean, right?: boolean,
   *   moveX?: number, moveZ?: number,
   *   jump?: boolean, faceYaw?: number,
   *   skipHorizontal?: boolean,
   * }} input
   */
  update(delta, input) {
    const colliders = this._platformColliders();
    const radius = this.config.radius;
    const height = this.config.height;
    const maxStep = this.config.maxStepHeight ?? 0.45;

    if (!input.skipHorizontal) {
      let mx = input.moveX ?? 0;
      let mz = input.moveZ ?? 0;
      if (input.forward) mz += 1;
      if (input.backward) mz -= 1;
      if (input.left) mx -= 1;
      if (input.right) mx += 1;

      const n = normalizeXZ(mx, mz);
      if (n.len > 0) {
        const speed = this.config.moveSpeed * delta;
        const yaw = input.faceYaw ?? 0;
        const world = localMoveToWorldXZ(n.x * speed, n.z * speed, yaw);
        this.root.position.x += world.x;
        this.root.position.z += world.z;

        const face = yawFromVelocityXZ(world.x, world.z);
        if (face != null) this.root.rotation.y = face;
      }
    }

    // Step-up onto low platforms before treating them as solid walls
    if (this.grounded || this.velocityY <= 0.05) {
      const stepped = tryStepUp(
        this.getFeetY(),
        this.root.position.x,
        this.root.position.z,
        colliders,
        maxStep
      );
      if (stepped != null) {
        this.root.position.y = stepped + height / 2;
        this.velocityY = 0;
        this.grounded = true;
      }
    }

    // Solid volume: push out of platform sides / interiors
    const resolved = resolveSolidCapsule(
      {
        x: this.root.position.x,
        y: this.root.position.y,
        z: this.root.position.z,
      },
      radius,
      height,
      colliders
    );
    this.root.position.x = resolved.x;
    this.root.position.z = resolved.z;

    if (input.jump && this.grounded) {
      this.velocityY = this.config.jumpSpeed;
      this.grounded = false;
    }

    this.velocityY -= this.config.gravity * delta;
    this.root.position.y += this.velocityY * delta;

    const support = getGroundHeightAt(
      this.root.position,
      colliders,
      this.groundY,
      {
        feetY: this.getFeetY(),
        maxStepHeight: maxStep,
        velocityY: this.velocityY,
        grounded: this.grounded,
      }
    );
    const targetY = support + height / 2;

    if (this.velocityY <= 0 && this.root.position.y <= targetY + 0.04) {
      this.root.position.y = targetY;
      this.velocityY = 0;
      this.grounded = true;
    } else if (this.root.position.y > targetY + 0.06) {
      this.grounded = false;
    }

    // Re-resolve solids after vertical move (landed inside a box edge case)
    const resolved2 = resolveSolidCapsule(
      {
        x: this.root.position.x,
        y: this.root.position.y,
        z: this.root.position.z,
      },
      radius,
      height,
      colliders
    );
    this.root.position.x = resolved2.x;
    this.root.position.z = resolved2.z;

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
   * @param {{ mesh: import('@babylonjs/core').Mesh, setHeld: (v: boolean) => void }} object
   * @param {import('@babylonjs/core').TransformNode|null} [attachNode]
   */
  pickUp(object, attachNode = null) {
    if (this.heldObject) return false;
    const parent = attachNode || this.hand;
    object.mesh.setParent(parent);
    object.mesh.position = Vector3.Zero();
    if (attachNode) {
      object.mesh.position = new Vector3(0, 0, 0.12);
    }
    object.setHeld(true);
    this.heldObject = object;
    return true;
  }

  drop() {
    if (!this.heldObject) return null;
    const obj = this.heldObject;
    const world = obj.mesh.getAbsolutePosition().clone();
    obj.mesh.setParent(null);
    obj.mesh.position.copyFrom(world);
    if (typeof obj.releaseToWorld === 'function') {
      obj.releaseToWorld();
    } else {
      obj.setHeld(false);
    }
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
