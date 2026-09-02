namespace TERRAGRAV.AI
{
    /// <summary>
    /// Autonomous AI Bot Behavioral States.
    /// </summary>
    public enum BotAIState
    {
        Expand,   // Roaming and curving outwards to capture new land
        Hunt,     // Intercepting and cutting a nearby exposed enemy trail
        Retreat,  // Returning back to home base to secure territory or escape danger
        Defend    // Intercepting an intruder who entered own base
    }
}
