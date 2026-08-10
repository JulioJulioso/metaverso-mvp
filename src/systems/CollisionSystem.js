/**
 * Lightweight proximity / ground helpers (no full physics engine).
 */

/**
 * @param {{ x: number, y: number, z: number }|import('@babylonjs/core').Vector3} a
 * @param {{ x: number, y: number, z: number }|import('@babylonjs/core').Vector3} b
 * @param {number} radius meters
 */
export function isNear(a, b, radius) {
  const dx = a.x - b.x;
  const dy = a.y - b.y;
  const dz = a.z - b.z;
  return dx * dx + dy * dy + dz * dz <= radius * radius;
}

/**
 * Horizontal proximity (XZ), useful for pickups on floor.
 */
export function isNearXZ(a, b, radius) {
  const dx = a.x - b.x;
  const dz = a.z - b.z;
  return dx * dx + dz * dz <= radius * radius;
}

/**
 * Highest platform top under the player XZ, else base ground Y.
 * @param {{ x: number, z: number }} playerPosition
 * @param {Array<{ minX:number, maxX:number, minZ:number, maxZ:number, topY:number }>} platforms
 * @param {number} baseY
 * @param {number} [margin]
 */
export function getGroundHeightAt(playerPosition, platforms, baseY = 0, margin = 0.05) {
  let best = baseY;
  const x = playerPosition.x;
  const z = playerPosition.z;

  for (const p of platforms) {
    if (
      x >= p.minX - margin &&
      x <= p.maxX + margin &&
      z >= p.minZ - margin &&
      z <= p.maxZ + margin
    ) {
      if (p.topY > best) best = p.topY;
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
