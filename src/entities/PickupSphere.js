import {
  MeshBuilder,
  PBRMaterial,
  Color3,
  Vector3,
} from '@babylonjs/core';

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

  getPosition() {
    return this.mesh.getAbsolutePosition().clone();
  }

  setHeld(value) {
    this.held = value;
  }

  isHeld() {
    return this.held;
  }

  dispose() {
    this.mesh.dispose();
  }
}

export default PickupSphere;
