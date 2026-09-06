using System;

namespace PogoDom.Core
{
    public enum ArenaHazardKind
    {
        Tnt = 0
    }

    [Serializable]
    public sealed class ArenaHazard
    {
        public int Id { get; }
        public ArenaHazardKind Kind { get; }
        public GridPos Position { get; }
        public int BlastRadius { get; }
        public int TicksRemaining { get; internal set; }

        public ArenaHazard(int id, ArenaHazardKind kind, GridPos position, int blastRadius, int ticksRemaining)
        {
            Id = id;
            Kind = kind;
            Position = position;
            BlastRadius = Math.Max(0, blastRadius);
            TicksRemaining = Math.Max(1, ticksRemaining);
        }

        public bool Contains(GridPos position)
        {
            return Math.Abs(position.X - Position.X) <= BlastRadius &&
                   Math.Abs(position.Y - Position.Y) <= BlastRadius;
        }
    }
}
