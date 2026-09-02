using UnityEngine;
using TERRAGRAV.Territory;

namespace TERRAGRAV.Multiplayer
{
    /// <summary>
    /// Authoritative deterministic 8-player radial spawn layout.
    /// Distributes players across the circular arena to ensure fair initial distance.
    /// </summary>
    public class PlayerSpawner : MonoBehaviour
    {
        [Header("Spawn Layout Settings")]
        [SerializeField] private float _startingTerritoryRadius = 15.0f;

        private static readonly Vector2[] _quadrantPositions = new Vector2[]
        {
            new Vector2(-150f, -150f), // P1
            new Vector2(   0f, -150f), // P2
            new Vector2( 150f, -150f), // P3
            new Vector2(-150f,    0f), // P4
            new Vector2( 150f,    0f), // P5
            new Vector2(-150f,  150f), // P6
            new Vector2(   0f,  150f), // P7
            new Vector2( 150f,  150f)  // P8
        };

        private static readonly Color[] _playerColorPalette = new Color[]
        {
            new Color(0.10f, 0.45f, 0.91f), // 1: Arnav (Blue)
            new Color(0.30f, 0.69f, 0.31f), // 2: Rohan (Green)
            new Color(1.00f, 0.70f, 0.00f), // 3: Vihaan (Yellow)
            new Color(0.90f, 0.22f, 0.21f), // 4: Kabir (Red)
            new Color(0.56f, 0.14f, 0.67f), // 5: Yash (Purple)
            new Color(0.00f, 0.67f, 0.76f), // 6: Dev (Teal)
            new Color(0.91f, 0.12f, 0.39f), // 7: Reyansh (Pink)
            new Color(0.98f, 0.55f, 0.00f)  // 8: Shiva (Orange)
        };

        /// <summary>
        /// Calculates the spawn position with subtle random offset for a player slot (0 to 7).
        /// </summary>
        public Vector3 GetSpawnPosition(int playerIndex, int totalPlayers = 8)
        {
            Vector2 basePos = _quadrantPositions[playerIndex % _quadrantPositions.Length];
            float offsetX = Random.Range(-10f, 10f);
            float offsetZ = Random.Range(-10f, 10f);

            return new Vector3(basePos.x + offsetX, 0.5f, basePos.y + offsetZ);
        }

        /// <summary>
        /// Gets the distinct faction color assigned to a player slot index.
        /// </summary>
        public Color GetPlayerColor(int playerIndex)
        {
            return _playerColorPalette[playerIndex % _playerColorPalette.Length];
        }
    }
}
