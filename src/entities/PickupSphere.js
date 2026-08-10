import { MeshBuilder, StandardMaterial, Color3, Vector3 } from '@babylonjs/core';

export class PickupSphere {
  /**
   * @param {import('@babylonjs/core').Scene} scene
   * @param {{ id: string, globalId?: string, position: {x:number,y:number,z:number}, radius?: number }} def
   */
  constructor(scene, def) {
    this.id = def.id;
    this.globalId = def.globalId ?? def.id;
    this.held = false;
    this.radius = def.radius ?? 0.35;

    this.mesh = MeshBuilder.CreateSphere(
      def.id,
      { diameter: this.radius * 2, segments: 16 },
      scene
    );
    this.mesh.position = new Vector3(
      def.position.x,
      def.position.y,
      def.position.z
    );

    const mat = new StandardMaterial(`${def.id}-mat`, scene);
    mat.diffuseColor = new Color3(0.85, 0.35, 0.45);
    mat.specularColor = new Color3(0.3, 0.3, 0.3);
    this.mesh.material = mat;
    this.mesh.isPickable = true;
    this.mesh.metadata = {
      bimId: this.globalId,
      label: 'Esfera interactiva',
      entity: 'pickup',
    };
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
