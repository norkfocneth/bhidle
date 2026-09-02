using UnityEngine;
using TERRAGRAV.Player;

namespace TERRAGRAV.Multiplayer
{
    /// <summary>
    /// Server-authoritative movement simulation with client-side prediction and tick interpolation.
    /// Ingests NetworkInputData directional vectors each simulation tick.
    /// </summary>
    [RequireComponent(typeof(PlayerMovement))]
    public class NetworkPlayerMovement : MonoBehaviour
    {
        [Header("Interpolation Settings")]
        [SerializeField] private float _snapThreshold = 5.0f;
        [SerializeField] private float _interpolationSmooth = 15f;

        private PlayerMovement _movement;
        private Vector3 _networkPosition;
        private Quaternion _networkRotation;

        private void Awake()
        {
            _movement = GetComponent<PlayerMovement>();
            _networkPosition = transform.position;
            _networkRotation = transform.rotation;
        }

        /// <summary>
        /// Fixed simulation step executed on server and predicted on client.
        /// </summary>
        public void SimulateTick(NetworkInputData input, float deltaTime)
        {
            if (_movement == null || !_movement.CanMove) return;

            if (input.isSteering)
            {
                _movement.SetInputDirection(input.movementDirection);
            }
        }

        /// <summary>
        /// Updates the authoritative position received from the server.
        /// </summary>
        public void ReceiveAuthoritativeTransform(Vector3 serverPos, Quaternion serverRot)
        {
            float distSqr = (transform.position - serverPos).sqrMagnitude;
            if (distSqr > _snapThreshold * _snapThreshold)
            {
                // Hard snap if desync exceeds threshold
                transform.position = serverPos;
                transform.rotation = serverRot;
            }
            else
            {
                _networkPosition = serverPos;
                _networkRotation = serverRot;
            }
        }

        private void Update()
        {
            // Smooth client-side interpolation between ticks
            transform.position = Vector3.Lerp(transform.position, _networkPosition, Time.deltaTime * _interpolationSmooth);
            transform.rotation = Quaternion.Slerp(transform.rotation, _networkRotation, Time.deltaTime * _interpolationSmooth);
        }
    }
}
