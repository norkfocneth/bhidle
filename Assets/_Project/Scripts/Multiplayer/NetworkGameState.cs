namespace TERRAGRAV.Multiplayer
{
    /// <summary>
    /// Network synchronized match lifecycle states.
    /// </summary>
    public enum NetworkMatchState
    {
        WaitingForPlayers, // Waiting in lobby / room for required player count
        Countdown,         // 3-2-1 Synchronized start sequence
        Playing,           // Live active territory capture match
        MatchEnded         // Match concluded, leaderboard results shown
    }
}
