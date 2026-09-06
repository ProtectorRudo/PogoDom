using System;
using System.Collections.Generic;
using PogoDom.Core;
using PogoDom.Meta;

namespace PogoDom.Verification
{
    public enum VerificationFailure
    {
        None = 0,
        InvalidSignature = 1,
        NotYetValid = 2,
        Expired = 3,
        UnknownRuleset = 4,
        InvalidInputLog = 5,
        InvalidLocalPlayer = 6,
        ReplayFailure = 7
    }

    public sealed class VerifiedBattle
    {
        public BattleResult Battle { get; }
        public CampaignContext Campaign { get; }

        public VerifiedBattle(BattleResult battle, CampaignContext campaign)
        {
            Battle = battle;
            Campaign = campaign;
        }
    }

    public sealed class ReplayVerificationResult
    {
        public bool Accepted { get; }
        public VerificationFailure Failure { get; }
        public VerifiedBattle Verified { get; }

        private ReplayVerificationResult(bool accepted, VerificationFailure failure, VerifiedBattle verified)
        {
            Accepted = accepted;
            Failure = failure;
            Verified = verified;
        }

        public static ReplayVerificationResult Accept(VerifiedBattle verified) => new ReplayVerificationResult(true, VerificationFailure.None, verified);
        public static ReplayVerificationResult Reject(VerificationFailure failure) => new ReplayVerificationResult(false, failure, null);
    }

    public sealed class ReplayVerifier
    {
        private readonly MatchTicketSigner _signer;
        private readonly RulesetRegistry _rulesets;

        public ReplayVerifier(MatchTicketSigner signer, RulesetRegistry rulesets)
        {
            _signer = signer ?? throw new ArgumentNullException(nameof(signer));
            _rulesets = rulesets ?? throw new ArgumentNullException(nameof(rulesets));
        }

        public ReplayVerificationResult Verify(BattleSubmission submission, long nowUnixSeconds)
        {
            if (submission == null || !_signer.Verify(submission.SignedTicket))
                return ReplayVerificationResult.Reject(VerificationFailure.InvalidSignature);

            var ticket = submission.SignedTicket.Ticket;
            if (nowUnixSeconds < ticket.IssuedUnixSeconds)
                return ReplayVerificationResult.Reject(VerificationFailure.NotYetValid);
            if (nowUnixSeconds > ticket.ExpiresUnixSeconds)
                return ReplayVerificationResult.Reject(VerificationFailure.Expired);

            var ruleset = _rulesets.Get(ticket.RulesetId);
            if (ruleset == null)
                return ReplayVerificationResult.Reject(VerificationFailure.UnknownRuleset);

            var config = ruleset.CreateConfig();
            var maximumTicks = (int)Math.Ceiling(config.MatchSeconds / config.TickSeconds) + 1;
            if (!ValidateInputs(submission.Inputs, maximumTicks))
                return ReplayVerificationResult.Reject(VerificationFailure.InvalidInputLog);

            try
            {
                var state = MatchFactory.CreateClassicPrototype(config);
                var local = state.PlayerById(ticket.LocalPlayerId);
                if (local == null || !local.IsHuman)
                    return ReplayVerificationResult.Reject(VerificationFailure.InvalidLocalPlayer);

                var runner = new MatchRunner(config, new XorShiftRandom(ticket.Seed));
                runner.Initialize(state);
                var recorder = new BattleSessionRecorder(ticket.MatchId, ticket.LocalPlayerId, ticket.CityId, ticket.NationId);
                var inputIndex = 0;
                var currentDirection = local.CurrentDirection;

                while (!state.IsFinished)
                {
                    while (inputIndex < submission.Inputs.Count && submission.Inputs[inputIndex].Tick == state.Tick)
                    {
                        currentDirection = submission.Inputs[inputIndex].Direction;
                        inputIndex++;
                    }

                    var input = new Dictionary<int, Direction> { [ticket.LocalPlayerId] = currentDirection };
                    var tick = runner.Tick(state, input);
                    recorder.Observe(tick);
                }

                var battle = recorder.Complete(state, true);
                return ReplayVerificationResult.Accept(new VerifiedBattle(battle, ticket.Campaign()));
            }
            catch
            {
                return ReplayVerificationResult.Reject(VerificationFailure.ReplayFailure);
            }
        }

        private static bool ValidateInputs(IReadOnlyList<DirectionChange> inputs, int maximumTicks)
        {
            var previousTick = -1;
            for (var i = 0; i < inputs.Count; i++)
            {
                var entry = inputs[i];
                if (entry.Tick < 0 || entry.Tick >= maximumTicks || entry.Tick <= previousTick)
                    return false;
                if (entry.Direction == Direction.None)
                    return false;
                if (entry.Direction < Direction.Up || entry.Direction > Direction.Left)
                    return false;
                previousTick = entry.Tick;
            }
            return true;
        }
    }
}
