/**
 * Lightweight AABB / capsule helpers (no full physics engine).
 */

export function isNear(a, b, radius) {
  const dx = a.x - b.x;
  const dy = a.y - b.y;
  const dz = a.z - b.z;
  return dx * dx + dy * dy + dz * dz <= radius * radius;
}

export function isNearXZ(a, b, radius) {
  const dx = a.x - b.x;
  const dz = a.z - b.z;
  return dx * dx + dz * dz <= radius * radius;
}

/**
 * Normalize platform collision records to include bottomY.
 * @param {{ minX:number, maxX:number, minZ:number, maxZ:number, topY:number, bottomY?: number, sizeY?: number }} p
 */
export function withBottom(p) {
  if (p.bottomY != null) return p;
  const sizeY = p.sizeY ?? 0.3;
  return { ...p, bottomY: p.topY - sizeY };
}

/**
 * Highest standable surface under the player.
 *
 * @param {{ x: number, z: number }} playerPosition
 * @param {Array<{ minX:number, maxX:number, minZ:number, maxZ:number, topY:number }>} platforms
 * @param {number} baseY
 * @param {{ feetY: number, maxStepHeight?: number, margin?: number, velocityY?: number, grounded?: boolean }} opts
 */
export function getGroundHeightAt(playerPosition, platforms, baseY = 0, opts = {}) {
  const margin = opts.margin ?? 0.08;
  const feetY = opts.feetY ?? baseY;
  const maxStep = opts.maxStepHeight ?? 0.45;
  const velocityY = opts.velocityY ?? 0;
  const grounded = opts.grounded ?? velocityY <= 0.05;

  let best = baseY;
  const x = playerPosition.x;
  const z = playerPosition.z;

  for (const raw of platforms) {
    const p = withBottom(raw);
    if (
      x < p.minX - margin ||
      x > p.maxX + margin ||
      z < p.minZ - margin ||
      z > p.maxZ + margin
    ) {
      continue;
    }

    const top = p.topY;
    const rise = top - feetY;

    // Already on / near top (standing or landing)
    if (feetY >= top - 0.4 && feetY <= top + 0.55 && velocityY <= 1.0) {
      if (top > best) best = top;
      continue;
    }

    // Walk up small ledges while grounded
    if (grounded && rise > 0 && rise <= maxStep) {
      if (top > best) best = top;
      continue;
    }

    // Coming down onto the surface
    if (velocityY <= 0 && rise >= -0.08 && rise <= 0.65) {
      if (top > best) best = top;
    }
  }

  return best;
}

/**
 * Push a capsule (XZ circle) out of solid platform AABBs.
 * Skips volumes when the feet are on/above the top (standing allowed).
 *
 * @param {{ x:number, y:number, z:number }} center capsule center
 * @param {number} radius
 * @param {number} height
 * @param {Array<object>} platforms collision data
 * @returns {{ x:number, y:number, z:number }}
 */
export function resolveSolidCapsule(center, radius, height, platforms) {
  let x = center.x;
  let y = center.y;
  let z = center.z;
  const feetY = y - height / 2;
  const headY = y + height / 2;
  const standEps = 0.14;

  for (let pass = 0; pass < 3; pass++) {
    for (const raw of platforms) {
      const p = withBottom(raw);
      const top = p.topY;
      const bottom = p.bottomY ?? top - 0.3;

      // Standing on top → no side block
      if (feetY >= top - standEps) continue;
      // Fully below or above the box
      if (headY < bottom + 0.02) continue;
      if (feetY > top + 0.02) continue;

      // Expand AABB by capsule radius
      const minX = p.minX - radius;
      const maxX = p.maxX + radius;
      const minZ = p.minZ - radius;
      const maxZ = p.maxZ + radius;

      if (x < minX || x > maxX || z < minZ || z > maxZ) continue;

      const pushLeft = x - minX;
      const pushRight = maxX - x;
      const pushDown = z - minZ;
      const pushUp = maxZ - z;
      const m = Math.min(pushLeft, pushRight, pushDown, pushUp);

      if (m === pushLeft) x = minX;
      else if (m === pushRight) x = maxX;
      else if (m === pushDown) z = minZ;
      else z = maxZ;
    }
  }

  return { x, y, z };
}

/**
 * Try to step the feet up onto a platform top when overlapping its footprint.
 * @returns {number|null} new feet Y if stepped, else null
 */
export function tryStepUp(feetY, x, z, platforms, maxStep, margin = 0.08) {
  let best = null;
  for (const raw of platforms) {
    const p = withBottom(raw);
    if (
      x < p.minX - margin ||
      x > p.maxX + margin ||
      z < p.minZ - margin ||
      z > p.maxZ + margin
    ) {
      continue;
    }
    const rise = p.topY - feetY;
    if (rise > 0.02 && rise <= maxStep) {
      if (best == null || p.topY > best) best = p.topY;
    }
  }
  return best;
}

export const CollisionSystem = {
  isNear,
  isNearXZ,
  getGroundHeightAt,
  resolveSolidCapsule,
  tryStepUp,
  withBottom,
};

export default CollisionSystem;
