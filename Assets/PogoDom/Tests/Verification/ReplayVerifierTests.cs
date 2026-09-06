using System.Collections.Generic;
using NUnit.Framework;
using PogoDom.Core;
using PogoDom.Meta;
using PogoDom.Verification;

namespace PogoDom.Tests.Verification
{
    public sealed class ReplayVerifierTests
    {
        [Test]
        public void ValidSignedOfflineInputLogReplaysToHumanBattle()
        {
            var signer = MatchTicketSignerTests.Signer();
            var ticket = Ticket();
            var submission = new BattleSubmission(signer.Sign(ticket), new List<DirectionChange>
            {
                new DirectionChange(2, Direction.Right),
                new DirectionChange(8, Direction.Down),
                new DirectionChange(14, Direction.Left)
            });

            var result = new ReplayVerifier(signer, RulesetRegistry.CreateCurrent()).Verify(submission, 150);

            Assert.IsTrue(result.Accepted);
            Assert.IsTrue(result.Verified.Battle.IsHuman);
            Assert.AreEqual("match-1", result.Verified.Battle.MatchId);
            Assert.AreEqual(new CityId("la-plata"), result.Verified.Battle.CityId);
        }

        [Test]
        public void ExpiredTicketIsRejectedBeforeReplay()
        {
            var signer = MatchTicketSignerTests.Signer();
            var submission = new BattleSubmission(signer.Sign(Ticket()), new DirectionChange[0]);
            var result = new ReplayVerifier(signer, RulesetRegistry.CreateCurrent()).Verify(submission, 201);
            Assert.IsFalse(result.Accepted);
            Assert.AreEqual(VerificationFailure.Expired, result.Failure);
        }

        [Test]
        public void DuplicateOrUnsortedInputTicksAreRejected()
        {
            var signer = MatchTicketSignerTests.Signer();
            var submission = new BattleSubmission(signer.Sign(Ticket()), new[]
            {
                new DirectionChange(4, Direction.Right),
                new DirectionChange(4, Direction.Down)
            });
            var result = new ReplayVerifier(signer, RulesetRegistry.CreateCurrent()).Verify(submission, 150);
            Assert.AreEqual(VerificationFailure.InvalidInputLog, result.Failure);
        }

        [Test]
        public void SameTicketAndInputsAlwaysProduceSameVerifiedResult()
        {
            var signer = MatchTicketSignerTests.Signer();
            var signed = signer.Sign(Ticket());
            var inputs = new[] { new DirectionChange(3, Direction.Right), new DirectionChange(20, Direction.Down) };
            var verifier = new ReplayVerifier(signer, RulesetRegistry.CreateCurrent());

            var a = verifier.Verify(new BattleSubmission(signed, inputs), 150).Verified.Battle;
            var b = verifier.Verify(new BattleSubmission(signed, inputs), 150).Verified.Battle;

            Assert.AreEqual(a.Placement, b.Placement);
            Assert.AreEqual(a.Score, b.Score);
            Assert.AreEqual(a.TilesPainted, b.TilesPainted);
            Assert.AreEqual(a.TilesStolen, b.TilesStolen);
            Assert.AreEqual(a.BankedPoints, b.BankedPoints);
        }

        [Test]
        public void VerifiedBattleStillCannotProgressTwice()
        {
            var signer = MatchTicketSignerTests.Signer();
            var verified = new ReplayVerifier(signer, RulesetRegistry.CreateCurrent()).Verify(new BattleSubmission(signer.Sign(Ticket()), new DirectionChange[0]), 150).Verified;
            var world = new WorldState();
            var nation = new NationId("argentina");
            var city = new CityId("la-plata");
            world.AddNation(new NationState(nation));
            world.AddCity(new CityState(city, nation, 800000));
            var engine = new MetaProgressionEngine();

            Assert.IsTrue(engine.Apply(world, verified.Battle, verified.Campaign).Applied);
            Assert.AreEqual(ProgressionRejection.DuplicateMatch, engine.Apply(world, verified.Battle, verified.Campaign).Rejection);
        }

        private static MatchTicket Ticket()
        {
            return new MatchTicket("match-1", "launch-m0-2", 12345, 0, new CityId("la-plata"), new NationId("argentina"), CampaignMode.None, null, 100, 200);
        }
    }
}
