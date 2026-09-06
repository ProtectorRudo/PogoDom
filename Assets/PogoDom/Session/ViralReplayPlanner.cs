using System;
using System.Collections.Generic;

namespace PogoDom.Session
{
    public enum ViralReplayAspect
    {
        Vertical9x16 = 0,
        Square1x1 = 1
    }

    /// <summary>
    /// A render recipe, not a recorded movie. PogoDom can replay the battle from
    /// tick zero using its seed + sparse direction changes, keep rendering hidden
    /// during WarmupTicks, then show only RenderStartTick..RenderEndTick.
    /// </summary>
    public sealed class ViralReplayRecipe
    {
        public HighlightMoment Highlight { get; }
        public int SimulationStartTick => 0;
        public int RenderStartTick { get; }
        public int HighlightTick { get; }
        public int RenderEndTick { get; }
        public int WarmupTicks => RenderStartTick;
        public float TickSeconds { get; }
        public bool AppendWinnerShowcase { get; }
        public ViralReplayAspect PreferredAspect { get; }
        public string CaptionKey { get; }

        public int RenderTickCount => Math.Max(0, RenderEndTick - RenderStartTick);
        public float RenderDurationSeconds => RenderTickCount * TickSeconds;

        public ViralReplayRecipe(
            HighlightMoment highlight,
            int renderStartTick,
            int renderEndTick,
            float tickSeconds,
            bool appendWinnerShowcase,
            ViralReplayAspect preferredAspect,
            string captionKey)
        {
            Highlight = highlight ?? throw new ArgumentNullException(nameof(highlight));
            if (highlight.Tick < 0) throw new ArgumentException("Highlight has no deterministic tick.", nameof(highlight));
            if (renderStartTick < 0) throw new ArgumentOutOfRangeException(nameof(renderStartTick));
            if (renderEndTick < renderStartTick) throw new ArgumentOutOfRangeException(nameof(renderEndTick));
            if (highlight.Tick < renderStartTick || highlight.Tick > renderEndTick)
                throw new ArgumentException("Highlight tick must be inside the render window.", nameof(highlight));
            if (tickSeconds <= 0f || float.IsNaN(tickSeconds) || float.IsInfinity(tickSeconds))
                throw new ArgumentOutOfRangeException(nameof(tickSeconds));
            if (string.IsNullOrWhiteSpace(captionKey)) throw new ArgumentException("Caption key is required.", nameof(captionKey));

            RenderStartTick = renderStartTick;
            HighlightTick = highlight.Tick;
            RenderEndTick = renderEndTick;
            TickSeconds = tickSeconds;
            AppendWinnerShowcase = appendWinnerShowcase;
            PreferredAspect = preferredAspect;
            CaptionKey = captionKey.Trim();
        }
    }

    public static class ViralReplayPlanner
    {
        public static ViralReplayRecipe CreateBest(
            IReadOnlyList<HighlightMoment> moments,
            int totalMatchTicks,
            float tickSeconds)
        {
            var best = SelectBest(moments);
            return best == null ? null : Create(best, totalMatchTicks, tickSeconds);
        }

        public static HighlightMoment SelectBest(IReadOnlyList<HighlightMoment> moments)
        {
            if (moments == null) throw new ArgumentNullException(nameof(moments));
            HighlightMoment best = null;
            for (var i = 0; i < moments.Count; i++)
            {
                var candidate = moments[i];
                if (candidate == null || candidate.Tick < 0) continue;
                if (best == null || IsBetter(candidate, best)) best = candidate;
            }
            return best;
        }

        public static ViralReplayRecipe Create(
            HighlightMoment highlight,
            int totalMatchTicks,
            float tickSeconds)
        {
            if (highlight == null) throw new ArgumentNullException(nameof(highlight));
            if (highlight.Tick < 0) throw new InvalidOperationException("Cannot build a replay recipe for an untimestamped highlight.");
            if (totalMatchTicks <= 0) throw new ArgumentOutOfRangeException(nameof(totalMatchTicks));
            if (highlight.Tick > totalMatchTicks) throw new ArgumentOutOfRangeException(nameof(highlight), "Highlight is beyond match duration.");
            if (tickSeconds <= 0f || float.IsNaN(tickSeconds) || float.IsInfinity(tickSeconds))
                throw new ArgumentOutOfRangeException(nameof(tickSeconds));

            float preSeconds;
            float postSeconds;
            bool appendWinner;
            string captionKey;
            ContextFor(highlight.Kind, out preSeconds, out postSeconds, out appendWinner, out captionKey);

            var preTicks = SecondsToTicks(preSeconds, tickSeconds);
            var postTicks = SecondsToTicks(postSeconds, tickSeconds);
            var start = Math.Max(0, highlight.Tick - preTicks);
            var end = Math.Min(totalMatchTicks, highlight.Tick + postTicks);

            return new ViralReplayRecipe(
                highlight,
                start,
                end,
                tickSeconds,
                appendWinner,
                ViralReplayAspect.Vertical9x16,
                captionKey);
        }

        private static bool IsBetter(HighlightMoment candidate, HighlightMoment current)
        {
            if (candidate.ViralityScore != current.ViralityScore)
                return candidate.ViralityScore > current.ViralityScore;
            if (candidate.Tick != current.Tick)
                return candidate.Tick > current.Tick;
            if (candidate.Kind != current.Kind)
                return candidate.Kind < current.Kind;
            return candidate.PlayerId < current.PlayerId;
        }

        private static int SecondsToTicks(float seconds, float tickSeconds)
        {
            return (int)Math.Ceiling(seconds / tickSeconds);
        }

        private static void ContextFor(
            HighlightKind kind,
            out float preSeconds,
            out float postSeconds,
            out bool appendWinnerShowcase,
            out string captionKey)
        {
            appendWinnerShowcase = false;
            switch (kind)
            {
                case HighlightKind.BigBank:
                    preSeconds = 2.0f;
                    postSeconds = 1.25f;
                    captionKey = "share.big_bank";
                    break;
                case HighlightKind.LeaderMissile:
                    preSeconds = 1.5f;
                    postSeconds = 1.75f;
                    captionKey = "share.leader_missile";
                    break;
                case HighlightKind.LateLeadChange:
                    preSeconds = 2.5f;
                    postSeconds = 1.75f;
                    captionKey = "share.late_lead_change";
                    break;
                case HighlightKind.PhotoFinish:
                    preSeconds = 4.0f;
                    postSeconds = 0f;
                    appendWinnerShowcase = true;
                    captionKey = "share.photo_finish";
                    break;
                case HighlightKind.ComebackWin:
                    preSeconds = 4.5f;
                    postSeconds = 0f;
                    appendWinnerShowcase = true;
                    captionKey = "share.comeback_win";
                    break;
                case HighlightKind.AreaCapture:
                    preSeconds = 2.0f;
                    postSeconds = 1.5f;
                    captionKey = "share.area_capture";
                    break;
                case HighlightKind.ArenaBlast:
                    preSeconds = 1.5f;
                    postSeconds = 1.5f;
                    captionKey = "share.arena_blast";
                    break;
                default:
                    preSeconds = 2.0f;
                    postSeconds = 1.5f;
                    captionKey = "share.highlight";
                    break;
            }
        }
    }
}
