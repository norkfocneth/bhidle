using UnityEngine;

namespace TERRAGRAV.Input
{
    /// <summary>
    /// Standalone Input Provider for PC and Unity Editor testing.
    /// Supports W/A/S/D, Arrow keys, and Mouse cursor steering without per-frame garbage allocations.
    /// </summary>
    public class StandaloneInputProvider : IInputProvider
    {
        private Vector2 _lastDirection = Vector2.up;
        private readonly bool _supportMouseSteering;

        public StandaloneInputProvider(bool supportMouseSteering = true)
        {
            _supportMouseSteering = supportMouseSteering;
        }

        public Vector2 GetMovementDirection()
        {
            // 1. Check Keyboard Inputs (WASD / Arrows)
            float horizontal = 0f;
            float vertical = 0f;

            if (UnityEngine.Input.GetKey(KeyCode.W) || UnityEngine.Input.GetKey(KeyCode.UpArrow)) vertical += 1f;
            if (UnityEngine.Input.GetKey(KeyCode.S) || UnityEngine.Input.GetKey(KeyCode.DownArrow)) vertical -= 1f;
            if (UnityEngine.Input.GetKey(KeyCode.D) || UnityEngine.Input.GetKey(KeyCode.RightArrow)) horizontal += 1f;
            if (UnityEngine.Input.GetKey(KeyCode.A) || UnityEngine.Input.GetKey(KeyCode.LeftArrow)) horizontal -= 1f;

            if (Mathf.Abs(horizontal) > 0.01f || Mathf.Abs(vertical) > 0.01f)
            {
                _lastDirection.x = horizontal;
                _lastDirection.y = vertical;
                _lastDirection.Normalize();
                return _lastDirection;
            }

            // 2. Check Mouse Steering if active
            if (_supportMouseSteering && UnityEngine.Input.GetMouseButton(0))
            {
                Vector3 mousePos = UnityEngine.Input.mousePosition;
                float halfWidth = Screen.width * 0.5f;
                float halfHeight = Screen.height * 0.5f;

                float mouseDx = mousePos.x - halfWidth;
                float mouseDy = mousePos.y - halfHeight;

                if ((mouseDx * mouseDx) + (mouseDy * mouseDy) > 100f) // small deadzone
                {
                    _lastDirection.x = mouseDx;
                    _lastDirection.y = mouseDy;
                    _lastDirection.Normalize();
                    return _lastDirection;
                }
            }

            return _lastDirection;
        }

        public bool HasInput()
        {
            return UnityEngine.Input.GetKey(KeyCode.W) ||
                   UnityEngine.Input.GetKey(KeyCode.A) ||
                   UnityEngine.Input.GetKey(KeyCode.S) ||
                   UnityEngine.Input.GetKey(KeyCode.D) ||
                   UnityEngine.Input.GetKey(KeyCode.UpArrow) ||
                   UnityEngine.Input.GetKey(KeyCode.DownArrow) ||
                   UnityEngine.Input.GetKey(KeyCode.LeftArrow) ||
                   UnityEngine.Input.GetKey(KeyCode.RightArrow) ||
                   (_supportMouseSteering && UnityEngine.Input.GetMouseButton(0));
        }
    }
}
