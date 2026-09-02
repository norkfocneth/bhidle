using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TERRAGRAV.Core;

namespace TERRAGRAV.Multiplayer
{
    /// <summary>
    /// Master server-authoritative multiplayer match coordinator.
    /// Controls network game state, synchronized match timers, elimination broadcasts, and results.
    /// </summary>
    public class NetworkGameManager : MonoBehaviour
    {
        [Header("Match Timing Configuration")]
        [SerializeField] private float _matchDurationSeconds = 480f; // 8 minutes
        [SerializeField] private int _countdownSeconds = 3;

        private NetworkMatchState _currentMatchState = NetworkMatchState.WaitingForPlayers;
        private float _remainingMatchTime;
        private readonly List<NetworkPlayer> _connectedPlayers = new List<NetworkPlayer>();

        public NetworkMatchState CurrentMatchState => _currentMatchState;
        public float RemainingMatchTime => _remainingMatchTime;
        public IReadOnlyList<NetworkPlayer> ConnectedPlayers => _connectedPlayers;

        private void Start()
        {
            _remainingMatchTime = _matchDurationSeconds;
        }

        /// <summary>
        /// Registers a newly connected player into the active session.
        /// </summary>
        public void RegisterPlayer(NetworkPlayer player)
        {
            if (!_connectedPlayers.Contains(player))
            {
                _connectedPlayers.Add(player);
                Debug.Log($"[NetworkGameManager] Registered player: {player.PlayerName} (ID: {player.PlayerId})");
            }
        }

        /// <summary>
        /// Unregisters a player on disconnect.
        /// </summary>
        public void UnregisterPlayer(NetworkPlayer player)
        {
            _connectedPlayers.Remove(player);
            Debug.Log($"[NetworkGameManager] Unregistered player: {player.PlayerName}");
        }

        /// <summary>
        /// Initiates the synchronized 3-2-1 match countdown sequence.
        /// </summary>
        public void StartMatchCountdown()
        {
            StartCoroutine(MatchCountdownRoutine());
        }

        private IEnumerator MatchCountdownRoutine()
        {
            _currentMatchState = NetworkMatchState.Countdown;

            for (int i = _countdownSeconds; i > 0; i--)
            {
                GameEvents.TriggerCountdownTick(i);
                yield return new WaitForSeconds(1.0f);
            }

            _currentMatchState = NetworkMatchState.Playing;
            GameEvents.TriggerGameStateChanged(GameState.Playing);
            Debug.Log("[NetworkGameManager] Match started!");
        }

        private void Update()
        {
            if (_currentMatchState != NetworkMatchState.Playing) return;

            _remainingMatchTime -= Time.deltaTime;
            GameEvents.TriggerMatchTimerUpdated(_remainingMatchTime);

            if (_remainingMatchTime <= 0f)
            {
                _remainingMatchTime = 0f;
                EndMatch();
            }
        }

        private void EndMatch()
        {
            _currentMatchState = NetworkMatchState.MatchEnded;
            GameEvents.TriggerGameStateChanged(GameState.GameOver);
            Debug.Log("[NetworkGameManager] Match time expired! Match concluded.");
        }
    }
}
