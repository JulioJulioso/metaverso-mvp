import {
  AdvancedDynamicTexture,
  Rectangle,
  TextBlock,
  Button,
  StackPanel,
  Control,
  ScrollViewer,
} from '@babylonjs/gui';

/**
 * In-XR HUD (DOM overlays are invisible in immersive-vr).
 * Mirrors checklist, wall actions, toasts, and build version.
 */
export class XRHud {
  /**
   * @param {import('@babylonjs/core').Scene} scene
   * @param {{
   *   versionLabel?: string,
   *   onRiseWalls?: () => void,
   *   onExplodeWalls?: () => void,
   * }} [opts]
   */
  constructor(scene, opts = {}) {
    this._opts = opts;
    this._visible = false;
    this._nearWalls = false;
    this._toastTimer = null;

    this.ui = AdvancedDynamicTexture.CreateFullscreenUI('xrHudUI', true, scene);
    this.ui.idealWidth = 1200;

    this.root = new Rectangle('xrHudRoot');
    this.root.width = 1;
    this.root.height = 1;
    this.root.thickness = 0;
    this.root.background = 'transparent';
    this.root.isVisible = false;
    this.ui.addControl(this.root);

    // Checklist panel (left)
    this.circuitPanel = new Rectangle('xrCircuit');
    this.circuitPanel.width = '360px';
    this.circuitPanel.height = '420px';
    this.circuitPanel.cornerRadius = 10;
    this.circuitPanel.thickness = 1;
    this.circuitPanel.color = 'rgba(255,255,255,0.2)';
    this.circuitPanel.background = 'rgba(8,10,12,0.72)';
    this.circuitPanel.horizontalAlignment = Control.HORIZONTAL_ALIGNMENT_LEFT;
    this.circuitPanel.verticalAlignment = Control.VERTICAL_ALIGNMENT_TOP;
    this.circuitPanel.left = '24px';
    this.circuitPanel.top = '24px';
    this.circuitPanel.paddingTop = '12px';
    this.circuitPanel.paddingLeft = '12px';
    this.circuitPanel.paddingRight = '12px';
    this.circuitPanel.paddingBottom = '12px';
    this.root.addControl(this.circuitPanel);

    const circuitStack = new StackPanel('xrCircuitStack');
    circuitStack.isVertical = true;
    circuitStack.width = 1;
    this.circuitPanel.addControl(circuitStack);

    this.circuitTitle = new TextBlock('xrCircuitTitle');
    this.circuitTitle.text = 'Circuito';
    this.circuitTitle.color = 'white';
    this.circuitTitle.fontSize = 22;
    this.circuitTitle.height = '32px';
    this.circuitTitle.textHorizontalAlignment = Control.HORIZONTAL_ALIGNMENT_LEFT;
    circuitStack.addControl(this.circuitTitle);

    this.coinsText = new TextBlock('xrCoins');
    this.coinsText.text = 'Marcadores: 0 / 0';
    this.coinsText.color = 'rgba(220,230,240,0.9)';
    this.coinsText.fontSize = 16;
    this.coinsText.height = '28px';
    this.coinsText.textHorizontalAlignment = Control.HORIZONTAL_ALIGNMENT_LEFT;
    circuitStack.addControl(this.coinsText);

    const scroll = new ScrollViewer('xrStepsScroll');
    scroll.width = 1;
    scroll.height = '320px';
    scroll.thickness = 0;
    scroll.barColor = 'rgba(255,255,255,0.25)';
    circuitStack.addControl(scroll);

    this.stepsText = new TextBlock('xrSteps');
    this.stepsText.text = '';
    this.stepsText.color = 'rgba(230,234,238,0.92)';
    this.stepsText.fontSize = 15;
    this.stepsText.textWrapping = true;
    this.stepsText.resizeToFit = true;
    this.stepsText.textHorizontalAlignment = Control.HORIZONTAL_ALIGNMENT_LEFT;
    this.stepsText.textVerticalAlignment = Control.VERTICAL_ALIGNMENT_TOP;
    this.stepsText.paddingTop = '4px';
    scroll.addControl(this.stepsText);

    // Wall actions (center-bottom when near)
    this.wallPanel = new Rectangle('xrWalls');
    this.wallPanel.width = '420px';
    this.wallPanel.height = '150px';
    this.wallPanel.cornerRadius = 10;
    this.wallPanel.thickness = 1;
    this.wallPanel.color = 'rgba(255,255,255,0.18)';
    this.wallPanel.background = 'rgba(8,10,12,0.78)';
    this.wallPanel.horizontalAlignment = Control.HORIZONTAL_ALIGNMENT_CENTER;
    this.wallPanel.verticalAlignment = Control.VERTICAL_ALIGNMENT_BOTTOM;
    this.wallPanel.top = '-90px';
    this.wallPanel.isVisible = false;
    this.root.addControl(this.wallPanel);

    const wallStack = new StackPanel('xrWallStack');
    wallStack.isVertical = true;
    wallStack.width = 0.95;
    this.wallPanel.addControl(wallStack);

    const wallTitle = new TextBlock('xrWallTitle');
    wallTitle.text = 'Muros (Y = levantar · X = despiece)';
    wallTitle.color = 'white';
    wallTitle.fontSize = 16;
    wallTitle.height = '28px';
    wallStack.addControl(wallTitle);

    this.riseBtn = Button.CreateSimpleButton('xrRise', 'Levantar muros');
    this.riseBtn.height = '40px';
    this.riseBtn.thickness = 0;
    this.riseBtn.color = 'white';
    this.riseBtn.background = '#2a6f97';
    this.riseBtn.cornerRadius = 6;
    this.riseBtn.paddingTop = '4px';
    this.riseBtn.onPointerUpObservable.add(() => this._opts.onRiseWalls?.());
    wallStack.addControl(this.riseBtn);

    this.explodeBtn = Button.CreateSimpleButton('xrExplode', 'Visión explotada');
    this.explodeBtn.height = '40px';
    this.explodeBtn.thickness = 0;
    this.explodeBtn.color = 'white';
    this.explodeBtn.background = '#445566';
    this.explodeBtn.cornerRadius = 6;
    this.explodeBtn.paddingTop = '6px';
    this.explodeBtn.isEnabled = false;
    this.explodeBtn.onPointerUpObservable.add(() => this._opts.onExplodeWalls?.());
    wallStack.addControl(this.explodeBtn);

    // Hints
    this.hints = new TextBlock('xrHints');
    this.hints.text =
      'Stick izq mover · der girar · gatillo interactuar · A saltar · grip soltar · apuntar pantalla = video';
    this.hints.color = 'rgba(220,224,230,0.75)';
    this.hints.fontSize = 14;
    this.hints.height = '36px';
    this.hints.horizontalAlignment = Control.HORIZONTAL_ALIGNMENT_CENTER;
    this.hints.verticalAlignment = Control.VERTICAL_ALIGNMENT_BOTTOM;
    this.hints.top = '-28px';
    this.root.addControl(this.hints);

    // Version
    this.version = new TextBlock('xrVersion');
    this.version.text = opts.versionLabel || '';
    this.version.color = 'rgba(220,224,230,0.8)';
    this.version.fontSize = 14;
    this.version.height = '28px';
    this.version.width = '280px';
    this.version.horizontalAlignment = Control.HORIZONTAL_ALIGNMENT_RIGHT;
    this.version.verticalAlignment = Control.VERTICAL_ALIGNMENT_BOTTOM;
    this.version.left = '-16px';
    this.version.top = '-16px';
    this.version.textHorizontalAlignment = Control.HORIZONTAL_ALIGNMENT_RIGHT;
    this.root.addControl(this.version);

    // Toast
    this.toast = new Rectangle('xrToast');
    this.toast.width = '520px';
    this.toast.height = '56px';
    this.toast.cornerRadius = 8;
    this.toast.thickness = 0;
    this.toast.background = 'rgba(12,14,16,0.85)';
    this.toast.horizontalAlignment = Control.HORIZONTAL_ALIGNMENT_CENTER;
    this.toast.verticalAlignment = Control.VERTICAL_ALIGNMENT_TOP;
    this.toast.top = '40px';
    this.toast.isVisible = false;
    this.root.addControl(this.toast);

    this.toastText = new TextBlock('xrToastText');
    this.toastText.color = 'white';
    this.toastText.fontSize = 18;
    this.toastText.textWrapping = true;
    this.toast.addControl(this.toastText);
  }

