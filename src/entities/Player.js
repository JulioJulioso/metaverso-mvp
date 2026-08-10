import {
  MeshBuilder,
  StandardMaterial,
  Color3,
  Vector3,
  TransformNode,
} from '@babylonjs/core';
import { getGroundHeightAt } from '../systems/CollisionSystem.js';

/**
 * Controllable capsule substitute (cylinder). Units: meters.
 */
export class Player {
  /**
   * @param {import('@babylonjs/core').Scene} scene
   * @param {object} config from levelConfig.player
   * @param {import('./Platform.js').Platform[]} platforms
   * @param {number} groundY
   */
  constructor(scene, config, platforms, groundY = 0) {
    this.scene = scene;
    this.config = config;
    this.platforms = platforms;
    this.groundY = groundY;
    this.heldObject = null;

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
      { height: h, diameter: r * 2, tessellation: 16 },
      scene
    );
    this.mesh.parent = this.root;
    this.mesh.position = Vector3.Zero();

    const mat = new StandardMaterial('playerMat', scene);
    mat.diffuseColor = new Color3(0.25, 0.55, 0.95);
    mat.specularColor = new Color3(0.15, 0.15, 0.2);
    this.mesh.material = mat;
    this.mesh.isPickable = true;
    this.mesh.metadata = { bimId: 'local-player', label: 'Visitante' };

    this.hand = new TransformNode('playerHand', scene);
    this.hand.parent = this.root;
    this.hand.position = new Vector3(0.45, 0.2, 0.35);
  }

  get position() {
    return this.root.position;
  }

  getPosition() {
    return this.root.position.clone();
  }

  /**
   * Feet Y for ground snap (bottom of cylinder).
   */
  getFeetY() {
    return this.root.position.y - this.config.height / 2;
  }

  /**
   * @param {number} delta
   * @param {{ forward: boolean, backward: boolean, left: boolean, right: boolean }} input
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
      mx = (mx / len) * speed * delta;
      mz = (mz / len) * speed * delta;
      this.root.position.x += mx;
      this.root.position.z += mz;
    }

    const platformData = this.platforms.map((p) => p.getCollisionData());
    const groundTop = getGroundHeightAt(
      this.root.position,
      platformData,
      this.groundY
    );
    this.root.position.y = groundTop + this.config.height / 2;
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
    // Place slightly above ground under feet-ish
    obj.mesh.position.y = Math.max(obj.mesh.position.y, this.getFeetY() + 0.4);
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
