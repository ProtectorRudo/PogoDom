namespace PogoDom.Core
{
    public enum MatchEventType
    {
        TilePainted,
        PlayerMoved,
        PlayerBlocked,
        Banked,
        ArrowUsed,
        ItemSpawned,
        ItemConsumed,
        MatchFinished
    }

    public sealed class MatchEvent
    {
        public MatchEventType Type { get; }
        public int PlayerId { get; }
        public GridPos Position { get; }
        public int Value { get; }
        public PowerUpKind ItemKind { get; }

        public MatchEvent(MatchEventType type, int playerId = -1, GridPos position = default, int value = 0, PowerUpKind itemKind = PowerUpKind.None)
        {
            Type = type;
            PlayerId = playerId;
            Position = position;
            Value = value;
            ItemKind = itemKind;
        }
    }
}