  setVisible(visible) {
    this._visible = !!visible;
    this.root.isVisible = this._visible;
  }

  setWallActionsVisible(visible) {
    this._nearWalls = !!visible;
    this.wallPanel.isVisible = this._visible && this._nearWalls;
  }

  setRiseButtonEnabled(enabled) {
    this.riseBtn.isEnabled = !!enabled;
  }

  setExplodeButtonEnabled(enabled) {
    this.explodeBtn.isEnabled = !!enabled;
    this.explodeBtn.background = enabled ? '#2a6f97' : '#445566';
  }

  markRiseStarted() {
    this.riseBtn.isEnabled = false;
    this.riseBtn.textBlock.text = 'Levantando…';
  }

  markRiseDone() {
    this.riseBtn.isEnabled = false;
    this.riseBtn.textBlock.text = 'Muros levantados';
    this.setExplodeButtonEnabled(true);
  }

  updateCoins(state) {
    this.coinsText.text = `Marcadores: ${state.collected} / ${state.total}`;
  }

  /**
   * @param {{ steps: Array<{id:string,label:string,done:boolean}>, message?: string }} snap
   */
  updateCircuit(snap) {
    this.stepsText.text = (snap.steps || [])
      .map((s) => `${s.done ? '[x]' : '[ ]'} ${s.label}`)
      .join('\n');
    if (snap.message) this.showMessage(snap.message, 2800);
  }

  showMessage(text, ms = 2200) {
    this.toastText.text = text;
    this.toast.isVisible = this._visible;
    if (this._toastTimer) clearTimeout(this._toastTimer);
    this._toastTimer = setTimeout(() => {
      this.toast.isVisible = false;
    }, ms);
  }

  setVersion(label) {
    this.version.text = label || '';
  }

  dispose() {
    if (this._toastTimer) clearTimeout(this._toastTimer);
    this.ui.dispose();
  }
}

export default XRHud;
