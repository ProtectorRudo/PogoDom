using System;
using System.Collections.Generic;
using PogoDom.Core;

namespace PogoDom.Session
{
    public enum ShowcaseOutcomeTone
    {
        Victory = 0,
        Defeat = 1
    }

    /// <summary>
    /// Pure presentation plan for turning a four-player result into a readable
    /// one-on-one story: the human versus the only rival that matters at the
    /// finish. This never changes ranking or battle state.
    /// </summary>
    public sealed class WinnerShowcasePlan
    {
        public int HumanPlayerId { get; }
        public int HumanPlacement { get; }
        public int WinnerPlayerId { get; }
        public int RunnerUpPlayerId { get; }
        public int FocalRivalPlayerId { get; }
        public int CelebrationPlayerId { get; }
        public bool IsHumanWinner { get; }
        public ShowcaseOutcomeTone Tone => IsHumanWinner ? ShowcaseOutcomeTone.Victory : ShowcaseOutcomeTone.Defeat;

        public WinnerShowcasePlan(
            int humanPlayerId,
            int humanPlacement,
            int winnerPlayerId,
            int runnerUpPlayerId,
            int focalRivalPlayerId,
            int celebrationPlayerId)
        {
            if (humanPlacement < 1) throw new ArgumentOutOfRangeException(nameof(humanPlacement));
            HumanPlayerId = humanPlayerId;
            HumanPlacement = humanPlacement;
            WinnerPlayerId = winnerPlayerId;
            RunnerUpPlayerId = runnerUpPlayerId;
            FocalRivalPlayerId = focalRivalPlayerId;
            CelebrationPlayerId = celebrationPlayerId;
            IsHumanWinner = winnerPlayerId == humanPlayerId;
        }
    }

    public static class WinnerShowcaseResolver
    {
        public static WinnerShowcasePlan Create(MatchState state, int humanPlayerId)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (state.Players.Count < 2) throw new InvalidOperationException("Winner showcase requires at least two players.");
            if (state.PlayerById(humanPlayerId) == null) throw new ArgumentException("Human player is not part of the match.", nameof(humanPlayerId));

            var standings = MatchOutcome.Standings(state);
            if (standings.Count < 2) throw new InvalidOperationException("Winner showcase requires a runner-up.");

            var winnerId = standings[0].PlayerId;
            var runnerUpId = standings[1].PlayerId;
            var humanPlacement = PlacementOf(standings, humanPlayerId);

            // If the human won, frame the runner-up as the rival they beat.
            // If the human lost, frame the actual winner as the rival to beat
            // next time. The other two players remain visible in standings but
            // do not dilute the final VS story.
            var focalRivalId = winnerId == humanPlayerId ? runnerUpId : winnerId;

            return new WinnerShowcasePlan(
                humanPlayerId,
                humanPlacement,
                winnerId,
                runnerUpId,
                focalRivalId,
                winnerId);
        }

        private static int PlacementOf(IReadOnlyList<MatchStanding> standings, int playerId)
        {
            for (var i = 0; i < standings.Count; i++)
                if (standings[i].PlayerId == playerId) return i + 1;
            throw new InvalidOperationException("Player disappeared from standings.");
        }
    }

    /// <summary>
    /// Initial mobile presentation hypothesis. Values live outside Core rules
    /// so later feel tests can tune them without invalidating battle replays.
    /// </summary>
    public sealed class WinnerShowcaseTimeline
    {
        public float ResultFreezeSeconds { get; }
        public float VsRevealSeconds { get; }
        public float CelebrationStartSeconds { get; }
        public float PrimaryCtaSeconds { get; }
        public float CelebrationWindowSeconds { get; }

        public WinnerShowcaseTimeline(
            float resultFreezeSeconds,
            float vsRevealSeconds,
            float celebrationStartSeconds,
            float primaryCtaSeconds,
            float celebrationWindowSeconds)
        {
            if (resultFreezeSeconds < 0f) throw new ArgumentOutOfRangeException(nameof(resultFreezeSeconds));
            if (vsRevealSeconds < resultFreezeSeconds) throw new ArgumentOutOfRangeException(nameof(vsRevealSeconds));
            if (celebrationStartSeconds < vsRevealSeconds) throw new ArgumentOutOfRangeException(nameof(celebrationStartSeconds));
            if (primaryCtaSeconds < celebrationStartSeconds) throw new ArgumentOutOfRangeException(nameof(primaryCtaSeconds));
            if (celebrationWindowSeconds <= 0f) throw new ArgumentOutOfRangeException(nameof(celebrationWindowSeconds));

            ResultFreezeSeconds = resultFreezeSeconds;
            VsRevealSeconds = vsRevealSeconds;
            CelebrationStartSeconds = celebrationStartSeconds;
            PrimaryCtaSeconds = primaryCtaSeconds;
            CelebrationWindowSeconds = celebrationWindowSeconds;
        }

        public static WinnerShowcaseTimeline FirstMobileHypothesis()
        {
            return new WinnerShowcaseTimeline(
                resultFreezeSeconds: 0.25f,
                vsRevealSeconds: 0.45f,
                celebrationStartSeconds: 0.60f,
                primaryCtaSeconds: 1.45f,
                celebrationWindowSeconds: 2.40f);
        }
    }
}
