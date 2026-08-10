import { MeshBuilder, StandardMaterial, Color3, Vector3 } from '@babylonjs/core';

export class Platform {
  /**
   * @param {import('@babylonjs/core').Scene} scene
   * @param {{ id: string, position: {x:number,y:number,z:number}, size: {x:number,y:number,z:number}, color?: {r:number,g:number,b:number} }} def
   */
  constructor(scene, def) {
    this.id = def.id;
    this.size = def.size;

    this.mesh = MeshBuilder.CreateBox(
      def.id,
      { width: def.size.x, height: def.size.y, depth: def.size.z },
      scene
    );
    this.mesh.position = new Vector3(
      def.position.x,
      def.position.y,
      def.position.z
    );

    const c = def.color ?? { r: 0.5, g: 0.5, b: 0.55 };
    const mat = new StandardMaterial(`${def.id}-mat`, scene);
    mat.diffuseColor = new Color3(c.r, c.g, c.b);
    mat.specularColor = new Color3(0.08, 0.08, 0.08);
    this.mesh.material = mat;
    this.mesh.isPickable = true;
    this.mesh.metadata = {
      bimId: def.id,
      label: `Plataforma ${def.id}`,
      entity: 'platform',
    };
  }

  /** Data used by CollisionSystem.getGroundHeightAt */
  getCollisionData() {
    const p = this.mesh.position;
    const s = this.size;
    return {
      minX: p.x - s.x / 2,
      maxX: p.x + s.x / 2,
      minZ: p.z - s.z / 2,
      maxZ: p.z + s.z / 2,
      topY: p.y + s.y / 2,
    };
  }

  dispose() {
    this.mesh.dispose();
  }
}

export default Platform;
