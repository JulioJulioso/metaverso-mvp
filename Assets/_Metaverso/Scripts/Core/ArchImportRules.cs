namespace Metaverso
{
    public static class ArchImportRules
    {
        public const string Folder = "Assets/_Metaverso/Models/Arch";
        public const int ArchitectureLayer = 8;

        public static bool AppliesTo(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                return false;

            var path = assetPath.Replace('\\', '/');
            return path == Folder || path.StartsWith(Folder + "/");
        }
    }
}
