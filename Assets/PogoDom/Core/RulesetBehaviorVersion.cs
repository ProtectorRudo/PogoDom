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
        V2 = 2,
        // V3 introduces the adaptive rival decision path for Medium bots.
        // Published V1/V2 rulesets remain on their historical bot behavior.
        V3 = 3
    }
}
