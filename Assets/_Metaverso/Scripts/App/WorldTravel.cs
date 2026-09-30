using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Metaverso
{
    /// <summary>
    /// Viaje entre mundos: fundido, espera en la losa de Boot, descarga del mundo anterior,
    /// carga Addressable aditiva del nuevo, spawn y ?world= en la barra de direcciones.
    /// Se descarga antes de cargar para no tener dos mundos en la memoria del Quest.
    /// </summary>
    public class WorldTravel : MonoBehaviour
    {
        public PersistentRig Rig;
        public TravelFader Fader;
        public float FadeSeconds = 0.35f;

        public static WorldTravel Instance { get; private set; }

        /// <summary>Antes de soltar el mundo actual (la red sale de la sala aca).</summary>
        public static event Action<WorldEntry> Departing;

        /// <summary>El jugador ya esta parado en el mundo nuevo.</summary>
        public static event Action<WorldEntry> Arrived;

        public WorldCatalog Catalog { get; private set; }
        public WorldEntry Current { get; private set; }
        public bool IsBusy { get; private set; }

        AsyncOperationHandle<SceneInstance> _scene;
        readonly HashSet<string> _preloaded = new HashSet<string>();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void SetCatalog(WorldCatalog catalog)
        {
            Catalog = catalog;
        }

        public static string PrivateMessage(WorldEntry entry)
        {
            return $"{entry.DisplayName} es un mundo privado. El acceso con password llega con el backend del evento.";
        }

        /// <summary>False si ya hay un viaje, si el mundo no existe o si es privado.</summary>
        public bool Go(string worldId, string spawnId = null)
        {
            if (IsBusy || Catalog == null)
                return false;

            var entry = Catalog.Find(worldId);
            if (entry == null)
            {
                MetaversoHud.Notify($"El mundo '{worldId}' no esta en el catalogo.");
                return false;
            }

            if (entry.IsPrivate)
            {
                MetaversoHud.Notify(PrivateMessage(entry));
                return false;
            }

            if (entry == Current && string.IsNullOrEmpty(spawnId))
                return false;

            IsBusy = true;
            StartCoroutine(Travel(entry, spawnId));
            return true;
        }

        /// <summary>Baja los bundles del mundo a la cache sin cargarlo. Una vez por mundo.</summary>
        public void Preload(string worldId)
        {
            var entry = Catalog?.Find(worldId);
            if (entry == null || entry.IsPrivate || entry == Current || !_preloaded.Add(entry.id))
                return;
            StartCoroutine(PreloadRoutine(entry));
        }

        IEnumerator PreloadRoutine(WorldEntry entry)
        {
            var exists = false;
            yield return HasLocation(entry.address, found => exists = found);
            if (!exists)
                yield break;

            var size = Addressables.GetDownloadSizeAsync(entry.address);
            while (!size.IsDone)
                yield return null;
            var bytes = size.Status == AsyncOperationStatus.Succeeded ? size.Result : 0L;
            size.Release();
            if (bytes <= 0)
                yield break;

            var download = Addressables.DownloadDependenciesAsync(entry.address);
            while (!download.IsDone)
                yield return null;
            if (download.Status == AsyncOperationStatus.Succeeded)
                Debug.Log($"[Metaverso] Precargado {entry.id} ({bytes / 1024} KB).");
            else
                _preloaded.Remove(entry.id);
            download.Release();
        }

        IEnumerator Travel(WorldEntry target, string spawnId)
        {
            var from = Current;
            var label = "Cargando " + target.DisplayName;
            ShowProgress(label);
            if (Fader != null)
                yield return Fader.FadeTo(1f, from == null ? 0f : FadeSeconds);

            if (from != null)
                Departing?.Invoke(from);
            if (Rig != null)
                Rig.Hold();

            if (_scene.IsValid())
            {
                var unload = Addressables.UnloadSceneAsync(_scene);
                while (!unload.IsDone)
                    yield return null;
                _scene = default;
                Current = null;
                yield return Resources.UnloadUnusedAssets();
            }

            string error = null;
            var exists = false;
            yield return HasLocation(target.address, found => exists = found);
            if (!exists)
            {
                error = "todavia no esta publicado en este build";
            }
            else
            {
                var load = Addressables.LoadSceneAsync(target.address, LoadSceneMode.Additive);
                while (!load.IsDone)
                {
                    ShowProgress($"{label} {Percent(load)}%");
                    yield return null;
                }

                if (load.Status == AsyncOperationStatus.Succeeded)
                {
                    _scene = load;
                }
                else
                {
                    error = load.OperationException != null ? load.OperationException.Message : "fallo la carga";
                    load.Release();
                }
            }

            if (error != null)
            {
                Debug.LogError($"[Metaverso] No se pudo abrir '{target.id}' ({target.address}): {error}");
                IsBusy = false;
                var back = from ?? Catalog.Default;
                if (back != null && back != target)
                {
                    MetaversoHud.Notify($"No se pudo abrir {target.DisplayName}. Vuelves a {back.DisplayName}.");
                    IsBusy = true;
                    StartCoroutine(Travel(back, null));
                }
                else
                {
                    ShowProgress($"No se pudo abrir {target.DisplayName}. Recarga la pagina.");
                }

                yield break;
            }

            var scene = _scene.Result.Scene;
            SceneManager.SetActiveScene(scene);
            var spawn = SpawnPoint.Choose(scene, spawnId, from?.id);
            if (spawn == null)
                Debug.LogWarning($"[Metaverso] {target.id} no tiene SpawnPoint; el jugador aparece en el origen.");
            if (Rig != null)
            {
                if (spawn != null)
                    Rig.PlaceAt(spawn.transform.position, spawn.transform.rotation);
                else
                    Rig.PlaceAt(Vector3.zero, Quaternion.identity);
            }

            Current = target;
            UrlBridge.SetWorld(target.id);
            MetaversoHud.SetStatus(target.DisplayName);
            Arrived?.Invoke(target);

            yield return null;
            if (Fader != null)
                yield return Fader.FadeTo(0f, FadeSeconds);
            IsBusy = false;
        }

        void ShowProgress(string text)
        {
            if (Fader != null)
                Fader.SetText(text);
            MetaversoHud.SetStatus(text);
        }

        static int Percent(AsyncOperationHandle<SceneInstance> handle)
        {
            var download = handle.GetDownloadStatus();
            var value = download.TotalBytes > 0 ? download.Percent : handle.PercentComplete;
            return Mathf.Clamp(Mathf.RoundToInt(value * 100f), 0, 100);
        }

        static IEnumerator HasLocation(string address, Action<bool> done)
        {
            var locations = Addressables.LoadResourceLocationsAsync(address);
            while (!locations.IsDone)
                yield return null;
            var found = locations.Status == AsyncOperationStatus.Succeeded && locations.Result != null && locations.Result.Count > 0;
            locations.Release();
            done(found);
        }
    }
}
