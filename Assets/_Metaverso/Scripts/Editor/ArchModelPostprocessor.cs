using UnityEditor;
using UnityEngine;

namespace Metaverso.EditorTools
{
    public class ArchModelPostprocessor : AssetPostprocessor
    {
        void OnPreprocessModel()
        {
            if (!ArchImportRules.AppliesTo(assetPath))
                return;

            var importer = (ModelImporter)assetImporter;
            importer.globalScale = 1f;
            importer.useFileScale = true;
            importer.importCameras = false;
            importer.importLights = false;
            importer.generateSecondaryUV = true;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            importer.bakeAxisConversion = true;
            importer.meshCompression = ModelImporterMeshCompression.Off;
        }
    }
}
