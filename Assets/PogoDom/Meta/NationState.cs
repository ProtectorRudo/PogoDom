namespace PogoDom.Meta
{
    public sealed class NationState
    {
        public NationId Id { get; }
        public long GlobalPoints { get; internal set; }

        public NationState(NationId id)
        {
            Id = id;
        }
    }
}
