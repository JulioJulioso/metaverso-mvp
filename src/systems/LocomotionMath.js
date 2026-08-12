/**
 * Pure XZ locomotion helpers (no Babylon dependency).
 * Isolated so WASD / XR facing can be unit-tested and debugged separately.
 *
 * Convention (matches CameraRigSystem orbit):
 * - yaw = 0 → camera sits on -Z looking toward +Z
 * - look forward on XZ = (-sin(yaw), cos(yaw))
 * - look right on XZ   = ( cos(yaw), sin(yaw))
 */

/**
 * @param {number} yaw radians (camera orbit yaw)
 * @returns {{ fx: number, fz: number, rx: number, rz: number }}
 */
export function getLookBasisXZ(yaw) {
  const sin = Math.sin(yaw);
  const cos = Math.cos(yaw);
  return {
    fx: -sin,
    fz: cos,
    rx: cos,
    rz: sin,
  };
}

/**
 * Convert local move (strafe right = +xLocal, forward = +zLocal) into world XZ.
 * @param {number} xLocal
 * @param {number} zLocal
 * @param {number} yaw
 * @returns {{ x: number, z: number }}
 */
export function localMoveToWorldXZ(xLocal, zLocal, yaw) {
  const { fx, fz, rx, rz } = getLookBasisXZ(yaw);
  return {
    x: xLocal * rx + zLocal * fx,
    z: xLocal * rz + zLocal * fz,
  };
}

/**
 * Normalize a 2D vector; returns zeros if length ~0.
 * @param {number} x
 * @param {number} z
 * @returns {{ x: number, z: number, len: number }}
 */
export function normalizeXZ(x, z) {
  const len = Math.hypot(x, z);
  if (len < 1e-8) return { x: 0, z: 0, len: 0 };
  return { x: x / len, z: z / len, len };
}

/**
 * Heading yaw from a world XZ velocity (for facing the mesh).
 * @param {number} x
 * @param {number} z
 * @returns {number} radians
 */
export function yawFromVelocityXZ(x, z) {
  if (Math.hypot(x, z) < 1e-6) return null;
  // atan2(x, z): 0 → +Z
  return Math.atan2(x, z);
}

export default {
  getLookBasisXZ,
  localMoveToWorldXZ,
  normalizeXZ,
  yawFromVelocityXZ,
};
