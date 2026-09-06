using System.Collections.Generic;

namespace PogoDom.Meta
{
    public enum ProgressionRejection
    {
        None = 0,
        DuplicateMatch = 1,
        InvalidMatch = 2,
        BotResult = 3,
        UnknownIdentity = 4,
        IdentityMismatch = 5,
        InvalidCampaign = 6
    }

    public sealed class ObjectiveDelta
    {
        public string ObjectiveId { get; }
        public int Delta { get; }
        public int Current { get; }
        public bool Completed { get; }

        public ObjectiveDelta(string objectiveId, int delta, int current, bool completed)
        {
            ObjectiveId = objectiveId;
            Delta = delta;
            Current = current;
            Completed = completed;
        }
    }

    public sealed class ProgressionReceipt
    {
        public bool Applied { get; internal set; }
        public ProgressionRejection Rejection { get; internal set; }
        public int CityPointDelta { get; internal set; }
        public int NationPointDelta { get; internal set; }
        public int TargetHpDelta { get; internal set; }
        public bool ConquestTriggered { get; internal set; }
        public List<ObjectiveDelta> Objectives { get; } = new List<ObjectiveDelta>();
    }
}
