using UnityEngine;
using TERRAGRAV.Core;

namespace TERRAGRAV.Player
{
    /// <summary>
    /// Encapsulates player metrics and score with read-only public access.
    /// Dispatches decoupled events upon value updates.
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        [Header("Player Identity")]
        [SerializeField] private int _playerId = 1;
        [SerializeField] private string _playerName = "Player";
        [SerializeField] private bool _isLocalPlayer = true;

        [Header("Read-Only Live Metrics")]
        [SerializeField] private int _territoryCells = 0;
        [SerializeField] private float _territoryPercentage = 0f;
        [SerializeField] private int _score = 0;
        [SerializeField] private bool _isAlive = true;

        public int PlayerId => _playerId;
        public string PlayerName => _playerName;
        public bool IsLocalPlayer => _isLocalPlayer;
        public int TerritoryCells => _territoryCells;
        public float TerritoryPercentage => _territoryPercentage;
        public int Score => _score;
        public bool IsAlive => _isAlive;

        public void Initialize(int playerId, string playerName, bool isLocalPlayer)
        {
            _playerId = playerId;
            _playerName = playerName;
            _isLocalPlayer = isLocalPlayer;
            _territoryCells = 0;
            _territoryPercentage = 0f;
            _score = 0;
            _isAlive = true;
        }

        public void UpdateTerritory(int cellsOwned, float percentage)
        {
            _territoryCells = cellsOwned;
            _territoryPercentage = percentage;
            _score = Mathf.RoundToInt(_territoryPercentage * 1000f);

            if (_isLocalPlayer)
            {
                GameEvents.TriggerTerritoryChanged(_playerId, _territoryCells, _territoryPercentage);
                GameEvents.TriggerScoreChanged(_playerId, _score);
            }
        }

        public void AddScore(int points)
        {
            _score += points;
            if (_isLocalPlayer)
            {
                GameEvents.TriggerScoreChanged(_playerId, _score);
            }
        }

        public void SetAlive(bool alive)
        {
            if (_isAlive == alive) return;
            _isAlive = alive;

            if (!alive)
            {
                GameEvents.TriggerPlayerDeath(_playerId, transform.position);
            }
        }
    }
}
