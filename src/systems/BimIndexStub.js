/**
 * BIM index query API. Loads real sidecars later; MVP seeds from levelConfig mock.
 */
export class BimIndexStub {
  constructor() {
    /** @type {Map<string, object>} */
    this._byId = new Map();
    this.loaded = false;
  }

  /**
   * @param {object} index bim-index document or { elements: [] }
   */
  loadFromObject(index) {
    this._byId.clear();
    const elements = index?.elements ?? [];
    for (const el of elements) {
      if (el.globalId) this._byId.set(el.globalId, el);
    }
    this.loaded = true;
  }

  /**
   * Seed demo elements from levelConfig.
   * @param {Array<object>} mockElements
   */
  loadMock(mockElements = []) {
    this.loadFromObject({ elements: mockElements });
  }

  getElement(id) {
    return this._byId.get(id) ?? null;
  }

  getProperties(id) {
    return this.getElement(id)?.properties ?? null;
  }

  worldBounds(id) {
    return this.getElement(id)?.bounds ?? null;
  }

  /**
   * Resolve mesh.metadata.bimId through the index.
   * @param {{ metadata?: { bimId?: string } }|null} mesh
   */
  resolveFromMesh(mesh) {
    const id = mesh?.metadata?.bimId;
    if (!id) return null;
    return this.getElement(id) ?? {
      globalId: id,
      name: mesh.metadata?.label ?? id,
      category: mesh.metadata?.entity ?? 'Unknown',
      properties: {},
    };
  }
}

export default BimIndexStub;
