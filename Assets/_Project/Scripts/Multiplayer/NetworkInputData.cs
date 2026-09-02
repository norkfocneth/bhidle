using UnityEngine;

namespace TERRAGRAV.Multiplayer
{
    /// <summary>
    /// Compact network input struct transmitted from client to server each simulation tick.
    /// Carries normalized directional steering heading.
    /// </summary>
    public struct NetworkInputData
    {
        public Vector2 movementDirection;
        public bool isSteering;
    }
}
