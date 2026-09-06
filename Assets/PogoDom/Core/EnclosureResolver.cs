using System;
using System.Collections.Generic;

namespace PogoDom.Core
{
    public sealed class EnclosureCaptureResult
    {
        public int PlayerId { get; }
        public List<GridPos> CapturedTiles { get; }
        public int StolenTiles { get; internal set; }
        public int Count => CapturedTiles.Count;

        public EnclosureCaptureResult(int playerId)
        {
            PlayerId = playerId;
            CapturedTiles = new List<GridPos>();
        }
    }

    public static class EnclosureResolver
    {
        private static readonly int[] Dx = { -1, 1, 0, 0 };
        private static readonly int[] Dy = { 0, 0, -1, 1 };

        public static List<GridPos> FindEnclosed(BoardState board, int playerId)
        {
            if (board == null) throw new ArgumentNullException(nameof(board));
            if (playerId < 0) throw new ArgumentOutOfRangeException(nameof(playerId));

            var exterior = new bool[board.Width, board.Height];
            var queue = new Queue<GridPos>();
            for (var x = 0; x < board.Width; x++)
            {
                SeedExterior(board, playerId, new GridPos(x, 0), exterior, queue);
                if (board.Height > 1) SeedExterior(board, playerId, new GridPos(x, board.Height - 1), exterior, queue);
            }
            for (var y = 1; y < board.Height - 1; y++)
            {
                SeedExterior(board, playerId, new GridPos(0, y), exterior, queue);
                if (board.Width > 1) SeedExterior(board, playerId, new GridPos(board.Width - 1, y), exterior, queue);
            }

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                for (var i = 0; i < 4; i++)
                {
                    var next = new GridPos(current.X + Dx[i], current.Y + Dy[i]);
                    if (!board.Contains(next) || exterior[next.X, next.Y] || board.OwnerAt(next) == playerId) continue;
                    exterior[next.X, next.Y] = true;
                    queue.Enqueue(next);
                }
            }

            var enclosed = new List<GridPos>();
            for (var y = 0; y < board.Height; y++)
                for (var x = 0; x < board.Width; x++)
                {
                    var pos = new GridPos(x, y);
                    if (board.OwnerAt(pos) != playerId && !exterior[x, y]) enclosed.Add(pos);
                }
            return enclosed;
        }

        public static List<EnclosureCaptureResult> CaptureSimultaneous(BoardState board, IReadOnlyList<PlayerState> players)
        {
            if (board == null) throw new ArgumentNullException(nameof(board));
            if (players == null) throw new ArgumentNullException(nameof(players));

            var claims = new Dictionary<GridPos, List<int>>();
            var results = new Dictionary<int, EnclosureCaptureResult>();
            for (var i = 0; i < players.Count; i++)
            {
                var playerId = players[i].Id;
                results[playerId] = new EnclosureCaptureResult(playerId);
                var enclosed = FindEnclosed(board, playerId);
                for (var t = 0; t < enclosed.Count; t++)
                {
                    var pos = enclosed[t];
                    List<int> claimants;
                    if (!claims.TryGetValue(pos, out claimants))
                    {
                        claimants = new List<int>();
                        claims.Add(pos, claimants);
                    }
                    claimants.Add(playerId);
                }
            }

            foreach (var pair in claims)
            {
                if (pair.Value.Count != 1) continue;
                var playerId = pair.Value[0];
                var pos = pair.Key;
                var previousOwner = board.OwnerAt(pos);
                if (previousOwner == playerId) continue;
                if (previousOwner >= 0 && IsShielded(players, previousOwner)) continue;

                board.SetOwner(pos, playerId);
                var result = results[playerId];
                result.CapturedTiles.Add(pos);
                if (previousOwner >= 0 && previousOwner != playerId) result.StolenTiles++;
            }

            var ordered = new List<EnclosureCaptureResult>();
            for (var i = 0; i < players.Count; i++) ordered.Add(results[players[i].Id]);
            return ordered;
        }

        public static int PreviewCaptureCount(BoardState board, int playerId, GridPos candidate)
        {
            if (board == null) throw new ArgumentNullException(nameof(board));
            if (!board.Contains(candidate)) return 0;
            var previousOwner = board.OwnerAt(candidate);
            if (previousOwner == playerId) return 0;
            board.SetOwner(candidate, playerId);
            try { return FindEnclosed(board, playerId).Count; }
            finally { board.SetOwner(candidate, previousOwner); }
        }

        private static bool IsShielded(IReadOnlyList<PlayerState> players, int ownerId)
        {
            for (var i = 0; i < players.Count; i++)
                if (players[i].Id == ownerId) return players[i].HasPadlock;
            return false;
        }

        private static void SeedExterior(BoardState board, int playerId, GridPos pos, bool[,] exterior, Queue<GridPos> queue)
        {
            if (exterior[pos.X, pos.Y] || board.OwnerAt(pos) == playerId) return;
            exterior[pos.X, pos.Y] = true;
            queue.Enqueue(pos);
        }
    }
}
