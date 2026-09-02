using UnityEngine;
using TERRAGRAV.Core;
using TERRAGRAV.Territory;
using TERRAGRAV.Trail;

namespace TERRAGRAV.Player
{
    /// <summary>
    /// Central player coordinator.
    /// Connects PlayerMovement, PlayerStats, PlayerInput, PlayerTrail, and TerritoryGrid interactions.
    /// </summary>
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(PlayerStats))]
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(PlayerTrail))]
    [RequireComponent(typeof(PlayerCollision))]
    [RequireComponent(typeof(PlayerDeathSystem))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private PlayerSettingsSO _settings;

        [Header("Visual Mesh References")]
        [SerializeField] private MeshRenderer _characterMeshRenderer;
        [SerializeField] private Transform _visualRoot;

        private PlayerMovement _movement;
        private PlayerStats _stats;
        private PlayerInput _input;
        private PlayerTrail _trail;
        private PlayerCollision _collision;
        private PlayerDeathSystem _deathSystem;
        private TerritoryGrid _territoryGrid;

        public PlayerMovement Movement => _movement;
        public PlayerStats Stats => _stats;
        public PlayerInput Input => _input;
        public PlayerTrail Trail => _trail;
        public PlayerCollision Collision => _collision;
        public PlayerDeathSystem DeathSystem => _deathSystem;
        public PlayerSettingsSO Settings => _settings;

        private void Awake()
        {
            _movement = GetComponent<PlayerMovement>();
            _stats = GetComponent<PlayerStats>();
            _input = GetComponent<PlayerInput>();
            _trail = GetComponent<PlayerTrail>();
            _collision = GetComponent<PlayerCollision>();
            _deathSystem = GetComponent<PlayerDeathSystem>();

            if (_settings == null)
            {
                Debug.LogWarning("[PlayerController] PlayerSettingsSO not assigned! Creating default instance.");
                _settings = ScriptableObject.CreateInstance<PlayerSettingsSO>();
            }

            _movement.Initialize(_settings);
        }

        private void Start()
        {
            if (ServiceLocator.TryGet(out TerritoryGrid grid))
            {
                _territoryGrid = grid;
            }
        }

        private void OnEnable()
        {
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
        }

        /// <summary>
        /// Initializes player identity, color, trail, and local/remote status.
        /// </summary>
        public void SetupPlayer(int playerId, string playerName, Color playerColor, bool isLocalPlayer)
        {
            _stats.Initialize(playerId, playerName, isLocalPlayer);
            _input.SetIsLocalPlayer(isLocalPlayer);
            ApplyColor(playerColor);

            if (_territoryGrid == null)
            {
                ServiceLocator.TryGet(out _territoryGrid);
            }

            float trailWidth = (_settings != null) ? _settings.TrailWidth : 0.6f;
            _trail.Initialize(playerId, playerColor, trailWidth, _territoryGrid);

            GameEvents.TriggerPlayerSpawned(playerId);
        }

        public void ApplyColor(Color color)
        {
            if (_characterMeshRenderer != null)
            {
                Material mat = _characterMeshRenderer.material;
                if (mat != null)
                {
                    mat.color = color;
                }
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.Playing:
                    _movement.SetCanMove(_stats.IsAlive);
                    break;
                case GameState.Boot:
                case GameState.Lobby:
                case GameState.Countdown:
                case GameState.GameOver:
                    _movement.SetCanMove(false);
                    break;
            }
        }

        public void Eliminate()
        {
            if (!_stats.IsAlive) return;

            _stats.SetAlive(false);
            _movement.SetCanMove(false);
            _trail.ClearTrail();

            if (_visualRoot != null)
            {
                _visualRoot.gameObject.SetActive(false);
            }
        }

        public void Respawn(Vector3 spawnPosition)
        {
            transform.position = spawnPosition;
            _stats.SetAlive(true);
            _trail.ClearTrail();

            if (_visualRoot != null)
            {
                _visualRoot.gameObject.SetActive(true);
            }

            _movement.SetCanMove(true);
            _movement.ResetSpeed();
        }
    }
}
