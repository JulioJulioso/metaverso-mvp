import {
  MeshBuilder,
  PBRMaterial,
  Color3,
  Vector3,
} from '@babylonjs/core';

export class Platform {
  /**
   * @param {import('@babylonjs/core').Scene} scene
   * @param {object} def
   * @param {import('@babylonjs/core').ShadowGenerator|null} [shadowGen]
   */
  constructor(scene, def, shadowGen = null) {
    this.id = def.id;
    this.size = def.size;
    this.requiresJump = !!def.requiresJump;

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

    const c = def.albedo ?? def.color ?? { r: 0.4, g: 0.39, b: 0.38 };
    const mat = new PBRMaterial(`${def.id}-mat`, scene);
    mat.albedoColor = new Color3(c.r, c.g, c.b);
    mat.metallic = 0.08;
    mat.roughness = 0.78;
    mat.environmentIntensity = 0.85;
    this.mesh.material = mat;
    this.mesh.receiveShadows = true;
    this.mesh.isPickable = true;
    this.mesh.metadata = {
      bimId: def.id,
      label: def.requiresJump
        ? `Plataforma alta (requiere salto)`
        : `Plataforma ${def.id}`,
      entity: 'platform',
    };

    if (shadowGen) shadowGen.addShadowCaster(this.mesh);
  }

  getCollisionData() {
    const p = this.mesh.position;
    const s = this.size;
    return {
      minX: p.x - s.x / 2,
      maxX: p.x + s.x / 2,
      minZ: p.z - s.z / 2,
      maxZ: p.z + s.z / 2,
      topY: p.y + s.y / 2,
      bottomY: p.y - s.y / 2,
      sizeY: s.y,
      requiresJump: this.requiresJump,
    };
  }

  dispose() {
    this.mesh.dispose();
  }
}

export default Platform;
