import {
  MeshBuilder,
  PBRMaterial,
  Color3,
  Vector3,
} from '@babylonjs/core';

export class Coin {
  /**
   * @param {import('@babylonjs/core').Scene} scene
   * @param {object} def
   * @param {import('@babylonjs/core').ShadowGenerator|null} [shadowGen]
   */
  constructor(scene, def, shadowGen = null) {
    this.id = def.id;
    this.globalId = def.globalId ?? def.id;
    this.collected = false;

    this.mesh = MeshBuilder.CreateCylinder(
      def.id,
      { height: 0.06, diameter: 0.38, tessellation: 32 },
      scene
    );
    this.mesh.position = new Vector3(def.position.x, def.position.y, def.position.z);
    this.mesh.rotation.z = Math.PI / 2;

    const mat = new PBRMaterial(`${def.id}-mat`, scene);
    mat.albedoColor = new Color3(0.72, 0.58, 0.22);
    mat.metallic = 0.95;
    mat.roughness = 0.28;
    mat.emissiveColor = new Color3(0.08, 0.06, 0.02);
    this.mesh.material = mat;
    this.mesh.isPickable = true;
    this.mesh.metadata = {
      bimId: this.globalId,
      label: `Marcador ${def.id}`,
      entity: 'coin',
    };
    if (shadowGen) shadowGen.addShadowCaster(this.mesh);
  }

  getPosition() {
    return this.mesh.position.clone();
  }

  update(delta) {
    if (this.collected) return;
    this.mesh.rotation.y += delta * 1.4;
  }

  collect(onCollected) {
    if (this.collected) return;
    this.collected = true;
    this.mesh.setEnabled(false);
    if (onCollected) onCollected();
  }

  dispose() {
    this.mesh.dispose();
  }
}

export default Coin;
