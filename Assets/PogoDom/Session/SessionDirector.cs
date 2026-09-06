using System;
using PogoDom.Meta;

namespace PogoDom.Session
{
    public enum SessionPhase
    {
        Ready = 0,
        Playing = 1,
        Results = 2
    }

    public enum ResultPrimaryAction
    {
        Rematch = 0
    }

    public sealed class ResultScreenModel
    {
        public BattleResult Battle { get; }
        public ProgressionReceipt Progression { get; }
        public ObjectiveSpotlight Objective { get; }
        public HighlightMoment Highlight { get; }
        public ResultPrimaryAction PrimaryAction => ResultPrimaryAction.Rematch;

        public ResultScreenModel(BattleResult battle, ProgressionReceipt progression, ObjectiveSpotlight objective, HighlightMoment highlight)
        {
            Battle = battle;
            Progression = progression;
            Objective = objective;
            Highlight = highlight;
        }
    }

    public sealed class SessionDirector
    {
        public SessionPhase Phase { get; private set; } = SessionPhase.Ready;
        public string ActiveMatchId { get; private set; }
        public int MatchesStarted { get; private set; }
        public int RematchesRequested { get; private set; }

        public void StartImmediate(string matchId)
        {
            if (string.IsNullOrWhiteSpace(matchId)) throw new ArgumentException("Match id cannot be empty.", nameof(matchId));
            if (Phase == SessionPhase.Playing) throw new InvalidOperationException("A match is already active.");
            ActiveMatchId = matchId;
            Phase = SessionPhase.Playing;
            MatchesStarted++;
        }

        public ResultScreenModel Complete(
            BattleResult battle,
            ProgressionReceipt progression,
            ObjectiveSpotlight objective,
            HighlightMoment highlight)
        {
            if (Phase != SessionPhase.Playing) throw new InvalidOperationException("No active match to complete.");
            if (battle == null) throw new ArgumentNullException(nameof(battle));
            if (battle.MatchId != ActiveMatchId) throw new InvalidOperationException("Battle result does not match the active session match.");

            Phase = SessionPhase.Results;
            return new ResultScreenModel(battle, progression, objective, highlight);
        }

        public void Rematch(string nextMatchId)
        {
            if (Phase != SessionPhase.Results) throw new InvalidOperationException("Rematch is only available from results.");
            if (string.IsNullOrWhiteSpace(nextMatchId)) throw new ArgumentException("Next match id cannot be empty.", nameof(nextMatchId));
            if (nextMatchId == ActiveMatchId) throw new InvalidOperationException("A rematch must receive a new idempotency key.");

            RematchesRequested++;
            ActiveMatchId = nextMatchId;
            Phase = SessionPhase.Playing;
            MatchesStarted++;
        }
    }
}
