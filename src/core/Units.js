/** Canonical runtime unit: meters. BIM properties typically use mm in sidecars. */
export const Units = {
  /** 1 Babylon world unit */
  METERS: 'm',
  MM_TO_M: 0.001,
  M_TO_MM: 1000,

  mmToM(mm) {
    return mm * this.MM_TO_M;
  },

  mToMm(m) {
    return m * this.M_TO_MM;
  },

  /** Convert a Vec3-like from millimeters to meters. */
  vec3MmToM(v) {
    return { x: this.mmToM(v.x), y: this.mmToM(v.y), z: this.mmToM(v.z) };
  },
};

export default Units;
