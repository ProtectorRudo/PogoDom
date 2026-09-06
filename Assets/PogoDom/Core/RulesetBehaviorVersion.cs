namespace PogoDom.Core
{
    /// <summary>
    /// Version of shared battle algorithms, independent from numeric tuning.
    /// Published rulesets must pin this explicitly. When a resolver/spawner/etc.
    /// changes semantics, add a new version and keep the old branch replayable.
    /// </summary>
    public enum RulesetBehaviorVersion
    {
        V1 = 1,
        V2 = 2
    }
}
