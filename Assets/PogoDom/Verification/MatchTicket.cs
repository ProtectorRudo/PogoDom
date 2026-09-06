using System;
using System.Globalization;
using System.Text;
using PogoDom.Meta;

namespace PogoDom.Verification
{
    internal static class TicketToken
    {
        public static string Normalize(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(name + " cannot be empty.", name);
            var normalized = value.Trim().ToLowerInvariant();
            for (var i = 0; i < normalized.Length; i++)
            {
                var c = normalized[i];
                if (!(char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == '.'))
                    throw new ArgumentException(name + " contains an unsupported character.", name);
            }
            return normalized;
        }
    }

    public sealed class MatchTicket
    {
        public string MatchId { get; }
        public string RulesetId { get; }
        public uint Seed { get; }
        public int LocalPlayerId { get; }
        public CityId CityId { get; }
        public NationId NationId { get; }
        public CampaignMode CampaignMode { get; }
        public CityId? TargetCity { get; }
        public long IssuedUnixSeconds { get; }
        public long ExpiresUnixSeconds { get; }

        public MatchTicket(
            string matchId,
            string rulesetId,
            uint seed,
            int localPlayerId,
            CityId cityId,
            NationId nationId,
            CampaignMode campaignMode,
            CityId? targetCity,
            long issuedUnixSeconds,
            long expiresUnixSeconds)
        {
            MatchId = TicketToken.Normalize(matchId, nameof(matchId));
            RulesetId = TicketToken.Normalize(rulesetId, nameof(rulesetId));
            if (localPlayerId < 0 || localPlayerId > 3) throw new ArgumentOutOfRangeException(nameof(localPlayerId));
            if (expiresUnixSeconds <= issuedUnixSeconds) throw new ArgumentException("Ticket expiry must be after issuance.");
            if (campaignMode == CampaignMode.Attack && !targetCity.HasValue) throw new ArgumentException("Attack ticket requires a target city.");
            if (campaignMode != CampaignMode.Attack && targetCity.HasValue && targetCity.Value != cityId) throw new ArgumentException("Only attack tickets can target another city.");
            Seed = seed;
            LocalPlayerId = localPlayerId;
            CityId = cityId;
            NationId = nationId;
            CampaignMode = campaignMode;
            TargetCity = targetCity;
            IssuedUnixSeconds = issuedUnixSeconds;
            ExpiresUnixSeconds = expiresUnixSeconds;
        }

        public CampaignContext Campaign()
        {
            if (CampaignMode == CampaignMode.Attack) return CampaignContext.Attack(CityId, TargetCity.Value);
            if (CampaignMode == CampaignMode.Defense) return CampaignContext.Defense(CityId);
            return CampaignContext.None(CityId);
        }

        internal string CanonicalPayload()
        {
            var builder = new StringBuilder();
            builder.Append(MatchId).Append('|')
                .Append(RulesetId).Append('|')
                .Append(Seed.ToString(CultureInfo.InvariantCulture)).Append('|')
                .Append(LocalPlayerId.ToString(CultureInfo.InvariantCulture)).Append('|')
                .Append(CityId.Value).Append('|')
                .Append(NationId.Value).Append('|')
                .Append(((int)CampaignMode).ToString(CultureInfo.InvariantCulture)).Append('|')
                .Append(TargetCity.HasValue ? TargetCity.Value.Value : "-").Append('|')
                .Append(IssuedUnixSeconds.ToString(CultureInfo.InvariantCulture)).Append('|')
                .Append(ExpiresUnixSeconds.ToString(CultureInfo.InvariantCulture));
            return builder.ToString();
        }
    }

    public sealed class SignedMatchTicket
    {
        public MatchTicket Ticket { get; }
        public string Signature { get; }

        public SignedMatchTicket(MatchTicket ticket, string signature)
        {
            Ticket = ticket ?? throw new ArgumentNullException(nameof(ticket));
            if (string.IsNullOrWhiteSpace(signature)) throw new ArgumentException("Signature cannot be empty.", nameof(signature));
            Signature = signature.Trim().ToLowerInvariant();
        }
    }
}
