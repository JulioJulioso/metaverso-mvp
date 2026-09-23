#if PHOTON_FUSION
using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Metaverso.Network
{
    /// <summary>
    /// Photon Fusion 2 en Shared Mode. Una sala por ?room= de la URL.
    /// Requiere el define PHOTON_FUSION y la referencia Fusion.Runtime en el asmdef.
    /// </summary>
    public class FusionSession : MonoBehaviour, INetworkRunnerCallbacks
    {
        public NetworkObject AvatarPrefab;

        NetworkRunner _runner;

        async void Start()
        {
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;
            var sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();
            var room = RoomQuery.FromUrl(Application.absoluteURL);
            var result = await _runner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.Shared,
                SessionName = room,
                Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
                SceneManager = sceneManager
            });

            if (!result.Ok)
            {
                Debug.LogError($"[Metaverso] Fusion no entro a '{room}': {result.ShutdownReason}");
                return;
            }

            NetworkServices.Session = new FusionSessionAdapter(room);
            Debug.Log($"[Metaverso] Conectado a la sala '{room}'.");
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (player != runner.LocalPlayer || AvatarPrefab == null)
                return;
            var pose = PoseSource.Active();
            var position = pose != null ? pose.transform.position : Vector3.zero;
            var rotation = pose != null ? pose.transform.rotation : Quaternion.identity;
            runner.Spawn(AvatarPrefab, position, rotation, player);
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    }

    sealed class FusionSessionAdapter : INetworkSession
    {
        public FusionSessionAdapter(string roomName) { RoomName = roomName; }
        public string RoomName { get; }
        public bool IsOnline => true;
    }
}
#endif
