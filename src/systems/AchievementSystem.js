/**
 * Achievement / mission progress (coins). Events for HUD.
 */
export class AchievementSystem {
  /**
   * @param {number} totalCoins
   */
  constructor(totalCoins) {
    this.total = totalCoins;
    this.collected = 0;
    this.completed = false;
    /** @type {Set<(payload: object) => void>} */
    this._listeners = new Set();
  }

  /**
   * @param {(payload: { collected: number, total: number, completed: boolean, message?: string }) => void} cb
   */
  onChange(cb) {
    this._listeners.add(cb);
    return () => this._listeners.delete(cb);
  }

  _emit(message) {
    const payload = {
      collected: this.collected,
      total: this.total,
      completed: this.completed,
      message,
    };
    for (const cb of this._listeners) cb(payload);
  }

  registerCoinCollected() {
    if (this.collected >= this.total) return;
    this.collected += 1;
    this._emit(`Marcador recogido (${this.collected}/${this.total})`);
    this.checkCompletion();
  }

  checkCompletion() {
    if (this.completed) return false;
    if (this.collected >= this.total && this.total > 0) {
      this.completed = true;
      this._emit('¡Misión completada!');
      return true;
    }
    return false;
  }

  getState() {
    return {
      collected: this.collected,
      total: this.total,
      completed: this.completed,
    };
  }
}

export default AchievementSystem;
