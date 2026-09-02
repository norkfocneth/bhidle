using UnityEngine;
using TERRAGRAV.Core;
using TERRAGRAV.Input;

namespace TERRAGRAV.Player
{
    /// <summary>
    /// Bridges the active IInputProvider to the PlayerMovement component.
    /// </summary>
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerInput : MonoBehaviour
    {
        private PlayerMovement _movement;
        private InputManager _inputManager;
        private bool _isLocalPlayer = true;

        private void Awake()
        {
            _movement = GetComponent<PlayerMovement>();
        }

        private void Start()
        {
            if (ServiceLocator.TryGet(out InputManager manager))
            {
                _inputManager = manager;
            }
        }

        public void SetIsLocalPlayer(bool isLocal)
        {
            _isLocalPlayer = isLocal;
        }

        private void Update()
        {
            if (!_isLocalPlayer) return;

            if (_inputManager == null)
            {
                ServiceLocator.TryGet(out _inputManager);
                if (_inputManager == null) return;
            }

            Vector2 inputDir = _inputManager.GetMovementDirection();
            _movement.SetInputDirection(inputDir);
        }
    }
}
