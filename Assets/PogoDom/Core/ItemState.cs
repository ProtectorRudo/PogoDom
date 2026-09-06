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
        public PowerUpKind ContainedPower { get; }

        public bool IsMysteryCrate => Kind == PowerUpKind.MysteryCrate;

        public ItemState(
            int id,
            PowerUpKind kind,
            GridPos position,
            Direction arrowDirection = Direction.Up,
            PowerUpKind containedPower = PowerUpKind.None)
        {
            if (kind == PowerUpKind.MysteryCrate && !MysteryCrateTable.IsValidPayload(containedPower))
                throw new ArgumentException("Mystery crates require a valid pre-rolled power payload.", nameof(containedPower));
            if (kind != PowerUpKind.MysteryCrate && containedPower != PowerUpKind.None)
                throw new ArgumentException("Only mystery crates may contain a hidden power payload.", nameof(containedPower));

            Id = id;
            Kind = kind;
            Position = position;
            ArrowDirection = arrowDirection;
            ContainedPower = containedPower;
        }
    }
}
