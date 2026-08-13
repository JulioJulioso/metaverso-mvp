import {
  MeshBuilder,
  PBRMaterial,
  Color3,
  Vector3,
  TransformNode,
} from '@babylonjs/core';

/**
 * Five adjacent walls: rise on demand (button/key), then explode / reassemble.
 */
export class WallAssembly {
  /**
   * @param {import('@babylonjs/core').Scene} scene
   * @param {object} config levelConfig.walls
   * @param {import('@babylonjs/core').ShadowGenerator|null} [shadowGen]
   */
  constructor(scene, config, shadowGen = null) {
    this.scene = scene;
    this.config = config;
    this.root = new TransformNode(config.id, scene);
    this.root.position = new Vector3(
      config.origin.x,
      config.origin.y,
      config.origin.z
    );

    this.walls = [];
    this.riseT = 0;
    this.riseStarted = false;
    this.riseDone = false;
    this.explodeT = 0;
    this.exploded = false;
    this._explodeAnimating = false;
    this._explodeTarget = 0;

    const mat = new PBRMaterial('wallMat', scene);
    mat.albedoColor = new Color3(0.72, 0.7, 0.66);
    mat.metallic = 0.05;
    mat.roughness = 0.72;
    mat.environmentIntensity = 0.9;

    const n = config.count;
    const startX = -((n - 1) * config.spacing) / 2;

    for (let i = 0; i < n; i++) {
      const mesh = MeshBuilder.CreateBox(
        `wall-${i}`,
        {
          width: config.width,
          height: config.height,
          depth: config.depth,
        },
        scene
      );
      mesh.parent = this.root;
      mesh.material = mat;
      mesh.receiveShadows = true;
      mesh.isPickable = true;
      mesh.metadata = {
        bimId: `wall-panel-${i + 1}`,
        label: `Muro ${i + 1}`,
        entity: 'wall',
      };

      if (shadowGen) shadowGen.addShadowCaster(mesh);

      const localX = startX + i * config.spacing;
      const assembled = new Vector3(localX, config.height / 2, 0);
      const dir = n === 1 ? 0 : i - (n - 1) / 2;
      const exploded = new Vector3(
        localX + dir * config.explodeDistance,
        config.height / 2 + 0.15,
        (i % 2 === 0 ? 1 : -1) * 0.25
      );

      const buriedY = -config.height / 2 - 0.2;
      mesh.position = new Vector3(localX, buriedY, 0);

      this.walls.push({ mesh, assembled, exploded, buriedY });
    }
  }

  /** Begin rise animation (idempotent). @returns {boolean} started now */
  startRise() {
    if (this.riseStarted || this.riseDone) return false;
    this.riseStarted = true;
    this.riseT = 0;
    return true;
  }

  isRiseStarted() {
    return this.riseStarted;
  }

  isRiseDone() {
    return this.riseDone;
  }

  /** World-space point used for proximity prompts (assembly center). */
  getInteractionPoint() {
    return {
      x: this.root.position.x,
      y: this.root.position.y,
      z: this.root.position.z,
    };
  }

  /**
   * Solid AABBs for player collision. Buried panels are skipped.
   * @returns {Array<{minX:number,maxX:number,minZ:number,maxZ:number,topY:number,bottomY:number,sizeY:number}>}
   */
  getCollisionBoxes() {
    const w = this.config.width;
    const h = this.config.height;
    const d = this.config.depth;
    const boxes = [];
    for (const wall of this.walls) {
      const p = wall.mesh.getAbsolutePosition();
      // Ignore panels still mostly underground
      if (p.y + h / 2 < 0.15) continue;
      boxes.push({
        minX: p.x - w / 2,
        maxX: p.x + w / 2,
        minZ: p.z - d / 2,
        maxZ: p.z + d / 2,
        topY: p.y + h / 2,
        bottomY: p.y - h / 2,
        sizeY: h,
      });
    }
    return boxes;
  }

  /** @returns {boolean} true when rise fully complete this frame */
  update(delta) {
    let justFinishedRise = false;

    if (this.riseStarted && !this.riseDone) {
      this.riseT += delta / this.config.riseDuration;
      if (this.riseT >= 1) {
        this.riseT = 1;
        this.riseDone = true;
        justFinishedRise = true;
      }
      const e = easeOutCubic(this.riseT);
      for (const w of this.walls) {
        const y = w.buriedY + (w.assembled.y - w.buriedY) * e;
        w.mesh.position.x = w.assembled.x;
        w.mesh.position.y = y;
        w.mesh.position.z = w.assembled.z;
      }
    }

    if (this._explodeAnimating) {
      const dir = this._explodeTarget > this.explodeT ? 1 : -1;
      this.explodeT += dir * (delta / this.config.explodeDuration);
      if (
        (dir > 0 && this.explodeT >= this._explodeTarget) ||
        (dir < 0 && this.explodeT <= this._explodeTarget)
      ) {
        this.explodeT = this._explodeTarget;
        this._explodeAnimating = false;
        this.exploded = this.explodeT >= 0.99;
      }
      this._applyExplodePose();
    }

    return justFinishedRise;
  }

  _applyExplodePose() {
    const e = easeInOut(this.explodeT);
    for (const w of this.walls) {
      w.mesh.position.x = w.assembled.x + (w.exploded.x - w.assembled.x) * e;
      w.mesh.position.y = w.assembled.y + (w.exploded.y - w.assembled.y) * e;
      w.mesh.position.z = w.assembled.z + (w.exploded.z - w.assembled.z) * e;
    }
  }

  toggleExplode() {
    if (!this.riseDone) return false;
    this._explodeTarget = this.exploded || this.explodeT > 0.5 ? 0 : 1;
    this._explodeAnimating = true;
    return true;
  }

  isExploded() {
    return this.exploded;
  }

  getExplodeTarget() {
    return this._explodeTarget;
  }

  dispose() {
    for (const w of this.walls) w.mesh.dispose();
    this.root.dispose();
  }
}

function easeOutCubic(t) {
  return 1 - (1 - t) ** 3;
}

function easeInOut(t) {
  return t < 0.5 ? 2 * t * t : 1 - (-2 * t + 2) ** 2 / 2;
}

export default WallAssembly;
