/**
 * DOM HUD: coins, checklist, wall buttons, completion, on-demand video modal.
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
    this._media = media;
    this._videoOpen = false;

    this.panel = document.createElement('div');
    this.panel.className = 'hud-panel';
    this.panel.innerHTML =
      '<div class="hud-panel-title">Recorrido</div>' +
      '<div><strong>Marcadores:</strong> <span data-coins>0 / 0</span></div>';

    this.actions = document.createElement('div');
    this.actions.className = 'hud-actions hud-actions--hidden';
    this.actions.setAttribute('aria-hidden', 'true');
    this.actions.innerHTML = `
      <div class="hud-panel-title">Interacción — muros</div>
      <button type="button" class="hud-btn" data-rise>Levantar muros</button>
      <button type="button" class="hud-btn hud-btn-secondary" data-explode disabled>Visión explotada</button>
      <p class="hud-actions-hint">Acércate a los muros · <kbd>X</kbd> despiece cuando esté disponible</p>
    `;

    this.circuit = document.createElement('div');
    this.circuit.className = 'hud-circuit';
    this.circuit.innerHTML =
      '<div class="hud-panel-title">Circuito — checklist</div><ul data-steps></ul>';

    // Video only as modal (opened from 3D screen click)
    this.videoModal = document.createElement('div');
    this.videoModal.className = 'hud-video-modal';
    this.videoModal.setAttribute('aria-hidden', 'true');
    this.videoModal.innerHTML = `
      <div class="hud-video-modal-backdrop" data-video-close></div>
      <div class="hud-video-modal-card" role="dialog" aria-modal="true" aria-labelledby="video-modal-title">
        <div class="hud-video-modal-header">
          <h2 id="video-modal-title" class="hud-video-modal-title">${media.videoTitle}</h2>
          <button type="button" class="hud-btn hud-btn-close" data-video-close aria-label="Cerrar video">Cerrar</button>
        </div>
        <div class="hud-video-frame">
          <iframe
            data-video-iframe
            title="YouTube video"
            allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
            allowfullscreen
            loading="lazy"
          ></iframe>
        </div>
      </div>
    `;

    this.toast = document.createElement('div');
    this.toast.className = 'hud-toast';
    this.toast.setAttribute('role', 'status');

    this.completion = document.createElement('div');
    this.completion.className = 'hud-completion';
    this.completion.setAttribute('aria-hidden', 'true');
    this.completion.innerHTML =
      '<div class="hud-completion-card" role="dialog" aria-labelledby="completion-title">' +
      '<h2 id="completion-title">¡Circuito completado!</h2>' +
      '<p>Has recogido los marcadores, recorrido las plataformas y entregado la pelota en los tres sitios. Puedes seguir explorando la escena.</p>' +
      '<button type="button" class="hud-btn hud-completion-accept" data-accept>Aceptar</button>' +
      '</div>';

    this._completionDismissed = false;
    this._completionOpen = false;

    this.hints = document.createElement('div');
    this.hints.className = 'hud-hints';
    this.hints.innerHTML =
      '<kbd>WASD</kbd> mover · <kbd>Q</kbd>/<kbd>E</kbd> girar · <kbd>clic der.</kbd> orbitar · <kbd>R</kbd> tomar · <kbd>F</kbd> soltar · cerca de muros = botones · clic pantalla = video';

    this.root.append(
      this.panel,
      this.actions,
      this.circuit,
      this.videoModal,
      this.toast,
      this.completion,
      this.hints
    );

    this._coinsEl = this.panel.querySelector('[data-coins]');
    this._stepsEl = this.circuit.querySelector('[data-steps]');
    this._riseBtn = this.actions.querySelector('[data-rise]');
    this._explodeBtn = this.actions.querySelector('[data-explode]');
    this._videoIframe = this.videoModal.querySelector('[data-video-iframe]');
    this._videoTitleEl = this.videoModal.querySelector('#video-modal-title');

    this._riseBtn?.addEventListener('click', () => {
      this._actions.onRiseWalls?.();
    });
    this._explodeBtn?.addEventListener('click', () => {
      this._actions.onExplodeWalls?.();
    });

    this.completion.querySelector('[data-accept]')?.addEventListener('click', () => {
      this.dismissCompletion();
    });

    this.videoModal.querySelectorAll('[data-video-close]').forEach((el) => {
      el.addEventListener('click', () => this.closeVideo());
    });

    this._onKeyDown = (e) => {
      if (e.code === 'Escape' && this._videoOpen) {
        this.closeVideo();
      }
    };
    window.addEventListener('keydown', this._onKeyDown);
  }

  /**
   * Open fullscreen video player from 3D screen interaction.
   * @param {{ url?: string, title?: string }} [opts]
   */
  openVideo(opts = {}) {
    const url = opts.url ?? this._media.youtubeEmbedUrl;
    const title = opts.title ?? this._media.videoTitle;
    if (this._videoTitleEl) this._videoTitleEl.textContent = title;
    // Force reload with autoplay when opening
    const autoplayUrl = url.includes('autoplay=1')
      ? url
      : `${url}${url.includes('?') ? '&' : '?'}autoplay=1`;
    if (this._videoIframe) {
      this._videoIframe.src = autoplayUrl;
    }
    this._videoOpen = true;
    this.videoModal.classList.add('visible');
    this.videoModal.setAttribute('aria-hidden', 'false');
  }

  closeVideo() {
    this._videoOpen = false;
    this.videoModal.classList.remove('visible');
    this.videoModal.setAttribute('aria-hidden', 'true');
    // Stop playback by clearing src
    if (this._videoIframe) {
      this._videoIframe.src = '';
    }
  }

  setRiseButtonEnabled(enabled) {
    if (this._riseBtn) this._riseBtn.disabled = !enabled;
  }

  setExplodeButtonEnabled(enabled) {
    if (this._explodeBtn) this._explodeBtn.disabled = !enabled;
  }

  /**
   * Proximity UI: show wall animation controls only when near the assembly.
   * @param {boolean} visible
   */
  setWallActionsVisible(visible) {
    this.actions.classList.toggle('hud-actions--hidden', !visible);
    this.actions.setAttribute('aria-hidden', visible ? 'false' : 'true');
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

    if (snap.message && !snap.circuitComplete) {
      this.showMessage(snap.message, 2800);
    } else if (snap.message && snap.circuitComplete && !this._completionDismissed) {
      this.showMessage(snap.message, 3500);
    }

    if (snap.circuitComplete && !this._completionDismissed && !this._completionOpen) {
      this.showCompletion(true);
    }
  }

  showCompletion(show) {
    this._completionOpen = !!show;
    this.completion.classList.toggle('visible', !!show);
    this.completion.setAttribute('aria-hidden', show ? 'false' : 'true');
  }

  dismissCompletion() {
    this._completionDismissed = true;
    this.showCompletion(false);
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
    window.removeEventListener('keydown', this._onKeyDown);
    this.closeVideo();
    if (this._toastTimer) clearTimeout(this._toastTimer);
    this.root.replaceChildren();
  }
}

export default HUD;
