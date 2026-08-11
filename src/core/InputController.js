/**
 * Keyboard + mouse look (hold right mouse or middle mouse to orbit camera).
 */
export class InputController {
  /**
   * @param {HTMLCanvasElement} [canvas]
   */
  constructor(canvas = null) {
    this.canvas = canvas;
    this._keys = new Set();
    this._interactPressed = false;
    this._dropPressed = false;
    this._jumpPressed = false;
    this._explodePressed = false;
    this._risePressed = false;

    this._lookActive = false;
    this._lookDx = 0;
    this._lookDy = 0;
    this.lookSensitivity = 0.0035;

    this._onKeyDown = (e) => {
      this._keys.add(e.code);
      if (e.code === 'KeyE') {
        if (!e.repeat) this._interactPressed = true;
        e.preventDefault();
      }
      if (e.code === 'Space') {
        if (!e.repeat) this._jumpPressed = true;
        e.preventDefault();
      }
      if (e.code === 'KeyF') {
        if (!e.repeat) this._dropPressed = true;
        e.preventDefault();
      }
      if (e.code === 'KeyX') {
        if (!e.repeat) this._explodePressed = true;
        e.preventDefault();
      }
      if (e.code === 'KeyR') {
        if (!e.repeat) this._risePressed = true;
        e.preventDefault();
      }
    };

    this._onKeyUp = (e) => {
      this._keys.delete(e.code);
    };

    this._onPointerDown = (e) => {
      if (e.button === 2 || e.button === 1) {
        this._lookActive = true;
        e.preventDefault();
      }
    };

    this._onPointerUp = (e) => {
      if (e.button === 2 || e.button === 1) {
        this._lookActive = false;
      }
    };

    this._onPointerMove = (e) => {
      if (!this._lookActive) return;
      this._lookDx += e.movementX * this.lookSensitivity;
      this._lookDy += e.movementY * this.lookSensitivity;
    };

    this._onContextMenu = (e) => {
      if (
        e.target === this.canvas ||
        (this.canvas && e.target instanceof Node && this.canvas.contains(e.target))
      ) {
        e.preventDefault();
      }
    };

    window.addEventListener('keydown', this._onKeyDown);
    window.addEventListener('keyup', this._onKeyUp);
    window.addEventListener('pointerup', this._onPointerUp);
    window.addEventListener('pointermove', this._onPointerMove);
    window.addEventListener('contextmenu', this._onContextMenu);

    if (this.canvas) {
      this.canvas.addEventListener('pointerdown', this._onPointerDown);
    } else {
      window.addEventListener('pointerdown', this._onPointerDown);
    }
  }

  getState() {
    const forward =
      this._keys.has('KeyW') || this._keys.has('ArrowUp');
    const backward =
      this._keys.has('KeyS') || this._keys.has('ArrowDown');
    const left =
      this._keys.has('KeyA') || this._keys.has('ArrowLeft');
    const right =
      this._keys.has('KeyD') || this._keys.has('ArrowRight');

    // Keyboard orbit while holding Shift + arrows
    let keyYaw = 0;
    let keyPitch = 0;
    if (this._keys.has('ShiftLeft') || this._keys.has('ShiftRight')) {
      if (this._keys.has('ArrowLeft')) keyYaw -= 0.045;
      if (this._keys.has('ArrowRight')) keyYaw += 0.045;
      if (this._keys.has('ArrowUp')) keyPitch -= 0.03;
      if (this._keys.has('ArrowDown')) keyPitch += 0.03;
    }
    // Q / ; for orbit without mouse
    if (this._keys.has('KeyQ')) keyYaw -= 0.04;
    if (this._keys.has('Semicolon') || this._keys.has('KeyV')) keyYaw += 0.04;

    const interact = this._interactPressed;
    const drop = this._dropPressed;
    const jump = this._jumpPressed;
    const explode = this._explodePressed;
    const riseWalls = this._risePressed;
    const lookDeltaX = this._lookDx + keyYaw;
    const lookDeltaY = this._lookDy + keyPitch;

    this._interactPressed = false;
    this._dropPressed = false;
    this._jumpPressed = false;
    this._explodePressed = false;
    this._risePressed = false;
    this._lookDx = 0;
    this._lookDy = 0;

    return {
      forward,
      backward,
      left,
      right,
      interact,
      drop,
      jump,
      explode,
      riseWalls,
      lookDeltaX,
      lookDeltaY,
    };
  }

  dispose() {
    window.removeEventListener('keydown', this._onKeyDown);
    window.removeEventListener('keyup', this._onKeyUp);
    window.removeEventListener('pointerup', this._onPointerUp);
    window.removeEventListener('pointermove', this._onPointerMove);
    window.removeEventListener('contextmenu', this._onContextMenu);
    if (this.canvas) {
      this.canvas.removeEventListener('pointerdown', this._onPointerDown);
    } else {
      window.removeEventListener('pointerdown', this._onPointerDown);
    }
  }
}

export default InputController;
