using UnityEngine;
using TERRAGRAV.Player;
using TERRAGRAV.Territory;
using TERRAGRAV.CameraSystem;

namespace TERRAGRAV.Core
{
    /// <summary>
    /// Initializer script for the Game scene: instantiates/configures local player,
    /// claims starting circular territory base in TerritoryGrid, updates PlayerStats,
    /// and links the 2.5D camera target.
    /// </summary>
    public class GameSetup : MonoBehaviour
    {
        [Header("Player Spawn Configuration")]
        [SerializeField] private PlayerController _playerPrefab;
        [SerializeField] private Vector3 _spawnPosition = new Vector3(0f, 0.5f, 0f);
        [SerializeField] private float _startingTerritoryRadius = 6.0f;
        [SerializeField] private Color _localPlayerColor = new Color(0.14f, 0.58f, 0.98f, 1f);

        [Header("System References")]
        [SerializeField] private TerritoryGrid _territoryGrid;
        [SerializeField] private GameCameraController _cameraController;

        private PlayerController _spawnedPlayer;

        public PlayerController SpawnedPlayer => _spawnedPlayer;

        private void Start()
        {
            SetupGame();
        }

        private void SetupGame()
        {
            // 1. Resolve or spawn Player
            if (_playerPrefab != null)
            {
                _spawnedPlayer = Instantiate(_playerPrefab, _spawnPosition, Quaternion.identity);
            }
            else
            {
                _spawnedPlayer = FindObjectOfType<PlayerController>();
            }

            if (_spawnedPlayer != null)
            {
                _spawnedPlayer.SetupPlayer(1, "Player 1", _localPlayerColor, true);

                if (_spawnedPlayer.Settings != null)
                {
                    _startingTerritoryRadius = _spawnedPlayer.Settings.StartingTerritoryRadius;
                    _localPlayerColor = _spawnedPlayer.Settings.CaptureColor;
                }

                // Link Camera Target
                if (_cameraController != null)
                {
                    _cameraController.SetTarget(_spawnedPlayer.transform);
                }
                else if (ServiceLocator.TryGet(out GameCameraController cam))
                {
                    cam.SetTarget(_spawnedPlayer.transform);
                }
            }
            else
            {
                Debug.LogError("[GameSetup] No PlayerController found or assigned!");
            }

            // 2. Resolve TerritoryGrid and Claim Starting Base
            if (_territoryGrid == null)
            {
                ServiceLocator.TryGet(out _territoryGrid);
            }

            if (_territoryGrid != null)
            {
                _territoryGrid.ClaimStartingTerritory(1, _spawnPosition, _startingTerritoryRadius, _localPlayerColor);

                if (_spawnedPlayer != null && _spawnedPlayer.Stats != null)
                {
                    int ownedCells = _territoryGrid.Data.GetPlayerCellCount(1);
                    float percentage = _territoryGrid.Data.GetPlayerPercentage(1);
                    _spawnedPlayer.Stats.UpdateTerritory(ownedCells, percentage);
                }
            }
            else
            {
                Debug.LogError("[GameSetup] Missing TerritoryGrid in scene!");
            }

            // 3. Ensure TerritoryCapture and TrailManager are present
            if (!ServiceLocator.TryGet(out TerritoryCapture capture))
            {
                gameObject.AddComponent<TerritoryCapture>();
            }

            if (!ServiceLocator.TryGet(out Trail.TrailManager trailManager))
            {
                gameObject.AddComponent<Trail.TrailManager>();
            }
        }
    }
}
