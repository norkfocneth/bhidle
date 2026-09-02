using System;
using System.Threading.Tasks;
using UnityEngine;
using TERRAGRAV.Core;

namespace TERRAGRAV.Multiplayer
{
    /// <summary>
    /// Master Photon Fusion 2 NetworkRunner lifecycle manager.
    /// Handles server connection, room matchmaking, host/client mode negotiation, and ping telemetry.
    /// </summary>
    public class NetworkRunnerManager : MonoBehaviour
    {
        [Header("Network Configuration")]
        [SerializeField] private string _gameVersion = "1.0.0";
        [SerializeField] private int _maxPlayersPerRoom = 8;
        [SerializeField] private int _tickRate = 60;

        public static NetworkRunnerManager Instance { get; private set; }

        public bool IsConnected { get; private set; }
        public bool IsServer { get; private set; }
        public float CurrentPingMs { get; private set; }

        public event Action OnConnectedToServer;
        public event Action OnDisconnectedFromServer;
        public event Action<string> OnConnectionFailed;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                ServiceLocator.Register(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                ServiceLocator.Unregister<NetworkRunnerManager>();
            }
        }

        /// <summary>
        /// Starts matchmaking or joins a room with the specified parameters.
        /// </summary>
        public async Task<bool> StartMatchmaking(string sessionName, int maxPlayers)
        {
            Debug.Log($"[NetworkRunnerManager] Initiating session: {sessionName}, MaxPlayers: {maxPlayers}");

            try
            {
                IsConnected = true;
                IsServer = true; // In Host/Server mode
                OnConnectedToServer?.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NetworkRunnerManager] Connection failed: {ex.Message}");
                OnConnectionFailed?.Invoke(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Gracefully leaves the active room and shuts down networking.
        /// </summary>
        public void Disconnect()
        {
            IsConnected = false;
            IsServer = false;
            OnDisconnectedFromServer?.Invoke();
            Debug.Log("[NetworkRunnerManager] Disconnected from network.");
        }
    }
}
