import { SceneLoader } from '@babylonjs/core';
import '@babylonjs/loaders/glTF';

/**
 * Thin wrapper around Babylon loaders for Unity/Revit-baked GLB runtime assets.
 * Not used by the demo level (primitives); ready for assets/models/.
 */
export class AssetLoader {
  /**
   * @param {import('@babylonjs/core').Scene} scene
   * @param {string} rootUrl e.g. '/models/' or path prefix
   * @param {string} fileName e.g. 'building.glb'
   */
  static async loadGlb(scene, rootUrl, fileName) {
    const result = await SceneLoader.ImportMeshAsync(
      '',
      rootUrl,
      fileName,
      scene
    );
    return result;
  }

  /**
   * Load bim-index JSON sidecar.
   * @param {string} url
   * @returns {Promise<object>}
   */
  static async loadBimIndex(url) {
    const res = await fetch(url);
    if (!res.ok) {
      throw new Error(`[AssetLoader] BIM index fetch failed: ${url} (${res.status})`);
    }
    return res.json();
  }
}

export default AssetLoader;
