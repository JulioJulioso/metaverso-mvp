import {
  Engine,
  Scene,
  UniversalCamera,
  HemisphericLight,
  DirectionalLight,
  Vector3,
  Color3,
  Color4,
  MeshBuilder,
  PBRMaterial,
  ShadowGenerator,
  CubeTexture,
  DefaultRenderingPipeline,
} from '@babylonjs/core';
import { ClockSystem } from './ClockSystem.js';

/**
 * Engine, scene, IBL-ish lighting, soft shadows, polished ground.
 */
export class SceneManager {
  /**
   * @param {HTMLCanvasElement} canvas
   * @param {{ useLargeWorldRendering?: boolean }} [options]
   */
  constructor(canvas, options = {}) {
    this.canvas = canvas;
    this.clock = new ClockSystem();
    this._onUpdate = null;
    this.shadowGenerator = null;

    const engineOpts = { adaptToDeviceRatio: true };
    if (options.useLargeWorldRendering) {
      engineOpts.useLargeWorldRendering = true;
    }

    this.engine = new Engine(canvas, true, engineOpts);
    this.scene = new Scene(this.engine);
    this.scene.clearColor = new Color4(0.55, 0.62, 0.72, 1);
    this.scene.ambientColor = new Color3(0.15, 0.16, 0.18);
    this.scene.fogMode = Scene.FOGMODE_EXP2;
    this.scene.fogDensity = 0.012;
    this.scene.fogColor = new Color3(0.62, 0.68, 0.75);

    this.camera = new UniversalCamera(
      'desktopCamera',
      new Vector3(0, 4, -8),
      this.scene
    );
    this.camera.setTarget(Vector3.Zero());
    this.camera.minZ = 0.05;
    this.camera.maxZ = 400;
    this.camera.attachControl(canvas, false);
    this.camera.inputs.clear();

    this._setupEnvironment();
    this._setupLights();
    this.ground = this._createGround();
    this._setupPipeline();

    this._resizeHandler = () => this.engine.resize();
    window.addEventListener('resize', this._resizeHandler);
  }

  _setupEnvironment() {
    try {
      const env = CubeTexture.CreateFromPrefilteredData(
        'https://assets.babylonjs.com/environments/environmentSpecular.env',
        this.scene
      );
      this.scene.environmentTexture = env;
      this.scene.environmentIntensity = 0.85;

      const sky = this.scene.createDefaultSkybox(env, true, 200, 0.6, true);
      if (sky) sky.infiniteDistance = true;
    } catch (err) {
      console.warn('[SceneManager] Environment load failed, using solid sky.', err);
    }
  }

  _setupLights() {
    const hemi = new HemisphericLight('hemi', new Vector3(0.15, 1, 0.1), this.scene);
    hemi.intensity = 0.35;
    hemi.groundColor = new Color3(0.25, 0.24, 0.22);
    hemi.diffuse = new Color3(0.92, 0.94, 1);

    const dir = new DirectionalLight('sun', new Vector3(-0.55, -1, 0.35), this.scene);
    dir.position = new Vector3(18, 28, -12);
    dir.intensity = 1.35;
    dir.diffuse = new Color3(1, 0.97, 0.92);
    dir.shadowEnabled = true;

    this.shadowGenerator = new ShadowGenerator(2048, dir);
    this.shadowGenerator.useBlurExponentialShadowMap = true;
    this.shadowGenerator.blurKernel = 24;
    this.shadowGenerator.darkness = 0.35;
    this.shadowGenerator.setDarkness(0.35);

    this.dirLight = dir;
  }

  _createGround() {
    const ground = MeshBuilder.CreateGround(
      'ground',
      { width: 60, height: 60, subdivisions: 48 },
      this.scene
    );
    const mat = new PBRMaterial('groundMat', this.scene);
    mat.albedoColor = new Color3(0.38, 0.37, 0.35);
    mat.metallic = 0.02;
    mat.roughness = 0.88;
    mat.environmentIntensity = 0.75;
    ground.material = mat;
    ground.receiveShadows = true;
    ground.isPickable = true;
    ground.metadata = { bimId: 'demo-ground', label: 'Losa de hormigón' };
    return ground;
  }

  _setupPipeline() {
    try {
      const pipeline = new DefaultRenderingPipeline(
        'defaultPipeline',
        true,
        this.scene,
        [this.camera]
      );
      pipeline.fxaaEnabled = true;
      pipeline.bloomEnabled = true;
      pipeline.bloomThreshold = 0.85;
      pipeline.bloomWeight = 0.15;
      pipeline.imageProcessingEnabled = true;
      if (pipeline.imageProcessing) {
        pipeline.imageProcessing.contrast = 1.05;
        pipeline.imageProcessing.exposure = 1.05;
        pipeline.imageProcessing.toneMappingEnabled = true;
      }
      this.pipeline = pipeline;
    } catch (err) {
      console.warn('[SceneManager] Post-process limited:', err);
      this.pipeline = null;
    }
  }

  getShadowGenerator() {
    return this.shadowGenerator;
  }

  getScene() {
    return this.scene;
  }

  getEngine() {
    return this.engine;
  }

  getCamera() {
    return this.camera;
  }

  getClock() {
    return this.clock;
  }

  /** @param {(delta: number) => void} callback */
  setUpdateCallback(callback) {
    this._onUpdate = callback;
  }

  start() {
    this.clock.reset();
    this.engine.runRenderLoop(() => {
      const delta = this.clock.tick();
      if (this._onUpdate) this._onUpdate(delta);
      this.scene.render();
    });
  }

  dispose() {
    window.removeEventListener('resize', this._resizeHandler);
    this.engine.stopRenderLoop();
    this.scene.dispose();
    this.engine.dispose();
  }
}

export default SceneManager;
