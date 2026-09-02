using UnityEngine;

namespace TERRAGRAV.Player
{
    /// <summary>
    /// Handles continuous forward locomotion, rotational steering, acceleration, and boundary limits.
    /// Operates entirely on the 2.5D horizontal X-Z plane.
    /// </summary>
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Configuration Reference")]
        [SerializeField] private PlayerSettingsSO _settings;

        private float _currentSpeed = 0f;
        private float _targetSpeed = 12f;
        private Vector3 _currentHeading = Vector3.forward;
        private Vector3 _desiredHeading = Vector3.forward;
        private bool _canMove = true;
        private float _elevationY = 0.5f;

        public float CurrentSpeed => _currentSpeed;
        public Vector3 CurrentHeading => _currentHeading;
        public bool CanMove => _canMove;

        public void Initialize(PlayerSettingsSO settings)
        {
            _settings = settings;
            if (_settings != null)
            {
                _targetSpeed = _settings.MovementSpeed;
                _currentSpeed = _settings.MovementSpeed;
                _elevationY = _settings.PlayerHeight;
            }
            else
            {
                Debug.LogError("[PlayerMovement] Missing PlayerSettingsSO reference during initialization!");
                _targetSpeed = 12f;
                _currentSpeed = 12f;
                _elevationY = 0.5f;
            }

            _currentHeading = transform.forward;
            _desiredHeading = transform.forward;
        }

        /// <summary>
        /// Updates the intended movement heading based on 2D input (X: horizontal, Y: vertical).
        /// </summary>
        public void SetInputDirection(Vector2 inputDirection)
        {
            if (inputDirection.sqrMagnitude > 0.001f)
            {
                // Map 2D input (X, Y) to 3D horizontal world space (X, 0, Z)
                _desiredHeading.x = inputDirection.x;
                _desiredHeading.y = 0f;
                _desiredHeading.z = inputDirection.y;
                _desiredHeading.Normalize();
            }
        }

        public void SetCanMove(bool canMove)
        {
            _canMove = canMove;
            if (!_canMove)
            {
                _targetSpeed = 0f;
            }
            else if (_settings != null)
            {
                _targetSpeed = _settings.MovementSpeed;
            }
        }

        public void SetTargetSpeed(float speed)
        {
            _targetSpeed = speed;
        }

        public void ResetSpeed()
        {
            if (_settings != null)
            {
                _targetSpeed = _settings.MovementSpeed;
            }
        }

        private void Update()
        {
            if (!_canMove && _currentSpeed <= 0.001f) return;

            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            float turnSpeed = _settings != null ? _settings.TurnSpeed : 500f;
            float accel = _settings != null ? _settings.Acceleration : 30f;
            float decel = _settings != null ? _settings.Deceleration : 30f;
            float boundaryLimit = _settings != null ? _settings.BoundaryLimit : 98f;

            // 1. Smoothly accelerate / decelerate towards target speed
            if (_currentSpeed < _targetSpeed)
            {
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, _targetSpeed, accel * dt);
            }
            else if (_currentSpeed > _targetSpeed)
            {
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, _targetSpeed, decel * dt);
            }

            // 2. Smoothly rotate towards the desired heading
            if (_desiredHeading.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(_desiredHeading, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * dt);
                _currentHeading = transform.forward;
            }

            // 3. Move continuously forward along current heading
            Vector3 movementDelta = _currentHeading * (_currentSpeed * dt);
            Vector3 targetPos = transform.position + movementDelta;
            targetPos.y = _elevationY;

            // 4. Clamp strictly within arena boundaries
            targetPos.x = Mathf.Clamp(targetPos.x, -boundaryLimit, boundaryLimit);
            targetPos.z = Mathf.Clamp(targetPos.z, -boundaryLimit, boundaryLimit);

            transform.position = targetPos;
        }
    }
}
