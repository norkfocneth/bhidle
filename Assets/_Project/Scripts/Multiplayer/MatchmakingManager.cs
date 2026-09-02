using System;
using UnityEngine;

namespace TERRAGRAV.Multiplayer
{
    /// <summary>
    /// Handles Quick Match and ranked queue matchmaking.
    /// Supports dynamic 2, 4, 6, and 8-player room topologies.
    /// </summary>
    public class MatchmakingManager : MonoBehaviour
    {
        public enum MatchSize
        {
            Duel2P = 2,
            Quad4P = 4,
            Hexa6P = 6,
            Full8P = 8
        }

        [Header("Matchmaking Config")]
        [SerializeField] private MatchSize _selectedMatchSize = MatchSize.Full8P;
        [SerializeField] private float _matchmakingTimeoutSeconds = 15f;

        public event Action OnMatchmakingStarted;
        public event Action<string> OnMatchFound;
        public event Action OnMatchmakingCancelled;

        public MatchSize SelectedSize => _selectedMatchSize;

        public void SetMatchSize(MatchSize size)
        {
            _selectedMatchSize = size;
        }

        public void StartQuickMatch()
        {
            string sessionName = $"TERRAGRAV_{(int)_selectedMatchSize}P_{Guid.NewGuid().ToString().Substring(0, 6)}";
            Debug.Log($"[MatchmakingManager] Searching for match with {_selectedMatchSize} players...");
            OnMatchmakingStarted?.Invoke();

            if (NetworkRunnerManager.Instance != null)
            {
                _ = NetworkRunnerManager.Instance.StartMatchmaking(sessionName, (int)_selectedMatchSize);
                OnMatchFound?.Invoke(sessionName);
            }
        }

        public void CancelMatchmaking()
        {
            Debug.Log("[MatchmakingManager] Matchmaking cancelled by user.");
            OnMatchmakingCancelled?.Invoke();
        }
    }
}
