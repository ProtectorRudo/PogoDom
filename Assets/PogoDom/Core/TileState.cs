namespace PogoDom.Core
{
    public sealed class TileState
    {
        public const int NeutralOwner = -1;

        public int OwnerPlayerId { get; internal set; } = NeutralOwner;

        public TileState Clone()
        {
            return new TileState { OwnerPlayerId = OwnerPlayerId };
        }
    }
}
