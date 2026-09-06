using System;

namespace PogoDom.Core
{
    [Serializable]
    public sealed class ItemState
    {
        public int Id { get; }
        public PowerUpKind Kind { get; }
        public GridPos Position { get; internal set; }
        public Direction ArrowDirection { get; internal set; }

        public ItemState(int id, PowerUpKind kind, GridPos position, Direction arrowDirection = Direction.Up)
        {
            Id = id;
            Kind = kind;
            Position = position;
            ArrowDirection = arrowDirection;
        }
    }
}
