using System;

namespace PogoDom.Meta
{
    public sealed class BattleResult
    {
        public string MatchId { get; }
        public bool IsHuman { get; }
        public bool IsValid { get; }
        public CityId CityId { get; }
        public NationId NationId { get; }
        public bool Won { get; }
        public int Placement { get; }
        public int Score { get; }
        public int TilesPainted { get; }
        public int TilesStolen { get; }
        public int BankedPoints { get; }
        public int Banks { get; }
        public int Arrows { get; }
        public int Speeds { get; }
        public int Missiles { get; }

        public BattleResult(
            string matchId,
            bool isHuman,
            bool isValid,
            CityId cityId,
            NationId nationId,
            bool won,
            int placement,
            int score,
            int tilesPainted,
            int tilesStolen,
            int bankedPoints,
            int banks,
            int arrows,
            int speeds,
            int missiles)
        {
            if (string.IsNullOrWhiteSpace(matchId)) throw new ArgumentException("Match id cannot be empty.", nameof(matchId));
            MatchId = matchId;
            IsHuman = isHuman;
            IsValid = isValid;
            CityId = cityId;
            NationId = nationId;
            Won = won;
            Placement = placement;
            Score = Math.Max(0, score);
            TilesPainted = Math.Max(0, tilesPainted);
            TilesStolen = Math.Max(0, tilesStolen);
            BankedPoints = Math.Max(0, bankedPoints);
            Banks = Math.Max(0, banks);
            Arrows = Math.Max(0, arrows);
            Speeds = Math.Max(0, speeds);
            Missiles = Math.Max(0, missiles);
        }
    }
}
