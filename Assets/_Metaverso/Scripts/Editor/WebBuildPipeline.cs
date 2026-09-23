using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Metaverso.EditorTools
{
    public static class WebBuildPipeline
    {
        public const string ScenePath = "Assets/_Metaverso/Scenes/Circuito.unity";
        public const string BuildInfoPath = "Assets/_Metaverso/Resources/BuildInfo.asset";

        const string StepKey = "Metaverso.WebBuild.Step";
        const string DocsKey = "Metaverso.WebBuild.Docs";

        enum BuildStep
        {
            None = 0,
            PrepareDesktop = 1,
            BuildDesktop = 2,
            BuildQuest = 3
        }

        public static string DeployRoot => Path.GetFullPath(Path.Combine(Application.dataPath, "..", "metaverso-web"));

        [InitializeOnLoadMethod]
        static void ResumeAfterDomainReload()
        {
            if (SessionState.GetInt(StepKey, (int)BuildStep.None) == (int)BuildStep.None)
                return;
            EditorApplication.delayCall += ContinueWhenReady;
        }

        [MenuItem("Metaverso/Build Web (desktop + Quest)")]
        public static void BuildBoth()
        {
            if (!File.Exists(ScenePath))
            {
                EditorUtility.DisplayDialog("Metaverso", "Falta la escena. Usa Metaverso > Crear mundo de prueba.", "Ok");
                return;
            }

            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Metaverso", "Sal de Play antes de compilar.", "Ok");
                return;
            }

            if (BuildPipeline.isBuildingPlayer)
            {
                EditorUtility.DisplayDialog("Metaverso", "Ya hay un build en curso.", "Ok");
                return;
            }

            if (SessionState.GetInt(StepKey, (int)BuildStep.None) != (int)BuildStep.None)
            {
                var restart = EditorUtility.DisplayDialog(
                    "Metaverso",
                    "Hay un build en curso, esperando la recarga del editor. Cancelarlo y empezar de nuevo?",
                    "Empezar de nuevo",
                    "Esperar");
                if (!restart)
                    return;
                ClearPending();
            }

            Stamp();
            var docs = Path.Combine(DeployRoot, "docs");
            Directory.CreateDirectory(docs);
            SessionState.SetString(DocsKey, docs);
            SessionState.SetInt(StepKey, (int)BuildStep.PrepareDesktop);
            RunStep();
        }

        static void ContinueWhenReady()
        {
            if (SessionState.GetInt(StepKey, (int)BuildStep.None) == (int)BuildStep.None)
                return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || BuildPipeline.isBuildingPlayer)
            {
                EditorApplication.delayCall += ContinueWhenReady;
                return;
            }

            RunStep();
        }

        static void RunStep()
        {
            var step = (BuildStep)SessionState.GetInt(StepKey, (int)BuildStep.None);
            var docs = SessionState.GetString(DocsKey, "");
            var info = AssetDatabase.LoadAssetAtPath<BuildInfo>(BuildInfoPath);
            if (step != BuildStep.None && (info == null || string.IsNullOrEmpty(docs)))
            {
                Fail("Falta BuildInfo o la carpeta de salida. Lanza el build de nuevo.");
                return;
            }

            switch (step)
            {
                case BuildStep.PrepareDesktop:
                    ApplyWebSettings();
                    if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL
                        && !EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL))
                    {
                        Fail("No se pudo activar la plataforma Web. Instala el modulo Web Build Support en Unity Hub.");
                        return;
                    }

                    ApplyFormat("DXTC", "desktop", BuildStep.BuildDesktop);
                    return;
                case BuildStep.BuildDesktop:
                    if (!BuildVariant(docs, "desktop", "DXTC", info))
                        return;
                    ApplyFormat("ASTC", "quest", BuildStep.BuildQuest);
                    return;
                case BuildStep.BuildQuest:
                    if (!BuildVariant(docs, "quest", "ASTC", info))
                        return;
                    WriteShell(docs, info);
                    ClearPending();
                    Debug.Log($"[Metaverso] Build {info.Label} en {docs}");
                    EditorUtility.DisplayDialog("Metaverso", $"Build listo: {info.Label}\n{docs}", "Ok");
                    return;
            }
        }

        static void ApplyWebSettings()
        {
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.nameFilesAsHashes = true;
            PlayerSettings.WebGL.dataCaching = true;
            AssetDatabase.SaveAssets();
        }

        // Cambiar DXTC/ASTC reimporta texturas y deja un domain reload pendiente.
        // BuildPlayer en ese mismo llamado falla con "A domain reload is pending".
        static void ApplyFormat(string format, string folder, BuildStep buildStep)
        {
            if (TrySetTextureFormat(format))
            {
                SessionState.SetInt(StepKey, (int)buildStep);
                AssetDatabase.SaveAssets();
                Debug.Log($"[Metaverso] Formato {format} aplicado. El editor recarga y luego compila {folder}.");
                EditorUtility.RequestScriptReload();
                return;
            }

            SessionState.SetInt(StepKey, (int)buildStep);
            RunStep();
        }

        static void Fail(string message)
        {
            ClearPending();
            Debug.LogError("[Metaverso] " + message);
            EditorUtility.DisplayDialog("Metaverso", message, "Ok");
        }

        static void ClearPending()
        {
            SessionState.EraseInt(StepKey);
            SessionState.EraseString(DocsKey);
            EditorUtility.ClearProgressBar();
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

        static bool BuildVariant(string docs, string folder, string textureFormat, BuildInfo info)
        {
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
            if (report.summary.result != BuildResult.Succeeded)
            {
                var detail = report.SummarizeErrors();
                if (string.IsNullOrWhiteSpace(detail))
                    detail = report.summary.result.ToString();
                if (report.summary.result == BuildResult.Unknown)
                    detail += " Unity cancelo el player porque habia una recarga del editor pendiente.";
                Fail($"Build {folder} fallo: {detail}");
                return false;
            }

            PatchIndex(Path.Combine(output, "index.html"), info);
            File.WriteAllText(Path.Combine(output, "texture-format.txt"), textureFormat + "\n");
            return true;
        }

        static bool TrySetTextureFormat(string formatName)
        {
            var editor = typeof(PlayerSettings).Assembly;
            var enumType = editor.GetType("UnityEditor.TextureCompressionFormat");
            var method = typeof(PlayerSettings).GetMethod(
                "SetDefaultTextureCompressionFormat",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (enumType == null || method == null || !Enum.IsDefined(enumType, formatName))
            {
                Debug.LogWarning("[Metaverso] No aplique el formato " + formatName + ". En Player Settings elige DXT para desktop y ASTC para Quest si el menu no lo cambia solo.");
                return false;
            }

            var value = Enum.Parse(enumType, formatName);
            if (CurrentFormatMatches(value))
                return false;

            var parameters = method.GetParameters();
            if (parameters.Length == 2)
                method.Invoke(null, new[] { (object)BuildTarget.WebGL, value });
            else if (parameters.Length == 1)
                method.Invoke(null, new[] { value });
            else
            {
                Debug.LogWarning("[Metaverso] No aplique el formato " + formatName + ". En Player Settings elige DXT para desktop y ASTC para Quest si el menu no lo cambia solo.");
                return false;
            }

            return true;
        }

        static bool CurrentFormatMatches(object desired)
        {
            var methods = typeof(PlayerSettings).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            for (var i = 0; i < methods.Length; i++)
            {
                var method = methods[i];
                if (method.Name != "GetDefaultTextureCompressionFormat")
                    continue;

                var parameters = method.GetParameters();
                object[] args = null;
                if (parameters.Length == 1)
                {
                    var argument = ArgumentFor(parameters[0]);
                    if (argument == null)
                        continue;
                    args = new[] { argument };
                }
                else if (parameters.Length != 0)
                {
                    continue;
                }

                object current;
                try
                {
                    current = method.Invoke(null, args);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning("[Metaverso] No pude leer el formato de textura actual: " + exception.Message);
                    continue;
                }

                if (FormatEquals(current, desired))
                    return true;
            }

            return false;
        }

        static bool FormatEquals(object current, object desired)
        {
            if (current == null || desired == null)
                return false;
            if (current is Array array)
                return array.Length == 1 && desired.Equals(array.GetValue(0));
            if (current is string)
                return current.Equals(desired);
            if (current is IEnumerable enumerable)
            {
                object only = null;
                var count = 0;
                foreach (var item in enumerable)
                {
                    only = item;
                    count++;
                    if (count > 1)
                        return false;
                }

                return count == 1 && desired.Equals(only);
            }

            return current.Equals(desired);
        }

        static object ArgumentFor(ParameterInfo parameter)
        {
            var type = parameter.ParameterType;
            if (type == typeof(BuildTarget))
                return BuildTarget.WebGL;
            if (type == typeof(BuildTargetGroup))
                return BuildTargetGroup.WebGL;

            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public;
            var property = type.GetProperty("WebGL", flags);
            if (property != null)
                return property.GetValue(null);
            var field = type.GetField("WebGL", flags);
            if (field != null)
                return field.GetValue(null);
            return null;
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
