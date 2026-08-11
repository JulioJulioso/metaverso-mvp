/**
 * DOM HUD: coins, circuit checklist, YouTube embed, wall action buttons, completion.
 */
export class HUD {
  /**
   * @param {HTMLElement} root
   * @param {{ youtubeEmbedUrl: string, videoTitle: string }} media
   * @param {{ onRiseWalls?: () => void, onExplodeWalls?: () => void }} [actions]
   */
  constructor(root, media, actions = {}) {
    this.root = root;
    this._toastTimer = null;
    this._actions = actions;

    this.panel = document.createElement('div');
    this.panel.className = 'hud-panel';
    this.panel.innerHTML =
      '<div class="hud-panel-title">Recorrido</div>' +
      '<div><strong>Marcadores:</strong> <span data-coins>0 / 0</span></div>';

    this.actions = document.createElement('div');
    this.actions.className = 'hud-actions';
    this.actions.innerHTML = `
      <div class="hud-panel-title">Animaciones</div>
      <button type="button" class="hud-btn" data-rise>Levantar muros</button>
      <button type="button" class="hud-btn hud-btn-secondary" data-explode disabled>Visión explotada (X)</button>
      <p class="hud-actions-hint">También: <kbd>R</kbd> levantar · <kbd>X</kbd> despiece</p>
    `;

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
      '<kbd>WASD</kbd> mover · <kbd>Espacio</kbd> saltar · <kbd>E</kbd> recoger · <kbd>F</kbd> soltar · <kbd>R</kbd> levantar muros · <kbd>X</kbd> despiece · VR: botón Babylon';

    this.root.append(
      this.panel,
      this.actions,
      this.circuit,
      this.videoPanel,
      this.toast,
      this.completion,
      this.hints
    );

    this._coinsEl = this.panel.querySelector('[data-coins]');
    this._stepsEl = this.circuit.querySelector('[data-steps]');
    this._riseBtn = this.actions.querySelector('[data-rise]');
    this._explodeBtn = this.actions.querySelector('[data-explode]');

    this._riseBtn?.addEventListener('click', () => {
      this._actions.onRiseWalls?.();
    });
    this._explodeBtn?.addEventListener('click', () => {
      this._actions.onExplodeWalls?.();
    });
  }

  setRiseButtonEnabled(enabled) {
    if (this._riseBtn) this._riseBtn.disabled = !enabled;
  }

  setExplodeButtonEnabled(enabled) {
    if (this._explodeBtn) this._explodeBtn.disabled = !enabled;
  }

  markRiseStarted() {
    if (this._riseBtn) {
      this._riseBtn.disabled = true;
      this._riseBtn.textContent = 'Levantando…';
    }
  }

  markRiseDone() {
    if (this._riseBtn) {
      this._riseBtn.disabled = true;
      this._riseBtn.textContent = 'Muros levantados';
    }
    this.setExplodeButtonEnabled(true);
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
