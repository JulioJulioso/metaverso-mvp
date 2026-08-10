import { MeshBuilder, StandardMaterial, Color3, Vector3 } from '@babylonjs/core';

export class Coin {
  /**
   * @param {import('@babylonjs/core').Scene} scene
   * @param {{ id: string, globalId?: string, position: {x:number,y:number,z:number} }} def
   */
  constructor(scene, def) {
    this.id = def.id;
    this.globalId = def.globalId ?? def.id;
    this.collected = false;

    this.mesh = MeshBuilder.CreateCylinder(
      def.id,
      { height: 0.08, diameter: 0.45, tessellation: 24 },
      scene
    );
    this.mesh.position = new Vector3(def.position.x, def.position.y, def.position.z);
    this.mesh.rotation.z = Math.PI / 2;

    const mat = new StandardMaterial(`${def.id}-mat`, scene);
    mat.diffuseColor = new Color3(0.95, 0.78, 0.15);
    mat.emissiveColor = new Color3(0.25, 0.18, 0.02);
    mat.specularColor = new Color3(0.4, 0.35, 0.1);
    this.mesh.material = mat;
    this.mesh.isPickable = true;
    this.mesh.metadata = {
      bimId: this.globalId,
      label: `Marcador ${def.id}`,
      entity: 'coin',
    };
  }

  getPosition() {
    return this.mesh.position.clone();
  }

  update(delta) {
    if (this.collected) return;
    this.mesh.rotation.y += delta * 2.2;
  }

  /** @param {() => void} [onCollected] */
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
