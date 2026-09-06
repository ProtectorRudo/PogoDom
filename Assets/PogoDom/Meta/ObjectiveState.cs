using System;

namespace PogoDom.Meta
{
    public enum ObjectiveScope
    {
        City = 0,
        Nation = 1
    }

    public enum ObjectiveMetric
    {
        MatchesPlayed = 0,
        Wins = 1,
        ScoreBanked = 2,
        TilesPainted = 3,
        TilesStolen = 4,
        BankCrates = 5,
        ArrowsUsed = 6,
        SpeedsUsed = 7,
        MissilesUsed = 8
    }

    public sealed class ObjectiveState
    {
        public string Id { get; }
        public ObjectiveScope Scope { get; }
        public string ScopeId { get; }
        public ObjectiveMetric Metric { get; }
        public int Target { get; }
        public int Current { get; internal set; }
        public bool IsComplete => Current >= Target;

        public ObjectiveState(string id, ObjectiveScope scope, string scopeId, ObjectiveMetric metric, int target)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Objective id cannot be empty.", nameof(id));
            if (string.IsNullOrWhiteSpace(scopeId)) throw new ArgumentException("Scope id cannot be empty.", nameof(scopeId));
            if (target <= 0) throw new ArgumentOutOfRangeException(nameof(target));
            Id = id;
            Scope = scope;
            ScopeId = scopeId.Trim().ToLowerInvariant();
            Metric = metric;
            Target = target;
        }

        internal int Apply(int delta)
        {
            if (delta <= 0 || IsComplete) return 0;
            var before = Current;
            Current = Math.Min(Target, Current + delta);
            return Current - before;
        }
    }
}
