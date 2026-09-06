using System.Collections.Generic;

namespace PogoDom.Core
{
    public sealed class TickResult
    {
        public Dictionary<int, GridPos> FromPositions { get; } = new Dictionary<int, GridPos>();
        public Dictionary<int, GridPos> ToPositions { get; } = new Dictionary<int, GridPos>();
        public List<MatchEvent> Events { get; } = new List<MatchEvent>();
    }
}
