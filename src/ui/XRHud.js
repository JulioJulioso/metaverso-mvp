import {
  MeshBuilder,
  Vector3,
  StandardMaterial,
  Color3,
} from '@babylonjs/core';
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
 * In-XR HUD on a plane ~2m in front of the headset (not fullscreen near-plane).
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
    this.scene = scene;
    this._opts = opts;
    this._visible = false;
    this._nearWalls = false;
    this._toastTimer = null;
    this._cam = null;

    // World panel sized for readable text at ~2m
    this.plane = MeshBuilder.CreatePlane(
      'xrHudPlane',
      { width: 1.35, height: 0.95 },
      scene
    );
    this.plane.isPickable = true;
    this.plane.isVisible = false;
    this.plane.renderingGroupId = 1;

    const mat = new StandardMaterial('xrHudPlaneMat', scene);
    mat.emissiveColor = new Color3(0.02, 0.02, 0.03);
    mat.disableLighting = true;
    mat.backFaceCulling = false;
    this.plane.material = mat;

    this.ui = AdvancedDynamicTexture.CreateForMesh(
      this.plane,
      1024,
      720,
      false
    );

    this.root = new Rectangle('xrHudRoot');
    this.root.width = 1;
    this.root.height = 1;
    this.root.thickness = 0;
    this.root.background = 'rgba(6,8,10,0.82)';
    this.root.cornerRadius = 24;
    this.ui.addControl(this.root);

    const circuitStack = new StackPanel('xrCircuitStack');
    circuitStack.isVertical = true;
    circuitStack.width = 0.92;
    circuitStack.horizontalAlignment = Control.HORIZONTAL_ALIGNMENT_CENTER;
    circuitStack.verticalAlignment = Control.VERTICAL_ALIGNMENT_TOP;
    circuitStack.top = '18px';
    this.root.addControl(circuitStack);

    this.circuitTitle = new TextBlock('xrCircuitTitle');
    this.circuitTitle.text = 'Circuito';
    this.circuitTitle.color = 'white';
    this.circuitTitle.fontSize = 36;
    this.circuitTitle.height = '48px';
    this.circuitTitle.fontWeight = 'bold';
    this.circuitTitle.textHorizontalAlignment = Control.HORIZONTAL_ALIGNMENT_LEFT;
    circuitStack.addControl(this.circuitTitle);

    this.coinsText = new TextBlock('xrCoins');
    this.coinsText.text = 'Marcadores: 0 / 0';
    this.coinsText.color = 'rgba(220,230,240,0.95)';
    this.coinsText.fontSize = 28;
    this.coinsText.height = '40px';
    this.coinsText.textHorizontalAlignment = Control.HORIZONTAL_ALIGNMENT_LEFT;
    circuitStack.addControl(this.coinsText);

    const scroll = new ScrollViewer('xrStepsScroll');
    scroll.width = 1;
    scroll.height = '380px';
    scroll.thickness = 0;
    scroll.barColor = 'rgba(255,255,255,0.3)';
    circuitStack.addControl(scroll);

    this.stepsText = new TextBlock('xrSteps');
    this.stepsText.text = '';
    this.stepsText.color = 'rgba(235,238,242,0.95)';
    this.stepsText.fontSize = 26;
    this.stepsText.textWrapping = true;
    this.stepsText.resizeToFit = true;
    this.stepsText.lineSpacing = '8px';
    this.stepsText.textHorizontalAlignment = Control.HORIZONTAL_ALIGNMENT_LEFT;
    this.stepsText.textVerticalAlignment = Control.VERTICAL_ALIGNMENT_TOP;
    scroll.addControl(this.stepsText);

    this.wallPanel = new Rectangle('xrWalls');
    this.wallPanel.width = 0.92;
    this.wallPanel.height = '150px';
    this.wallPanel.cornerRadius = 12;
    this.wallPanel.thickness = 1;
    this.wallPanel.color = 'rgba(255,255,255,0.2)';
    this.wallPanel.background = 'rgba(20,28,36,0.95)';
    this.wallPanel.verticalAlignment = Control.VERTICAL_ALIGNMENT_BOTTOM;
    this.wallPanel.top = '-100px';
    this.wallPanel.isVisible = false;
    this.root.addControl(this.wallPanel);

    const wallStack = new StackPanel('xrWallStack');
    wallStack.isVertical = true;
    wallStack.width = 0.95;
    this.wallPanel.addControl(wallStack);

    const wallTitle = new TextBlock('xrWallTitle');
    wallTitle.text = 'Muros — Y levantar · X despiece';
    wallTitle.color = 'white';
    wallTitle.fontSize = 24;
    wallTitle.height = '36px';
    wallStack.addControl(wallTitle);

    this.riseBtn = Button.CreateSimpleButton('xrRise', 'Levantar muros');
    this.riseBtn.height = '48px';
    this.riseBtn.thickness = 0;
    this.riseBtn.color = 'white';
    this.riseBtn.background = '#2a6f97';
    this.riseBtn.cornerRadius = 8;
    this.riseBtn.fontSize = 24;
    this.riseBtn.onPointerUpObservable.add(() => this._opts.onRiseWalls?.());
    wallStack.addControl(this.riseBtn);

    this.explodeBtn = Button.CreateSimpleButton('xrExplode', 'Visión explotada');
    this.explodeBtn.height = '48px';
    this.explodeBtn.thickness = 0;
    this.explodeBtn.color = 'white';
    this.explodeBtn.background = '#445566';
    this.explodeBtn.cornerRadius = 8;
    this.explodeBtn.fontSize = 24;
    this.explodeBtn.paddingTop = '8px';
    this.explodeBtn.isEnabled = false;
    this.explodeBtn.onPointerUpObservable.add(() => this._opts.onExplodeWalls?.());
    wallStack.addControl(this.explodeBtn);

    this.hints = new TextBlock('xrHints');
    this.hints.text =
      'Stick izq mover · der girar · A / click stick der = SALTAR · gatillo = tomar · grip = soltar';
    this.hints.color = 'rgba(220,224,230,0.9)';
    this.hints.fontSize = 22;
    this.hints.height = '56px';
    this.hints.textWrapping = true;
    this.hints.verticalAlignment = Control.VERTICAL_ALIGNMENT_BOTTOM;
    this.hints.top = '-18px';
    this.root.addControl(this.hints);

    this.version = new TextBlock('xrVersion');
    this.version.text = opts.versionLabel || '';
    this.version.color = 'rgba(200,210,220,0.85)';
    this.version.fontSize = 20;
    this.version.height = '28px';
    this.version.horizontalAlignment = Control.HORIZONTAL_ALIGNMENT_RIGHT;
    this.version.verticalAlignment = Control.VERTICAL_ALIGNMENT_TOP;
    this.version.left = '-16px';
    this.version.top = '12px';
    this.version.textHorizontalAlignment = Control.HORIZONTAL_ALIGNMENT_RIGHT;
    this.root.addControl(this.version);

    this.toast = new Rectangle('xrToast');
    this.toast.width = 0.9;
    this.toast.height = '64px';
    this.toast.cornerRadius = 10;
    this.toast.thickness = 0;
    this.toast.background = 'rgba(12,14,16,0.92)';
    this.toast.verticalAlignment = Control.VERTICAL_ALIGNMENT_TOP;
    this.toast.top = '56px';
    this.toast.isVisible = false;
    this.root.addControl(this.toast);

    this.toastText = new TextBlock('xrToastText');
    this.toastText.color = 'white';
    this.toastText.fontSize = 26;
    this.toastText.textWrapping = true;
    this.toast.addControl(this.toastText);
  }

  /**
   * Parent HUD plane to XR camera at a comfortable reading distance.
   * @param {import('@babylonjs/core').Camera|null} camera
   */
  attachToCamera(camera) {
    this._cam = camera;
    if (!camera) {
      this.plane.parent = null;
      return;
    }
    this.plane.parent = camera;
    // In front of eyes (~2m), slightly down and left so it does not block center gaze
    this.plane.position = new Vector3(-0.35, -0.12, 2.05);
    // Face the user (plane default faces +Z; camera looks +Z → flip)
    this.plane.rotation.set(0, Math.PI, 0);
  }

  setVisible(visible) {
    this._visible = !!visible;
    this.plane.isVisible = this._visible;
    if (this._visible && this._cam) {
      this.attachToCamera(this._cam);
    }
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
    this.plane.dispose();
  }
}

export default XRHud;
