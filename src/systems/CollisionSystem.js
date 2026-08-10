/**
 * Lightweight proximity / ground helpers (no full physics engine).
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
 * Highest reachable ground under the player.
 * Platforms much higher than feet require a jump (they are ignored until feet rise close enough).
 *
 * @param {{ x: number, z: number }} playerPosition
 * @param {Array<{ minX:number, maxX:number, minZ:number, maxZ:number, topY:number }>} platforms
 * @param {number} baseY
 * @param {{ feetY: number, maxStepHeight?: number, margin?: number, airborne?: boolean }} opts
 */
export function getGroundHeightAt(playerPosition, platforms, baseY = 0, opts = {}) {
  const margin = opts.margin ?? 0.06;
  const feetY = opts.feetY ?? baseY;
  const maxStep = opts.maxStepHeight ?? 0.42;
  const airborne = opts.airborne ?? false;

  let best = baseY;
  const x = playerPosition.x;
  const z = playerPosition.z;

  for (const p of platforms) {
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

    // Already on / slightly above top → can stand
    if (feetY >= top - 0.18) {
      if (top > best) best = top;
      continue;
    }

    // Walk up only small ledges
    if (!airborne && rise > 0 && rise <= maxStep) {
      if (top > best) best = top;
      continue;
    }

    // Landing from a jump/fall: feet must come down onto the top surface
    if (airborne && rise >= -0.05 && rise <= 0.55) {
      if (top > best) best = top;
    }
  }

  return best;
}

export const CollisionSystem = {
  isNear,
  isNearXZ,
  getGroundHeightAt,
};

export default CollisionSystem;
