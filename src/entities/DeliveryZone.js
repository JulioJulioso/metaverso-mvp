import {
  MeshBuilder,
  PBRMaterial,
  Color3,
  Vector3,
  TransformNode,
} from '@babylonjs/core';
import { isNearXZ } from '../systems/CollisionSystem.js';

/**
 * Visible delivery pad on open ground: glowing disc + post marker.
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

    this.root = new TransformNode(def.id, scene);
    this.root.position = this.position.clone();

    const c = def.color ?? { r: 0.2, g: 0.5, b: 0.55 };

    this.mesh = MeshBuilder.CreateCylinder(
      `${def.id}-pad`,
      { diameter: def.radius * 2, height: 0.06, tessellation: 40 },
      scene
    );
    this.mesh.parent = this.root;
    this.mesh.position.y = 0.03;

    const mat = new PBRMaterial(`${def.id}-mat`, scene);
    mat.albedoColor = new Color3(c.r, c.g, c.b);
    mat.emissiveColor = new Color3(c.r * 0.45, c.g * 0.45, c.b * 0.45);
    mat.metallic = 0.2;
    mat.roughness = 0.4;
    this.mesh.material = mat;
    this.mesh.isPickable = true;
    this.mesh.metadata = {
      bimId: def.id,
      label: def.label,
      entity: 'delivery-zone',
    };

    // Ring rim for visibility
    this.ring = MeshBuilder.CreateTorus(
      `${def.id}-ring`,
      { diameter: def.radius * 2 - 0.05, thickness: 0.08, tessellation: 32 },
      scene
    );
    this.ring.parent = this.root;
    this.ring.position.y = 0.06;
    this.ring.rotation.x = Math.PI / 2;
    const ringMat = new PBRMaterial(`${def.id}-ring-mat`, scene);
    ringMat.albedoColor = new Color3(c.r * 1.2, c.g * 1.2, c.b * 1.2);
    ringMat.emissiveColor = new Color3(c.r * 0.6, c.g * 0.6, c.b * 0.6);
    ringMat.metallic = 0.4;
    ringMat.roughness = 0.3;
    this.ring.material = ringMat;
    this.ring.isPickable = false;

    // Vertical post so the site is visible from afar
    this.post = MeshBuilder.CreateCylinder(
      `${def.id}-post`,
      { diameter: 0.12, height: 1.6, tessellation: 12 },
      scene
    );
    this.post.parent = this.root;
    this.post.position = new Vector3(0, 0.8, 0);
    const postMat = new PBRMaterial(`${def.id}-post-mat`, scene);
    postMat.albedoColor = new Color3(c.r, c.g, c.b);
    postMat.emissiveColor = new Color3(c.r * 0.35, c.g * 0.35, c.b * 0.35);
    postMat.metallic = 0.5;
    postMat.roughness = 0.4;
    this.post.material = postMat;
    this.post.isPickable = false;

    this.beacon = MeshBuilder.CreateSphere(
      `${def.id}-beacon`,
      { diameter: 0.28, segments: 12 },
      scene
    );
    this.beacon.parent = this.root;
    this.beacon.position = new Vector3(0, 1.7, 0);
    const beaconMat = new PBRMaterial(`${def.id}-beacon-mat`, scene);
    beaconMat.albedoColor = new Color3(1, 1, 1);
    beaconMat.emissiveColor = new Color3(c.r * 0.9, c.g * 0.9, c.b * 0.9);
    this.beacon.material = beaconMat;
    this.beacon.isPickable = false;
  }

  /**
   * @param {{ x:number, y:number, z:number }} ballPos absolute
   * @param {boolean} held player holding ball
   */
  tryDeliver(ballPos, held) {
    if (this.completed || held) return false;
    if (!isNearXZ(ballPos, this.position, this.radius)) return false;
    // Resting height roughly radius above pad (~0.3–0.6 m)
    if (ballPos.y > 1.4) return false;

    this.completed = true;
    const markDone = (mat) => {
      if (!mat) return;
      mat.emissiveColor = new Color3(0.12, 0.55, 0.22);
      mat.albedoColor = new Color3(0.28, 0.58, 0.35);
    };
    markDone(this.mesh.material);
    markDone(this.ring.material);
    markDone(this.post.material);
    if (this.beacon.material) {
      this.beacon.material.emissiveColor = new Color3(0.2, 0.9, 0.35);
    }
    return true;
  }

  dispose() {
    this.mesh.dispose();
    this.ring.dispose();
    this.post.dispose();
    this.beacon.dispose();
    this.root.dispose();
  }
}

export default DeliveryZone;
