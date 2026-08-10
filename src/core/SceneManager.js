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
  StandardMaterial,
} from '@babylonjs/core';
import { ClockSystem } from './ClockSystem.js';

/**
 * Owns Engine, Scene, desktop camera, lights, ground, and the render loop.
 * Units: meters. Option useLargeWorldRendering for large AEC sites (off in MVP demo).
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

    const engineOpts = {};
    if (options.useLargeWorldRendering) {
      // Babylon 8+: float origin / high-precision matrices for large coordinates
      engineOpts.useLargeWorldRendering = true;
    }

    this.engine = new Engine(canvas, true, engineOpts);
    this.scene = new Scene(this.engine);
    this.scene.clearColor = new Color4(0.12, 0.14, 0.2, 1);

    this.camera = new UniversalCamera(
      'desktopCamera',
      new Vector3(0, 4, -8),
      this.scene
    );
    this.camera.setTarget(Vector3.Zero());
    this.camera.minZ = 0.05;
    this.camera.maxZ = 500;
    this.camera.attachControl(canvas, false);
    this.camera.inputs.clear(); // CameraRigSystem drives position; InputController moves player

    this._setupLights();
    this.ground = this._createGround();

    this._resizeHandler = () => this.engine.resize();
    window.addEventListener('resize', this._resizeHandler);
  }

  _setupLights() {
    const hemi = new HemisphericLight('hemi', new Vector3(0, 1, 0), this.scene);
    hemi.intensity = 0.65;
    hemi.groundColor = new Color3(0.2, 0.22, 0.28);

    const dir = new DirectionalLight('dir', new Vector3(-0.4, -1, 0.3), this.scene);
    dir.position = new Vector3(8, 16, -6);
    dir.intensity = 0.85;
  }

  _createGround() {
    const ground = MeshBuilder.CreateGround(
      'ground',
      { width: 40, height: 40, subdivisions: 2 },
      this.scene
    );
    const mat = new StandardMaterial('groundMat', this.scene);
    mat.diffuseColor = new Color3(0.28, 0.32, 0.36);
    mat.specularColor = new Color3(0.05, 0.05, 0.05);
    ground.material = mat;
    ground.isPickable = true;
    ground.metadata = { bimId: 'demo-ground', label: 'Suelo base (demo)' };
    return ground;
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
