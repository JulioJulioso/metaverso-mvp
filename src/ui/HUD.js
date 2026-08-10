/**
 * DOM HUD: coins, circuit checklist, YouTube embed, completion banner.
 */
export class HUD {
  /**
   * @param {HTMLElement} root
   * @param {{ youtubeEmbedUrl: string, videoTitle: string }} media
   */
  constructor(root, media) {
    this.root = root;
    this._toastTimer = null;

    this.panel = document.createElement('div');
    this.panel.className = 'hud-panel';
    this.panel.innerHTML =
      '<div class="hud-panel-title">Recorrido</div>' +
      '<div><strong>Marcadores:</strong> <span data-coins>0 / 0</span></div>';

    this.circuit = document.createElement('div');
    this.circuit.className = 'hud-circuit';
    this.circuit.innerHTML =
      '<div class="hud-panel-title">Circuito — checklist</div><ul data-steps></ul>';

    this.videoPanel = document.createElement('div');
    this.videoPanel.className = 'hud-video';
    this.videoPanel.innerHTML = `
      <div class="hud-panel-title">${media.videoTitle}</div>
      <div class="hud-video-frame">
        <iframe
          src="${media.youtubeEmbedUrl}"
          title="YouTube video"
          allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
          allowfullscreen
          loading="lazy"
        ></iframe>
      </div>
    `;

    this.toast = document.createElement('div');
    this.toast.className = 'hud-toast';
    this.toast.setAttribute('role', 'status');

    this.completion = document.createElement('div');
    this.completion.className = 'hud-completion';
    this.completion.innerHTML =
      '<div class="hud-completion-card">' +
      '<h2>¡Circuito completado!</h2>' +
      '<p>Has recogido los marcadores, recorrido las plataformas y entregado la pelota en los tres sitios.</p>' +
      '</div>';

    this.hints = document.createElement('div');
    this.hints.className = 'hud-hints';
    this.hints.innerHTML =
      '<kbd>WASD</kbd> mover · <kbd>Espacio</kbd> saltar · <kbd>E</kbd> interactuar · <kbd>F</kbd> soltar · <kbd>X</kbd> despiece muros · VR: botón Babylon';

    this.root.append(
      this.panel,
      this.circuit,
      this.videoPanel,
      this.toast,
      this.completion,
      this.hints
    );

    this._coinsEl = this.panel.querySelector('[data-coins]');
    this._stepsEl = this.circuit.querySelector('[data-steps]');
  }

  updateCoins(state) {
    if (this._coinsEl) {
      this._coinsEl.textContent = `${state.collected} / ${state.total}`;
    }
    if (state.message) this.showMessage(state.message, state.completed ? 4000 : 2200);
  }

  /**
   * @param {{ steps: Array<{id:string,label:string,done:boolean}>, message?: string, circuitComplete?: boolean }} snap
   */
  updateCircuit(snap) {
    if (!this._stepsEl) return;
    this._stepsEl.innerHTML = snap.steps
      .map(
        (s) =>
          `<li class="${s.done ? 'done' : ''}">` +
          `<span class="check">${s.done ? '✓' : ''}</span>` +
          `<span>${s.label}</span></li>`
      )
      .join('');

    if (snap.message) this.showMessage(snap.message, snap.circuitComplete ? 5000 : 2800);
    if (snap.circuitComplete) this.showCompletion(true);
  }

  showCompletion(show) {
    this.completion.classList.toggle('visible', !!show);
  }

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
