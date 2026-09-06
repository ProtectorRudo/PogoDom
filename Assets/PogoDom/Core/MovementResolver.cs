using System;
using System.Collections.Generic;

namespace PogoDom.Core
{
    public static class MovementResolver
    {
        public static Dictionary<int, GridPos> Resolve(
            BoardState board,
            IReadOnlyList<PlayerState> players,
            IReadOnlyDictionary<int, Direction> directions,
            IRandomSource random)
        {
            var current = new Dictionary<int, GridPos>(players.Count);
            var candidate = new Dictionary<int, GridPos>(players.Count);

            for (var i = 0; i < players.Count; i++)
            {
                var player = players[i];
                current[player.Id] = player.Position;
                var direction = directions.TryGetValue(player.Id, out var d) ? d : player.CurrentDirection;
                candidate[player.Id] = board.Step(player.Position, direction);
            }

            // Direct swaps are blocked for both players.
            for (var i = 0; i < players.Count; i++)
            {
                for (var j = i + 1; j < players.Count; j++)
                {
                    var a = players[i].Id;
                    var b = players[j].Id;
                    if (candidate[a] == current[b] && candidate[b] == current[a] && current[a] != current[b])
                    {
                        candidate[a] = current[a];
                        candidate[b] = current[b];
                    }
                }
            }

            // Stabilize. A player that loses a collision becomes stationary and can
            // consequently block another player that wanted its original square.
            var maxPasses = Math.Max(4, players.Count * players.Count);
            for (var pass = 0; pass < maxPasses; pass++)
            {
                var changed = false;
                var groups = BuildGroups(players, candidate);

                foreach (var pair in groups)
                {
                    var ids = pair.Value;
                    if (ids.Count <= 1)
                        continue;

                    var stationary = new List<int>();
                    for (var i = 0; i < ids.Count; i++)
                    {
                        var id = ids[i];
                        if (candidate[id] == current[id])
                            stationary.Add(id);
                    }

                    if (stationary.Count > 0)
                    {
                        for (var i = 0; i < ids.Count; i++)
                        {
                            var id = ids[i];
                            if (candidate[id] != current[id])
                            {
                                candidate[id] = current[id];
                                changed = true;
                            }
                        }
                        continue;
                    }

                    // Same free target: exactly one moving player wins.
                    var winner = ids[random.NextInt(0, ids.Count)];
                    for (var i = 0; i < ids.Count; i++)
                    {
                        var id = ids[i];
                        if (id == winner)
                            continue;
                        if (candidate[id] != current[id])
                        {
                            candidate[id] = current[id];
                            changed = true;
                        }
                    }
                }

                // New swaps can appear after resetting a loser in a collision chain.
                for (var i = 0; i < players.Count; i++)
                {
                    for (var j = i + 1; j < players.Count; j++)
                    {
                        var a = players[i].Id;
                        var b = players[j].Id;
                        if (candidate[a] == current[b] && candidate[b] == current[a] && current[a] != current[b])
                        {
                            if (candidate[a] != current[a]) { candidate[a] = current[a]; changed = true; }
                            if (candidate[b] != current[b]) { candidate[b] = current[b]; changed = true; }
                        }
                    }
                }

                if (!changed)
                    break;
            }

            EnsureUnique(players, candidate);
            return candidate;
        }

        private static Dictionary<GridPos, List<int>> BuildGroups(IReadOnlyList<PlayerState> players, Dictionary<int, GridPos> positions)
        {
            var groups = new Dictionary<GridPos, List<int>>();
            for (var i = 0; i < players.Count; i++)
            {
                var id = players[i].Id;
                var pos = positions[id];
                if (!groups.TryGetValue(pos, out var list))
                {
                    list = new List<int>();
                    groups[pos] = list;
                }
                list.Add(id);
            }
            return groups;
        }

        private static void EnsureUnique(IReadOnlyList<PlayerState> players, Dictionary<int, GridPos> positions)
        {
            var seen = new HashSet<GridPos>();
            for (var i = 0; i < players.Count; i++)
            {
                var pos = positions[players[i].Id];
                if (!seen.Add(pos))
                    throw new InvalidOperationException($"Collision resolver produced duplicate final position {pos}.");
            }
        }
    }
}
