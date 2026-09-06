using NUnit.Framework;
using PogoDom.Meta;
using PogoDom.Verification;

namespace PogoDom.Tests.Verification
{
    public sealed class MatchTicketSignerTests
    {
        [Test]
        public void TamperingCityInvalidatesSignature()
        {
            var signer = Signer();
            var original = Ticket("la-plata");
            var signed = signer.Sign(original);
            Assert.IsTrue(signer.Verify(signed));

            var changed = new MatchTicket(original.MatchId, original.RulesetId, original.Seed, original.LocalPlayerId, new CityId("berisso"), original.NationId, original.CampaignMode, original.TargetCity, original.IssuedUnixSeconds, original.ExpiresUnixSeconds);
            var tampered = new SignedMatchTicket(changed, signed.Signature);
            Assert.IsFalse(signer.Verify(tampered));
        }

        private static MatchTicket Ticket(string city)
        {
            return new MatchTicket("match-1", "launch-m0-2", 9, 0, new CityId(city), new NationId("argentina"), CampaignMode.None, null, 100, 200);
        }

        internal static MatchTicketSigner Signer()
        {
            var key = new byte[32];
            for (var i = 0; i < key.Length; i++) key[i] = (byte)(i + 1);
            return new MatchTicketSigner(key);
        }
    }
}
