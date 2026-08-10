/**
 * Ordered circuit checklist (replaces single "coins only" mission).
 */
export class CircuitSystem {
  /**
   * @param {Array<{ id: string, label: string }>} steps
   */
  constructor(steps) {
    this.steps = steps.map((s) => ({ ...s, done: false }));
    /** @type {Set<(snapshot: object) => void>} */
    this._listeners = new Set();
    this.circuitComplete = false;
  }

  onChange(cb) {
    this._listeners.add(cb);
    cb(this.getSnapshot());
    return () => this._listeners.delete(cb);
  }

  _emit(message) {
    for (const cb of this._listeners) {
      cb({ ...this.getSnapshot(), message });
    }
  }

  isDone(id) {
    return this.steps.find((s) => s.id === id)?.done ?? false;
  }

  /**
   * @param {string} id
   * @param {string} [message]
   */
  complete(id, message) {
    const step = this.steps.find((s) => s.id === id);
    if (!step || step.done) return false;
    step.done = true;
    this._emit(message ?? `Completado: ${step.label}`);

    const coreIds = this.steps
      .filter((s) => s.id !== 'circuit_done')
      .every((s) => s.done);

    if (coreIds && !this.isDone('circuit_done')) {
      const doneStep = this.steps.find((s) => s.id === 'circuit_done');
      if (doneStep) {
        doneStep.done = true;
        this.circuitComplete = true;
        this._emit('¡Circuito completado!');
      }
    }
    return true;
  }

  getSnapshot() {
    const done = this.steps.filter((s) => s.done).length;
    return {
      steps: this.steps.map((s) => ({ ...s })),
      doneCount: done,
      total: this.steps.length,
      circuitComplete: this.circuitComplete,
      coinsLabel: null,
    };
  }
}

export default CircuitSystem;
