import {
  MeshBuilder,
  PBRMaterial,
  Color3,
  Vector3,
  TransformNode,
} from '@babylonjs/core';

/**
 * 3D display unit in the scene. Click opens YouTube player in UI overlay.
 */
export class MediaScreen {
  /**
   * @param {import('@babylonjs/core').Scene} scene
   * @param {object} config levelConfig.video
   * @param {import('@babylonjs/core').ShadowGenerator|null} [shadowGen]
   */
  constructor(scene, config, shadowGen = null) {
    this.config = config;
    this.scene = scene;
    const w = config.screenSize.width;
    const h = config.screenSize.height;

    this.root = new TransformNode('mediaScreenRoot', scene);
    this.root.position = new Vector3(
      config.screenPosition.x,
      0,
      config.screenPosition.z
    );

    // Pedestal
    this.base = MeshBuilder.CreateBox(
      'mediaBase',
      { width: w + 0.4, height: 0.12, depth: 0.55 },
      scene
    );
    this.base.parent = this.root;
    this.base.position = new Vector3(0, 0.06, 0);
    const baseMat = new PBRMaterial('mediaBaseMat', scene);
    baseMat.albedoColor = new Color3(0.18, 0.18, 0.2);
    baseMat.metallic = 0.4;
    baseMat.roughness = 0.55;
    this.base.material = baseMat;
    this.base.receiveShadows = true;
    if (shadowGen) shadowGen.addShadowCaster(this.base);

    const poleH = Math.max(0.6, config.screenPosition.y - 0.25);
    this.pole = MeshBuilder.CreateBox(
      'mediaPole',
      { width: 0.12, height: poleH, depth: 0.12 },
      scene
    );
    this.pole.parent = this.root;
    this.pole.position = new Vector3(0, poleH / 2 + 0.12, 0);
    this.pole.material = baseMat;
    if (shadowGen) shadowGen.addShadowCaster(this.pole);

    this.mesh = MeshBuilder.CreatePlane(
      'mediaScreen',
      { width: w, height: h, sideOrientation: 2 },
      scene
    );
    this.mesh.parent = this.root;
    this.mesh.position = new Vector3(0, config.screenPosition.y, 0.02);
    // Default plane faces +Z (toward spawn at origin when screen sits at negative Z)

    const mat = new PBRMaterial('mediaScreenMat', scene);
    mat.albedoColor = new Color3(0.06, 0.08, 0.12);
    mat.emissiveColor = new Color3(0.08, 0.22, 0.38);
    mat.metallic = 0.35;
    mat.roughness = 0.4;
    this.mesh.material = mat;
    this.mesh.isPickable = true;
    this.mesh.metadata = {
      bimId: 'media-screen-01',
      label: config.title,
      entity: 'media',
      isMediaScreen: true,
    };

    this.frame = MeshBuilder.CreateBox(
      'mediaFrame',
      { width: w + 0.14, height: h + 0.14, depth: 0.1 },
      scene
    );
    this.frame.parent = this.root;
    this.frame.position = new Vector3(0, config.screenPosition.y, -0.04);
    const frameMat = new PBRMaterial('mediaFrameMat', scene);
    frameMat.albedoColor = new Color3(0.1, 0.1, 0.11);
    frameMat.metallic = 0.88;
    frameMat.roughness = 0.32;
    this.frame.material = frameMat;
    this.frame.isPickable = true;
    this.frame.metadata = { ...this.mesh.metadata };
    if (shadowGen) shadowGen.addShadowCaster(this.frame);

    // Play triangle facing +Z (same as screen)
    this.playIcon = MeshBuilder.CreateCylinder(
      'mediaPlayIcon',
      { diameter: 0.45, height: 0.04, tessellation: 3 },
      scene
    );
    this.playIcon.parent = this.root;
    this.playIcon.position = new Vector3(0, config.screenPosition.y, 0.05);
    this.playIcon.rotation.z = Math.PI / 2;
    this.playIcon.rotation.y = Math.PI / 2;
    const playMat = new PBRMaterial('mediaPlayMat', scene);
    playMat.albedoColor = new Color3(0.9, 0.92, 0.95);
    playMat.emissiveColor = new Color3(0.55, 0.65, 0.85);
    playMat.metallic = 0.1;
    playMat.roughness = 0.4;
    this.playIcon.material = playMat;
    this.playIcon.isPickable = true;
    this.playIcon.metadata = { ...this.mesh.metadata };
  }

  getEmbedUrl(autoplay = false) {
    const id = this.config.youtubeId;
    const ap = autoplay ? '&autoplay=1' : '';
    return `https://www.youtube.com/embed/${id}?rel=0${ap}`;
  }

  getTitle() {
    return this.config.title;
  }

  /**
   * @param {import('@babylonjs/core').AbstractMesh|null} mesh
   */
  isScreenMesh(mesh) {
    return !!(mesh?.metadata?.isMediaScreen);
  }

  dispose() {
    this.playIcon.dispose();
    this.mesh.dispose();
    this.frame.dispose();
    this.pole.dispose();
    this.base.dispose();
    this.root.dispose();
  }
}

export default MediaScreen;
