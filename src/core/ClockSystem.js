/**
 * Centralized delta time (seconds) for animation and simple physics.
 */
export class ClockSystem {
  constructor() {
    this._lastMs = performance.now();
    this.delta = 0;
    this.elapsed = 0;
  }

  /** Call once per frame; returns delta in seconds (clamped). */
  tick() {
    const now = performance.now();
    let dt = (now - this._lastMs) / 1000;
    this._lastMs = now;
    // Avoid spiral of death on tab background stalls
    if (dt > 0.1) dt = 0.1;
    if (dt < 0) dt = 0;
    this.delta = dt;
    this.elapsed += dt;
    return this.delta;
  }

  reset() {
    this._lastMs = performance.now();
    this.delta = 0;
    this.elapsed = 0;
  }
}

export default ClockSystem;
