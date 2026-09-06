using System.Collections.Generic;

namespace PogoDom.Core
{
    public sealed class MatchStanding
    {
        public int PlayerId { get; }
        public int Score { get; }
        public int UnbankedTiles { get; }

        public MatchStanding(int playerId, int score, int unbankedTiles)
        {
            PlayerId = playerId;
            Score = score;
            UnbankedTiles = unbankedTiles;
        }
    }

    public static class MatchOutcome
    {
        public static PlayerState Leader(MatchState state)
        {
            PlayerState best = null;
            for (var i = 0; i < state.Players.Count; i++)
            {
                var player = state.Players[i];
                if (best == null || Compare(state, player, best) < 0)
                    best = player;
            }
            return best;
        }

        public static List<MatchStanding> Standings(MatchState state)
        {
            var list = new List<MatchStanding>();
            for (var i = 0; i < state.Players.Count; i++)
            {
                var player = state.Players[i];
                list.Add(new MatchStanding(player.Id, player.Score, state.Board.CountOwnedBy(player.Id)));
            }

            list.Sort((a, b) =>
            {
                if (a.Score != b.Score) return b.Score.CompareTo(a.Score);
                if (a.UnbankedTiles != b.UnbankedTiles) return b.UnbankedTiles.CompareTo(a.UnbankedTiles);
                return a.PlayerId.CompareTo(b.PlayerId);
            });
            return list;
        }

        private static int Compare(MatchState state, PlayerState a, PlayerState b)
        {
            if (a.Score != b.Score) return b.Score.CompareTo(a.Score);
            var aTiles = state.Board.CountOwnedBy(a.Id);
            var bTiles = state.Board.CountOwnedBy(b.Id);
            if (aTiles != bTiles) return bTiles.CompareTo(aTiles);
            return a.Id.CompareTo(b.Id);
        }
    }
}
