using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Metaverso.EditorTools
{
    public static class WebBuildPipeline
    {
        public const string ScenePath = "Assets/_Metaverso/Scenes/Circuito.unity";
        public const string BuildInfoPath = "Assets/_Metaverso/Resources/BuildInfo.asset";

        public static string DeployRoot => Path.GetFullPath(Path.Combine(Application.dataPath, "..", "metaverso-web"));

        [MenuItem("Metaverso/Build Web (desktop + Quest)")]
        public static void BuildBoth()
        {
            if (!File.Exists(ScenePath))
            {
                EditorUtility.DisplayDialog("Metaverso", "Falta la escena. Usa Metaverso > Crear mundo de prueba.", "Ok");
                return;
            }

            var info = Stamp();
            var docs = Path.Combine(DeployRoot, "docs");
            Directory.CreateDirectory(docs);

            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL))
            {
                EditorUtility.DisplayDialog(
                    "Metaverso",
                    "No se pudo activar la plataforma Web. Instala el modulo Web Build Support en Unity Hub.",
                    "Ok");
                return;
            }

            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.nameFilesAsHashes = true;
            PlayerSettings.WebGL.dataCaching = true;

            BuildVariant(docs, "desktop", "DXTC", info);
            BuildVariant(docs, "quest", "ASTC", info);
            WriteShell(docs, info);
            Debug.Log($"[Metaverso] Build {info.Label} en {docs}");
            EditorUtility.DisplayDialog("Metaverso", $"Build listo: {info.Label}\n{docs}", "Ok");
        }

        public static BuildInfo Stamp()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(BuildInfoPath) ?? "Assets/_Metaverso/Resources");
            var info = AssetDatabase.LoadAssetAtPath<BuildInfo>(BuildInfoPath);
            if (info == null)
            {
                info = ScriptableObject.CreateInstance<BuildInfo>();
                AssetDatabase.CreateAsset(info, BuildInfoPath);
            }

            info.version = BuildInfo.NextPatch(info.version);
            info.builtAt = BuildInfo.StampNow();
            EditorUtility.SetDirty(info);
            AssetDatabase.SaveAssets();
            return info;
        }

        static void BuildVariant(string docs, string folder, string textureFormat, BuildInfo info)
        {
            TrySetTextureFormat(textureFormat);
            var output = Path.Combine(docs, folder);
            if (Directory.Exists(output))
                Directory.Delete(output, true);

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = output,
                target = BuildTarget.WebGL,
                targetGroup = BuildTargetGroup.WebGL
            };
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new BuildFailedException($"Build {folder} fallo: {report.summary.result}");

            PatchIndex(Path.Combine(output, "index.html"), info);
            File.WriteAllText(Path.Combine(output, "texture-format.txt"), textureFormat + "\n");
        }

        static void TrySetTextureFormat(string formatName)
        {
            var editor = typeof(PlayerSettings).Assembly;
            var enumType = editor.GetType("UnityEditor.TextureCompressionFormat");
            var method = typeof(PlayerSettings).GetMethod(
                "SetDefaultTextureCompressionFormat",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (enumType == null || method == null || !Enum.IsDefined(enumType, formatName))
            {
                Debug.LogWarning("[Metaverso] No aplique el formato " + formatName + ". En Player Settings elige DXT para desktop y ASTC para Quest si el menu no lo cambia solo.");
                return;
            }

            var value = Enum.Parse(enumType, formatName);
            var parameters = method.GetParameters();
            if (parameters.Length == 2)
                method.Invoke(null, new[] { (object)BuildTarget.WebGL, value });
            else if (parameters.Length == 1)
                method.Invoke(null, new[] { value });
        }

        static void PatchIndex(string indexPath, BuildInfo info)
        {
            if (!File.Exists(indexPath))
                return;
            var html = File.ReadAllText(indexPath);
            const string note = "<p id=\"metaverso-load-note\" style=\"position:fixed;left:16px;bottom:12px;z-index:5;color:#fff;font:14px sans-serif;text-shadow:0 1px 2px #000;\">La primera visita descarga el mundo en el navegador. No se instala una aplicacion. Las siguientes usan la cache. VERSION</p>";
            if (!html.Contains("metaverso-load-note"))
                html = html.Replace("</body>", note.Replace("VERSION", info.Label) + "</body>");
            File.WriteAllText(indexPath, html);
        }

        public static void WriteShell(string docs, BuildInfo info)
        {
            File.WriteAllText(Path.Combine(docs, "version.json"),
                "{\"version\":\"" + info.version + "\",\"builtAt\":\"" + info.builtAt + "\",\"label\":\"" + info.Label + "\"}\n");
            File.WriteAllText(Path.Combine(docs, ".nojekyll"), "");
            File.WriteAllText(Path.Combine(docs, "index.html"), ShellHtml());
        }

        public static string ShellHtml()
        {
            return @"<!DOCTYPE html>
<html lang=""es"">
<head>
  <meta charset=""utf-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
  <title>Metaverso Sinestesia</title>
  <style>
    body { margin: 0; background: #1b1814; color: #f4efe6; font-family: Georgia, serif; }
    main { max-width: 42rem; margin: 12vh auto; padding: 1.5rem; }
  </style>
</head>
<body>
  <main>
    <h1>Metaverso Sinestesia</h1>
    <p id=""note"">La primera visita descarga el mundo en el navegador. No se instala una aplicacion. Las siguientes usan la cache.</p>
    <p id=""version""></p>
  </main>
  <script>
    fetch(""version.json"").then(function (r) { return r.json(); }).then(function (info) {
      var node = document.getElementById(""version"");
      if (node && info.label) node.textContent = info.label;
    }).catch(function () {});
    var quest = /Quest|OculusBrowser/i.test(navigator.userAgent);
    var target = (quest ? ""quest/"" : ""desktop/"") + location.search + location.hash;
    location.replace(target);
  </script>
</body>
</html>
";
        }
    }
}
