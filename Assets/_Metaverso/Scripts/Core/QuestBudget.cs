namespace Metaverso
{
    public readonly struct BudgetReport
    {
        public readonly int DrawCalls;
        public readonly int Triangles;
        public readonly int MaxTextureSize;
        public readonly bool DrawCallsOk;
        public readonly bool TrianglesOk;
        public readonly bool TextureOk;

        public BudgetReport(int drawCalls, int triangles, int maxTextureSize, bool drawCallsOk, bool trianglesOk, bool textureOk)
        {
            DrawCalls = drawCalls;
            Triangles = triangles;
            MaxTextureSize = maxTextureSize;
            DrawCallsOk = drawCallsOk;
            TrianglesOk = trianglesOk;
            TextureOk = textureOk;
        }

        public bool Passes => DrawCallsOk && TrianglesOk && TextureOk;

        public string Summary =>
            $"draw calls {DrawCalls}/{QuestBudget.MaxDrawCalls} {(DrawCallsOk ? "ok" : "ALTO")}, " +
            $"triangulos {Triangles}/{QuestBudget.MaxTriangles} {(TrianglesOk ? "ok" : "ALTO")}, " +
            $"textura {MaxTextureSize}px/{QuestBudget.MaxTextureSize} {(TextureOk ? "ok" : "ALTO")}";
    }

    /// <summary>
    /// Presupuesto de un mundo para Quest Browser. El draw call es una estimacion
    /// (un renderer visible), no el frame debugger.
    /// </summary>
    public static class QuestBudget
    {
        public const int MaxDrawCalls = 150;
        public const int MaxTriangles = 500000;
        public const int MaxTextureSize = 2048;
        public const long MaxCompressedDataBytes = 50L * 1024L * 1024L;

        public static BudgetReport Evaluate(int drawCalls, int triangles, int maxTextureSize)
        {
            return new BudgetReport(
                drawCalls,
                triangles,
                maxTextureSize,
                drawCalls <= MaxDrawCalls,
                triangles <= MaxTriangles,
                maxTextureSize <= MaxTextureSize);
        }
    }
}
