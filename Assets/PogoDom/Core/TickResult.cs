using System.Collections.Generic;

namespace PogoDom.Core
{
    public sealed class MovementStep
    {
        public int PlayerId { get; }
        public GridPos From { get; }
        public GridPos To { get; }
        public int Phase { get; }

        public MovementStep(int playerId, GridPos from, GridPos to, int phase)
        {
            PlayerId = playerId;
            From = from;
            To = to;
            Phase = phase;
        }
    }

    public sealed class TickResult
    {
        public Dictionary<int, GridPos> FromPositions { get; } = new Dictionary<int, GridPos>();
        public Dictionary<int, GridPos> ToPositions { get; } = new Dictionary<int, GridPos>();
        public List<MovementStep> MovementSteps { get; } = new List<MovementStep>();
        public List<MatchEvent> Events { get; } = new List<MatchEvent>();
    }
}
