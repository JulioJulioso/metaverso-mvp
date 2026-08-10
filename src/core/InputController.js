/**
 * Keyboard input: WASD, Space=jump, E=interact, F=drop, X=explode walls.
 */
export class InputController {
  constructor() {
    this._keys = new Set();
    this._interactPressed = false;
    this._dropPressed = false;
    this._jumpPressed = false;
    this._explodePressed = false;

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
    };

    this._onKeyUp = (e) => {
      this._keys.delete(e.code);
    };

    window.addEventListener('keydown', this._onKeyDown);
    window.addEventListener('keyup', this._onKeyUp);
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

    const interact = this._interactPressed;
    const drop = this._dropPressed;
    const jump = this._jumpPressed;
    const explode = this._explodePressed;
    this._interactPressed = false;
    this._dropPressed = false;
    this._jumpPressed = false;
    this._explodePressed = false;

    return { forward, backward, left, right, interact, drop, jump, explode };
  }

  dispose() {
    window.removeEventListener('keydown', this._onKeyDown);
    window.removeEventListener('keyup', this._onKeyUp);
  }
}

export default InputController;
