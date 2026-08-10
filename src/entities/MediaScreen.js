import {
  MeshBuilder,
  PBRMaterial,
  Color3,
  Vector3,
} from '@babylonjs/core';

/**
 * Physical screen plane; video plays via DOM iframe (YouTube CORS-safe embed).
 */
export class MediaScreen {
  /**
   * @param {import('@babylonjs/core').Scene} scene
   * @param {object} config levelConfig.video
   */
  constructor(scene, config) {
    this.config = config;
    const w = config.screenSize.width;
    const h = config.screenSize.height;

    this.mesh = MeshBuilder.CreatePlane(
      'mediaScreen',
      { width: w, height: h, sideOrientation: 2 },
      scene
    );
    this.mesh.position = new Vector3(
      config.screenPosition.x,
      config.screenPosition.y,
      config.screenPosition.z
    );
    this.mesh.rotation.y = Math.PI;

    const mat = new PBRMaterial('mediaScreenMat', scene);
    mat.albedoColor = new Color3(0.08, 0.08, 0.1);
    mat.emissiveColor = new Color3(0.12, 0.12, 0.15);
    mat.metallic = 0.6;
    mat.roughness = 0.35;
    this.mesh.material = mat;
    this.mesh.isPickable = true;
    this.mesh.metadata = {
      bimId: 'media-screen-01',
      label: config.title,
      entity: 'media',
    };

    // Frame
    this.frame = MeshBuilder.CreateBox(
      'mediaFrame',
      { width: w + 0.12, height: h + 0.12, depth: 0.08 },
      scene
    );
    this.frame.position = this.mesh.position.clone();
    this.frame.position.z += 0.05;
    this.frame.rotation.y = Math.PI;
    const frameMat = new PBRMaterial('mediaFrameMat', scene);
    frameMat.albedoColor = new Color3(0.12, 0.12, 0.13);
    frameMat.metallic = 0.85;
    frameMat.roughness = 0.35;
    this.frame.material = frameMat;
  }

  getEmbedUrl() {
    return `https://www.youtube.com/embed/${this.config.youtubeId}?rel=0`;
  }

  dispose() {
    this.mesh.dispose();
    this.frame.dispose();
  }
}

export default MediaScreen;
