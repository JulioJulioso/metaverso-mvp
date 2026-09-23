using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.XR.Management;
using WebXR;

namespace Metaverso.EditorXR
{
    [InitializeOnLoad]
    static class WebXrAutoSetup
    {
        const string PrefKey = "Metaverso.WebXrSetupDone";

        static WebXrAutoSetup()
        {
            EditorApplication.delayCall += () =>
            {
                if (!EditorPrefs.GetBool(PrefKey, false))
                {
                    WebXrProjectSetup.Configure();
                    EditorPrefs.SetBool(PrefKey, true);
                }

                if (GameObject.Find("Player") != null && GameObject.Find("XR Origin") == null)
                    PlayerRigBuilder.Create();
            };
        }
    }

    public static class WebXrProjectSetup
    {
        [MenuItem("Metaverso/Configurar WebXR")]
        public static void Configure()
        {
            CopyTemplates();
            PlayerSettings.WebGL.template = "PROJECT:WebXRFullView2020";
            EnableLoader();
            Debug.Log("[Metaverso] WebXR: plantillas copiadas, template WebXRFullView2020, loader pedido para WebGL.");
        }

        public static void CopyTemplates()
        {
            var package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(WebXRManager).Assembly);
            if (package == null)
            {
                Debug.LogError("[Metaverso] No encontre el paquete WebXR.");
                return;
            }

            var source = Path.Combine(package.resolvedPath, "Hidden~");
            if (!Directory.Exists(source))
            {
                Debug.LogError("[Metaverso] El paquete WebXR no trae Hidden~.");
                return;
            }

            CopyFolder(source, Application.dataPath);
            AssetDatabase.Refresh();
        }

        static void EnableLoader()
        {
            var generalType = typeof(XRGeneralSettingsPerBuildTarget);
            var create = generalType.GetMethod("GetOrCreate", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            create?.Invoke(null, null);

            var settings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.WebGL);
            if (settings == null || settings.Manager == null)
            {
                Debug.LogWarning("[Metaverso] Abre Project Settings > XR Plug-in Management > WebGL y marca WebXR Export.");
                return;
            }

            var assigned = XRPackageMetadataStore.AssignLoader(settings.Manager, typeof(WebXRLoader).FullName, BuildTargetGroup.WebGL);
            if (!assigned)
                Debug.LogWarning("[Metaverso] No pude marcar el loader. Hazlo a mano en XR Plug-in Management > WebGL.");
        }

        static void CopyFolder(string sourceFolderName, string destFolderName)
        {
            var directory = new DirectoryInfo(sourceFolderName);
            if (!directory.Exists)
                return;
            Directory.CreateDirectory(destFolderName);
            foreach (var file in directory.GetFiles())
                file.CopyTo(Path.Combine(destFolderName, file.Name), true);

            foreach (var sub in directory.GetDirectories())
                CopyFolder(sub.FullName, Path.Combine(destFolderName, sub.Name));
        }
    }
}
