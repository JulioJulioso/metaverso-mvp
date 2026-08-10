/**
 * DOM HUD: coin counter + temporary messages.
 */
export class HUD {
  /**
   * @param {HTMLElement} root
   */
  constructor(root) {
    this.root = root;
    this._toastTimer = null;

    this.panel = document.createElement('div');
    this.panel.className = 'hud-panel';
    this.panel.innerHTML = '<strong>Marcadores:</strong> <span data-coins>0 / 0</span>';

    this.toast = document.createElement('div');
    this.toast.className = 'hud-toast';
    this.toast.setAttribute('role', 'status');

    this.hints = document.createElement('div');
    this.hints.className = 'hud-hints';
    this.hints.innerHTML =
      'Mover <kbd>WASD</kbd> · Interactuar <kbd>E</kbd> · Soltar <kbd>F</kbd> · Clic = info BIM · VR: botón Babylon';

    this.root.append(this.panel, this.toast, this.hints);
    this._coinsEl = this.panel.querySelector('[data-coins]');
  }

  /**
   * @param {{ collected: number, total: number, message?: string, completed?: boolean }} state
   */
  updateCoins(state) {
    if (this._coinsEl) {
      this._coinsEl.textContent = `${state.collected} / ${state.total}`;
    }
    if (state.message) this.showMessage(state.message);
  }

  /**
   * @param {string} text
   * @param {number} [ms]
   */
  showMessage(text, ms = 2200) {
    this.toast.textContent = text;
    this.toast.classList.add('visible');
    if (this._toastTimer) clearTimeout(this._toastTimer);
    this._toastTimer = setTimeout(() => {
      this.toast.classList.remove('visible');
    }, ms);
  }

  dispose() {
    if (this._toastTimer) clearTimeout(this._toastTimer);
    this.root.replaceChildren();
  }
}

export default HUD;
