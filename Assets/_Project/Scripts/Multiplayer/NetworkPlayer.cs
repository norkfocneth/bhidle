using UnityEngine;
using TERRAGRAV.Core;
using TERRAGRAV.Player;

namespace TERRAGRAV.Multiplayer
{
    /// <summary>
    /// Network representation of a connected human or bot player.
    /// Synchronizes identity, statistics, active state, and score across all connected clients.
    /// </summary>
    public class NetworkPlayer : MonoBehaviour
    {
        [Header("Synchronized Player State")]
        public int PlayerId { get; private set; }
        public string PlayerName { get; private set; }
        public Color PlayerColor { get; private set; }
        public bool IsLocalPlayer { get; private set; }
        public bool IsAlive { get; private set; } = true;
        public int Kills { get; private set; }
        public int Score { get; private set; }
        public float TerritoryPercentage { get; private set; }

        private PlayerController _localController;

        public void Initialize(int id, string name, Color color, bool isLocal)
        {
            PlayerId = id;
            PlayerName = name;
            PlayerColor = color;
            IsLocalPlayer = isLocal;
            IsAlive = true;

            _localController = GetComponent<PlayerController>();
            if (_localController != null)
            {
                _localController.SetupPlayer(id, name, color, isLocal);
            }
        }

        public void UpdateStats(int kills, int score, float territoryPct)
        {
            Kills = kills;
            Score = score;
            TerritoryPercentage = territoryPct;
        }

        public void SetAlive(bool alive)
        {
            IsAlive = alive;
        }
    }
}
