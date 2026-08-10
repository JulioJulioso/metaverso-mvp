import {
  MeshBuilder,
  PBRMaterial,
  Color3,
  Vector3,
} from '@babylonjs/core';
import { isNearXZ } from '../systems/CollisionSystem.js';

/**
 * Ground marker ring/cylinder for ball delivery sites.
 */
export class DeliveryZone {
  /**
   * @param {import('@babylonjs/core').Scene} scene
   * @param {object} def
   */
  constructor(scene, def) {
    this.id = def.id;
    this.label = def.label;
    this.position = new Vector3(def.position.x, def.position.y, def.position.z);
    this.radius = def.radius;
    this.completed = false;

    this.mesh = MeshBuilder.CreateCylinder(
      def.id,
      { diameter: def.radius * 2, height: 0.04, tessellation: 32 },
      scene
    );
    this.mesh.position = this.position.clone();
    this.mesh.position.y = 0.02;

    const mat = new PBRMaterial(`${def.id}-mat`, scene);
    const c = def.color ?? { r: 0.2, g: 0.5, b: 0.55 };
    mat.albedoColor = new Color3(c.r, c.g, c.b);
    mat.emissiveColor = new Color3(c.r * 0.25, c.g * 0.25, c.b * 0.25);
    mat.metallic = 0.15;
    mat.roughness = 0.45;
    this.mesh.material = mat;
    this.mesh.isPickable = true;
    this.mesh.metadata = {
      bimId: def.id,
      label: def.label,
      entity: 'delivery-zone',
    };
  }

  /**
   * @param {{ x:number, y:number, z:number }} ballPos absolute
   * @param {boolean} held player holding ball
   */
  tryDeliver(ballPos, held) {
    if (this.completed || held) return false;
    if (!isNearXZ(ballPos, this.position, this.radius)) return false;
    // Ball must be near ground in zone
    if (ballPos.y > 1.2) return false;
    this.completed = true;
    const mat = this.mesh.material;
    if (mat) {
      mat.emissiveColor = new Color3(0.1, 0.45, 0.2);
      mat.albedoColor = new Color3(0.25, 0.55, 0.35);
    }
    return true;
  }

  dispose() {
    this.mesh.dispose();
  }
}

export default DeliveryZone;
