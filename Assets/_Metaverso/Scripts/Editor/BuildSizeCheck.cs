using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Metaverso.EditorTools
{
    public class BuildSizeCheck : IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.WebGL)
                return;

            var output = report.summary.outputPath;
            if (string.IsNullOrEmpty(output) || !Directory.Exists(output))
                return;

            var files = Directory.GetFiles(output, "*.data*", SearchOption.AllDirectories);
            long largest = 0;
            string largestPath = null;
            for (var i = 0; i < files.Length; i++)
            {
                var length = new FileInfo(files[i]).Length;
                if (length <= largest)
                    continue;
                largest = length;
                largestPath = files[i];
            }

            if (largest <= QuestBudget.MaxCompressedDataBytes)
            {
                Debug.Log($"[Metaverso] Datos web {largest / (1024 * 1024)} MB, dentro del presupuesto de 50 MB.");
                return;
            }

            throw new BuildFailedException(
                $"[Metaverso] {largestPath} pesa {largest / (1024 * 1024)} MB. El presupuesto de descarga es 50 MB.");
        }
    }
}
