/**
 * Desktop click-to-pick; resolves BIM metadata via BimIndexStub.
 */
export class PickingSystem {
  /**
   * @param {import('@babylonjs/core').Scene} scene
   * @param {import('@babylonjs/core').Camera} camera
   * @param {HTMLCanvasElement} canvas
   * @param {import('./BimIndexStub.js').BimIndexStub} bimIndex
   * @param {(info: object|null) => void} onPick
   */
  constructor(scene, camera, canvas, bimIndex, onPick) {
    this.scene = scene;
    this.camera = camera;
    this.canvas = canvas;
    this.bimIndex = bimIndex;
    this.onPick = onPick;

    this._onPointer = (evt) => {
      if (evt.button !== 0) return;
      const pick = this.scene.pick(
        this.scene.pointerX,
        this.scene.pointerY,
        null,
        false,
        this.camera
      );
      if (pick?.hit && pick.pickedMesh) {
        const element = this.bimIndex.resolveFromMesh(pick.pickedMesh);
        this.onPick?.({
          meshName: pick.pickedMesh.name,
          point: pick.pickedPoint,
          element,
        });
      } else {
        this.onPick?.(null);
      }
    };

    canvas.addEventListener('pointerdown', this._onPointer);
  }

  dispose() {
    this.canvas.removeEventListener('pointerdown', this._onPointer);
  }
}

export default PickingSystem;
