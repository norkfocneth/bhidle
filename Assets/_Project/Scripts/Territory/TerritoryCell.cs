using System;

namespace TERRAGRAV.Territory
{
    /// <summary>
    /// Compact struct representing the state of an individual territory grid cell.
    /// Memory footprint is minimal for mobile cache performance.
    /// </summary>
    [Serializable]
    public struct TerritoryCell
    {
        public const int UNCLAIMED = -1;

        /// <summary>
        /// ID of the player who currently owns this cell (-1 for unclaimed).
        /// </summary>
        public int ownerId;

        /// <summary>
        /// Whether this cell is part of a permanently claimed territory surface.
        /// </summary>
        public bool isCaptured;

        /// <summary>
        /// Whether this cell lies on the outer perimeter boundary of the player's territory.
        /// </summary>
        public bool isBoundary;

        public static TerritoryCell CreateEmpty()
        {
            return new TerritoryCell
            {
                ownerId = UNCLAIMED,
                isCaptured = false,
                isBoundary = false
            };
        }
    }
}
