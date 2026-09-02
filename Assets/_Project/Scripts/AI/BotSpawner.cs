using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TERRAGRAV.Core;
using TERRAGRAV.Player;
using TERRAGRAV.Territory;

namespace TERRAGRAV.AI
{
    /// <summary>
    /// Spawns and manages autonomous bot opponents in offline/practice matches.
    /// Distributes starting positions radially and handles automatic respawning.
    /// </summary>
    public class BotSpawner : MonoBehaviour
    {
        [Header("Bot Configuration")]
        [SerializeField] private PlayerController _botPrefab;
        [SerializeField] private int _botCount = 7;
        [SerializeField] private float _spawnRadiusFromCenter = 65f;
        [SerializeField] private float _startingTerritoryRadius = 15.0f;
        [SerializeField] private float _botRespawnDelay = 4.0f;

        private readonly List<PlayerController> _spawnedBots = new List<PlayerController>();
        private TerritoryGrid _grid;

        private static readonly (string name, Color color, Vector2 basePos)[] _botProfiles = new (string, Color, Vector2)[]
        {
            ("Rohan",   new Color(0.30f, 0.69f, 0.31f), new Vector2(   0f, -150f)), // P2
            ("Vihaan",  new Color(1.00f, 0.70f, 0.00f), new Vector2( 150f, -150f)), // P3
            ("Kabir",   new Color(0.90f, 0.22f, 0.21f), new Vector2(-150f,    0f)), // P4
            ("Yash",    new Color(0.56f, 0.14f, 0.67f), new Vector2( 150f,    0f)), // P5
            ("Dev",     new Color(0.00f, 0.67f, 0.76f), new Vector2(-150f,  150f)), // P6
            ("Reyansh", new Color(0.91f, 0.12f, 0.39f), new Vector2(   0f,  150f)), // P7
            ("Shiva",   new Color(0.98f, 0.55f, 0.00f), new Vector2( 150f,  150f))  // P8
        };

        private void Start()
        {
            if (ServiceLocator.TryGet(out TerritoryGrid grid))
            {
                _grid = grid;
            }

            SpawnAllBots();
        }

        private void OnEnable()
        {
            GameEvents.OnPlayerEliminated += HandlePlayerEliminated;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerEliminated -= HandlePlayerEliminated;
        }

        public void SpawnAllBots()
        {
            if (_grid == null)
            {
                ServiceLocator.TryGet(out _grid);
            }

            int count = Mathf.Min(_botCount, _botProfiles.Length);

            for (int i = 0; i < count; i++)
            {
                int botId = i + 2; // Player 1 (Arnav) is local player at (-150, -150)
                var profile = _botProfiles[i];

                float offsetX = Random.Range(-10f, 10f);
                float offsetZ = Random.Range(-10f, 10f);
                Vector3 spawnPos = new Vector3(profile.basePos.x + offsetX, 0.5f, profile.basePos.y + offsetZ);

                SpawnSingleBot(botId, profile.name, profile.color, spawnPos);
            }
        }

        private void SpawnSingleBot(int botId, string botName, Color botColor, Vector3 spawnPos)
        {
            PlayerController botInstance;
            if (_botPrefab != null)
            {
                botInstance = Instantiate(_botPrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                GameObject botObj = new GameObject($"Bot_{botName}");
                botObj.transform.position = spawnPos;
                botInstance = botObj.AddComponent<PlayerController>();
            }

            // Ensure BotAI is attached
            BotAI ai = botInstance.GetComponent<BotAI>();
            if (ai == null)
            {
                ai = botInstance.gameObject.AddComponent<BotAI>();
            }

            botInstance.SetupPlayer(botId, botName, botColor, false);

            if (_grid != null)
            {
                _grid.ClaimStartingTerritory(botId, spawnPos, _startingTerritoryRadius, botColor);
            }

            _spawnedBots.Add(botInstance);
        }

        private void HandlePlayerEliminated(int killerId, int victimId, Vector3 deathPos)
        {
            // If an eliminated bot died, schedule respawn
            for (int i = 0; i < _spawnedBots.Count; i++)
            {
                PlayerController bot = _spawnedBots[i];
                if (bot != null && bot.Stats.PlayerId == victimId)
                {
                    StartCoroutine(RespawnBotRoutine(bot));
                    break;
                }
            }
        }

        private IEnumerator RespawnBotRoutine(PlayerController bot)
        {
            yield return new WaitForSeconds(_botRespawnDelay);

            if (bot != null && _grid != null)
            {
                float angle = Random.Range(0f, Mathf.PI * 2f);
                float dist = Random.Range(30f, _spawnRadiusFromCenter);
                Vector3 newSpawn = new Vector3(Mathf.Cos(angle) * dist, 0.5f, Mathf.Sin(angle) * dist);

                bot.Respawn(newSpawn);
                Color botColor = bot.Settings != null ? bot.Settings.CaptureColor : Color.red;
                _grid.ClaimStartingTerritory(bot.Stats.PlayerId, newSpawn, _startingTerritoryRadius, botColor);
            }
        }
    }
}
