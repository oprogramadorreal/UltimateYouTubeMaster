public sealed class ClosePlayerInfo
{
    public ClosePlayerInfo(Player player, Planet planet, bool isOccludedByBlackHole)
    {
        Player = player;
        Planet = planet;
        IsOccludedByBlackHole = isOccludedByBlackHole;
    }

    public Player Player { get; }

    public Planet Planet { get; }

    public bool IsOccludedByBlackHole { get; }
}