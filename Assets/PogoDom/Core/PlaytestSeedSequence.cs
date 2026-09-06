using System;

namespace PogoDom.Core
{
    /// <summary>
    /// Stable seed sequence for repeatable human playtests. Match zero uses the
    /// exact inspector seed; each rematch advances by an odd full-period uint step.
    /// A same-seed replay can therefore compare two rulesets without changing RNG.
    /// </summary>
    public static class PlaytestSeedSequence
    {
        private const uint GoldenRatioStep = 0x9E3779B9u;

        public static uint SeedFor(uint baseSeed, int matchIndex)
        {
            if (matchIndex < 0) throw new ArgumentOutOfRangeException(nameof(matchIndex));
            return unchecked(baseSeed + GoldenRatioStep * (uint)matchIndex);
        }
    }
}
