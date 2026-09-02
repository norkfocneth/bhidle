using UnityEngine;
using TERRAGRAV.Core;
using TERRAGRAV.Player;
using TERRAGRAV.Territory;
using TERRAGRAV.Trail;

namespace TERRAGRAV.AI
{
    /// <summary>
    /// Autonomous decision-making state machine for AI bot opponents.
    /// Handles strategic land expansion, trail hunting, safe retreat, and arena boundary avoidance.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class BotAI : MonoBehaviour
    {
        [Header("AI Tuning")]
        [SerializeField] private BotAIState _currentState = BotAIState.Expand;
        [SerializeField] private float _visionRadius = 18f;
        [SerializeField] private float _maxTrailLengthBeforeRetreat = 24f;
        [SerializeField] private float _boundarySafetyMargin = 12f;

        private PlayerController _controller;
        private TrailManager _trailManager;
        private TerritoryGrid _territoryGrid;
        private BotInputProvider _inputProvider;

        private Vector3 _homeBaseCenter;
        private float _stateTimer = 0f;
        private float _loopDuration = 2.5f;
        private float _loopCurveDirection = 1f;
        private float _currentHeadingAngle = 0f;

        public BotAIState CurrentState => _currentState;

        private void Awake()
        {
            _controller = GetComponent<PlayerController>();
            _inputProvider = new BotInputProvider();
        }

        private void Start()
        {
            if (ServiceLocator.TryGet(out TrailManager tm)) _trailManager = tm;
            if (ServiceLocator.TryGet(out TerritoryGrid grid)) _territoryGrid = grid;

            _homeBaseCenter = transform.position;
            _currentHeadingAngle = Random.Range(0f, Mathf.PI * 2f);
            _loopCurveDirection = Random.value > 0.5f ? 1f : -1f;
            _loopDuration = Random.Range(2.0f, 4.0f);
        }

        public void Initialize(BotInputProvider inputProvider, Vector3 spawnPosition)
        {
            _inputProvider = inputProvider;
            _homeBaseCenter = spawnPosition;
        }

        private void Update()
        {
            if (_controller == null || !_controller.Stats.IsAlive || _inputProvider == null) return;

            _stateTimer += Time.deltaTime;
            Vector3 myPos = transform.position;
            int myPlayerId = _controller.Stats.PlayerId;

            // Resolve references if needed
            if (_trailManager == null) ServiceLocator.TryGet(out _trailManager);
            if (_territoryGrid == null) ServiceLocator.TryGet(out _territoryGrid);

            Vector2 desiredDirection = Vector2.up;

            // 1. Check for nearby exposed enemy trails to hunt (Aggressive Hunter State)
            if (TryFindClosestEnemyTrail(myPos, myPlayerId, out Vector3 targetTrailPos))
            {
                _currentState = BotAIState.Hunt;
                Vector3 toTarget = (targetTrailPos - myPos).normalized;
                desiredDirection = new Vector2(toTarget.x, toTarget.z);
            }
            else
            {
                // 2. State Machine: Expand vs Retreat
                bool isTrailLong = _controller.Trail != null && _controller.Trail.PointCount > _maxTrailLengthBeforeRetreat;
                if (isTrailLong || _stateTimer > _loopDuration * 1.6f)
                {
                    _currentState = BotAIState.Retreat;
                }

                if (_currentState == BotAIState.Retreat)
                {
                    // Steer back towards home base
                    Vector3 toHome = (_homeBaseCenter - myPos).normalized;
                    desiredDirection = new Vector2(toHome.x, toHome.z);

                    // Check if safely returned home
                    if (_controller.Trail != null && !_controller.Trail.IsOutside)
                    {
                        _currentState = BotAIState.Expand;
                        _stateTimer = 0f;
                        _loopCurveDirection = Random.value > 0.5f ? 1f : -1f;
                        _loopDuration = Random.Range(2.0f, 4.0f);
                        _homeBaseCenter = myPos;
                    }
                }
                else
                {
                    // Expand state: curve outwards in an arc
                    float curveSpeed = (Mathf.PI / _loopDuration) * _loopCurveDirection;
                    _currentHeadingAngle += curveSpeed * Time.deltaTime;
                    desiredDirection = new Vector2(Mathf.Cos(_currentHeadingAngle), Mathf.Sin(_currentHeadingAngle));
                }
            }

            // 3. Boundary Avoidance
            float boundaryLimit = _controller.Settings != null ? _controller.Settings.BoundaryLimit : 98f;
            float margin = boundaryLimit - _boundarySafetyMargin;

            if (myPos.x < -margin) desiredDirection.x = Mathf.Abs(desiredDirection.x) + 0.8f;
            if (myPos.x > margin) desiredDirection.x = -Mathf.Abs(desiredDirection.x) - 0.8f;
            if (myPos.z < -margin) desiredDirection.y = Mathf.Abs(desiredDirection.y) + 0.8f;
            if (myPos.z > margin) desiredDirection.y = -Mathf.Abs(desiredDirection.y) - 0.8f;

            _inputProvider.SetDirection(desiredDirection);

            // Pass direction to movement
            if (_controller.Movement != null)
            {
                _controller.Movement.SetInputDirection(desiredDirection);
            }
        }

        private bool TryFindClosestEnemyTrail(Vector3 myPos, int myPlayerId, out Vector3 targetPos)
        {
            targetPos = Vector3.zero;
            if (_trailManager == null) return false;

            float visionSqr = _visionRadius * _visionRadius;
            float closestDistSqr = visionSqr;
            bool found = false;

            for (int t = 0; t < _trailManager.ActiveTrails.Count; t++)
            {
                PlayerTrail otherTrail = _trailManager.ActiveTrails[t];
                if (otherTrail == null || otherTrail.PlayerId == myPlayerId || otherTrail.PointCount < 3) continue;

                var points = otherTrail.Points;
                for (int i = 0; i < points.Count - 2; i++)
                {
                    Vector3 p = points[i].position;
                    float distSqr = (myPos.x - p.x) * (myPos.x - p.x) + (myPos.z - p.z) * (myPos.z - p.z);
                    if (distSqr < closestDistSqr)
                    {
                        closestDistSqr = distSqr;
                        targetPos = p;
                        found = true;
                    }
                }
            }

            return found;
        }
    }
}
