/**
 * Placeholder for future multiplayer / presence.
 * Other systems may import this API now without coupling to a real network stack.
 */
export const NetworkStub = {
  isConnected: false,

  onPlayerJoin(_handler) {
    /* no-op until multiplayer */
  },

  onPlayerLeave(_handler) {
    /* no-op until multiplayer */
  },

  /** Push local state for replication (future). */
  sendState(_state) {
    /* no-op */
  },

  /** Apply remote snapshot (future). */
  syncState(_state) {
    /* no-op */
  },

  connect(_url) {
    console.info('[NetworkStub] connect() is a no-op in MVP single-user build.');
    return Promise.resolve(false);
  },

  disconnect() {
    /* no-op */
  },
};

export default NetworkStub;
