using UnityEngine;
using TERRAGRAV.Core;
using TERRAGRAV.Territory;
using TERRAGRAV.Trail;

namespace TERRAGRAV.Player
{
    /// <summary>
    /// Continuous collision detector for trail cutting, self-intersection, and base defense.
    /// Operates on the player's head position in the 2.5D X-Z plane.
    /// </summary>
    public class PlayerCollision : MonoBehaviour
    {
        [Header("Collision Parameters")]
        [Tooltip("Radius around the player's head used to test for trail cutting.")]
        [SerializeField] private float _collisionRadius = 0.8f;

        [Tooltip("Enable self-trail collision (hitting own trail results in death).")]
        [SerializeField] private bool _enableSelfCollision = true;

        private PlayerController _controller;
        private TrailManager _trailManager;
        private TerritoryGrid _territoryGrid;

        private void Awake()
        {
            _controller = GetComponent<PlayerController>();
        }

        private void Start()
        {
            if (ServiceLocator.TryGet(out TrailManager tm))
            {
                _trailManager = tm;
            }

            if (ServiceLocator.TryGet(out TerritoryGrid grid))
            {
                _territoryGrid = grid;
            }
        }

        private void Update()
        {
            if (_controller == null || !_controller.Stats.IsAlive) return;

            if (_trailManager == null)
            {
                ServiceLocator.TryGet(out _trailManager);
                if (_trailManager == null) return;
            }

            Vector3 headPos = transform.position;
            int myPlayerId = _controller.Stats.PlayerId;

            // 1. Check for Trail Cutting Intersections
            if (_trailManager.CheckTrailIntersection(headPos, _collisionRadius, myPlayerId, out int victimPlayerId, out int segmentIndex))
            {
                if (victimPlayerId == myPlayerId)
                {
                    // Self-collision
                    if (_enableSelfCollision)
                    {
                        if (TryGetComponent(out PlayerDeathSystem deathSystem))
                        {
                            deathSystem.Eliminate(myPlayerId);
                        }
                    }
                }
                else
                {
                    // Cut an opponent's trail!
                    EliminateOpponent(victimPlayerId);
                }
            }
        }

        private void EliminateOpponent(int victimPlayerId)
        {
            // Find victim entity via TrailManager active trails
            if (_trailManager != null)
            {
                for (int i = 0; i < _trailManager.ActiveTrails.Count; i++)
                {
                    PlayerTrail victimTrail = _trailManager.ActiveTrails[i];
                    if (victimTrail != null && victimTrail.PlayerId == victimPlayerId)
                    {
                        if (victimTrail.TryGetComponent(out PlayerDeathSystem victimDeathSystem))
                        {
                            victimDeathSystem.Eliminate(_controller.Stats.PlayerId);

                            // Award points to this player
                            _controller.Stats.AddScore(250);
                        }
                        break;
                    }
                }
            }
        }
    }
}
