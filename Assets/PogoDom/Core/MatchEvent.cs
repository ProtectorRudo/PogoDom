namespace PogoDom.Core
{
    public enum MatchEventType
    {
        TilePainted,
        TileStolen,
        PlayerMoved,
        PlayerBlocked,
        Banked,
        ArrowUsed,
        SpeedActivated,
        MissileFired,
        PlayerStunned,
        ItemSpawned,
        ItemConsumed,
        MatchFinished
    }

    public sealed class MatchEvent
    {
        public MatchEventType Type { get; }
        public int PlayerId { get; }
        public int SecondaryPlayerId { get; }
        public GridPos Position { get; }
        public int Value { get; }
        public PowerUpKind ItemKind { get; }

        public MatchEvent(
            MatchEventType type,
            int playerId = -1,
            GridPos position = default,
            int value = 0,
            PowerUpKind itemKind = PowerUpKind.None,
            int secondaryPlayerId = -1)
        {
            Type = type;
            PlayerId = playerId;
            SecondaryPlayerId = secondaryPlayerId;
            Position = position;
            Value = value;
            ItemKind = itemKind;
        }
    }
}
