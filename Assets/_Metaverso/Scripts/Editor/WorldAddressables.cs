using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build.Pipeline.Utilities;
using UnityEngine;

namespace Metaverso.EditorTools
{
    /// <summary>
    /// Grupo "Worlds": una escena por bundle, carga remota desde {pagina}/worlds/ y catalogo
    /// remoto con nombre fijo (catalog_metaverso) para actualizar mundos sin recompilar el player.
    /// </summary>
    public static class WorldAddressables
    {
        public const string GroupName = "Worlds";
        public const string RemoteBuildPathValue = "ServerData/[BuildTarget]";
        public const string RemoteLoadPathValue = "{" + WorldContent.RemoteBaseProperty + "}";
        public const string PlayerVersion = "metaverso";

        public static string ServerDataFolder =>
            Path.GetFullPath(Path.Combine(Application.dataPath, "..", "ServerData", "WebGL"));

        [MenuItem("Metaverso/Mundos/Configurar Addressables")]
        public static void ConfigureMenu()
        {
            var count = Configure().Count;
            Debug.Log($"[Metaverso] Addressables: {count} mundos en el grupo {GroupName}.");
        }

        /// <summary>Deja settings, perfil y grupo listos. Devuelve las claves publicadas.</summary>
        public static HashSet<string> Configure()
        {
            var settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            var profiles = settings.profileSettings;
            var profileId = settings.activeProfileId;
            profiles.SetValue(profileId, AddressableAssetSettings.kRemoteBuildPath, RemoteBuildPathValue);
            profiles.SetValue(profileId, AddressableAssetSettings.kRemoteLoadPath, RemoteLoadPathValue);

            settings.BuildRemoteCatalog = true;
            settings.RemoteCatalogBuildPath.SetVariableByName(settings, AddressableAssetSettings.kRemoteBuildPath);
            settings.RemoteCatalogLoadPath.SetVariableByName(settings, AddressableAssetSettings.kRemoteLoadPath);
            settings.OverridePlayerVersion = PlayerVersion;
            settings.BuildAddressablesWithPlayerBuild = AddressableAssetSettings.PlayerBuildOption.DoNotBuildWithPlayer;

            var group = settings.FindGroup(GroupName)
                ?? settings.CreateGroup(GroupName, false, false, true, null, typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
            var bundled = group.GetSchema<BundledAssetGroupSchema>() ?? group.AddSchema<BundledAssetGroupSchema>();
            bundled.BuildPath.SetVariableByName(settings, AddressableAssetSettings.kRemoteBuildPath);
            bundled.LoadPath.SetVariableByName(settings, AddressableAssetSettings.kRemoteLoadPath);
            bundled.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackSeparately;
            bundled.Compression = BundledAssetGroupSchema.BundleCompressionMode.LZ4;
            bundled.UseAssetBundleCache = true;
            bundled.IncludeInBuild = true;
            var update = group.GetSchema<ContentUpdateGroupSchema>() ?? group.AddSchema<ContentUpdateGroupSchema>();
            update.StaticContent = false;

            var addresses = new HashSet<string>();
            var keep = new HashSet<string>();
            var guids = AssetDatabase.FindAssets("t:Scene", new[] { WorldPaths.WorldsFolder });
            for (var i = 0; i < guids.Length; i++)
            {
                var id = WorldPaths.WorldIdOf(AssetDatabase.GUIDToAssetPath(guids[i]));
                if (id == null)
                    continue;
                var entry = settings.CreateOrMoveEntry(guids[i], group, false, false);
                entry.address = WorldDescriptor.AddressFor(id);
                addresses.Add(entry.address);
                keep.Add(guids[i]);
            }

            var stale = new List<AddressableAssetEntry>();
            foreach (var entry in group.entries)
            {
                if (!keep.Contains(entry.guid))
                    stale.Add(entry);
            }

            for (var i = 0; i < stale.Count; i++)
                group.RemoveAssetEntry(stale[i], false);

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true, true);
            AssetDatabase.SaveAssets();
            WarnMissingWorlds(addresses);
            return addresses;
        }

        /// <summary>
        /// Construye los bundles para el formato de textura activo (DXT o ASTC). Borra ServerData
        /// y la cache de SBP: si no, el segundo build reusa bundles con el formato del primero.
        /// </summary>
        public static bool BuildContent(out string error)
        {
            Configure();
            if (Directory.Exists(ServerDataFolder))
                Directory.Delete(ServerDataFolder, true);
            BuildCache.PurgeCache(false);
            AddressableAssetSettings.CleanPlayerContent();
            AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
            error = result != null ? result.Error : "sin resultado";
            if (!string.IsNullOrEmpty(error))
                return false;
            if (!Directory.Exists(ServerDataFolder))
            {
                error = "no se generaron bundles en " + ServerDataFolder;
                return false;
            }

            error = null;
            return true;
        }

        public static void CopyContent(string destination)
        {
            if (Directory.Exists(destination))
                Directory.Delete(destination, true);
            CopyFolder(ServerDataFolder, destination);
        }

        static void WarnMissingWorlds(HashSet<string> addresses)
        {
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(WorldPaths.CatalogAsset);
            if (asset == null)
            {
                Debug.LogWarning("[Metaverso] Falta " + WorldPaths.CatalogAsset);
                return;
            }

            if (!WorldCatalog.TryParse(asset.text, out var catalog, out var error))
            {
                Debug.LogError("[Metaverso] worlds.json: " + error);
                return;
            }

            for (var i = 0; i < catalog.Worlds.Count; i++)
            {
                var world = catalog.Worlds[i];
                if (addresses.Contains(world.address))
                    continue;
                var note = world.IsPrivate ? " (privado: se publica cuando exista la escena)" : "";
                Debug.LogWarning($"[Metaverso] worlds.json pide '{world.address}' para {world.id} y no hay escena {WorldPaths.WorldScene(world.id)}{note}.");
            }
        }

        static void CopyFolder(string source, string destination)
        {
            Directory.CreateDirectory(destination);
            foreach (var file in Directory.GetFiles(source))
                File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), true);
            foreach (var folder in Directory.GetDirectories(source))
                CopyFolder(folder, Path.Combine(destination, Path.GetFileName(folder)));
        }
    }
}
