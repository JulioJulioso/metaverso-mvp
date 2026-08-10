import { isNear } from './CollisionSystem.js';

/**
 * Generic interactables (Spatial-like triggers): onEnter / onInteract / autoCollect.
 */
export class InteractionSystem {
  constructor() {
    /** @type {Map<string, object>} */
    this._items = new Map();
    this._inside = new Set();
  }

  /**
   * @param {{
   *   id: string,
   *   getPosition: () => {x:number,y:number,z:number},
   *   radius: number,
   *   enabled?: () => boolean,
   *   autoCollect?: boolean,
   *   onEnter?: () => void,
   *   onExit?: () => void,
   *   onInteract?: () => void,
   * }} item
   */
  register(item) {
    this._items.set(item.id, item);
  }

  unregister(id) {
    this._items.delete(id);
    this._inside.delete(id);
  }

  /**
   * @param {{x:number,y:number,z:number}} playerPos
   * @param {{ interact?: boolean }} inputEdges
   */
  update(playerPos, inputEdges = {}) {
    for (const item of this._items.values()) {
      if (item.enabled && !item.enabled()) continue;

      const pos = item.getPosition();
      const near = isNear(playerPos, pos, item.radius);
      const wasInside = this._inside.has(item.id);

      if (near && !wasInside) {
        this._inside.add(item.id);
        item.onEnter?.();
        if (item.autoCollect) item.onInteract?.();
      } else if (!near && wasInside) {
        this._inside.delete(item.id);
        item.onExit?.();
      }

      if (near && inputEdges.interact && !item.autoCollect) {
        item.onInteract?.();
      }
    }
  }

  clear() {
    this._items.clear();
    this._inside.clear();
  }
}

export default InteractionSystem;
