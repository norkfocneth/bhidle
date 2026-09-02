using UnityEngine;
using TERRAGRAV.Input;

namespace TERRAGRAV.AI
{
    /// <summary>
    /// Adapts the autonomous BotAI decision-making into the standard IInputProvider interface.
    /// Allows the exact same PlayerMovement and PlayerController components to drive both humans and bots.
    /// </summary>
    public class BotInputProvider : IInputProvider
    {
        private Vector2 _currentDirection = Vector2.up;

        public void SetDirection(Vector2 direction)
        {
            if (direction.sqrMagnitude > 0.001f)
            {
                _currentDirection = direction.normalized;
            }
        }

        public Vector2 GetMovementDirection()
        {
            return _currentDirection;
        }

        public bool HasInput()
        {
            return true;
        }
    }
}
