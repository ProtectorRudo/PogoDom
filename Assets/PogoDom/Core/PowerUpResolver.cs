using System;
using System.Collections.Generic;

namespace PogoDom.Core
{
    public static class PowerUpResolver
    {
        public static bool ApplyItemUnderPlayer(
            MatchState state,
            PlayerState player,
            MatchConfig config,
            List<MatchEvent> events)
        {
            var item = state.ItemAt(player.Position);
            if (item == null)
                return false;

            switch (item.Kind)
            {
                case PowerUpKind.BankCrate:
                {
                    var banked = BankingResolver.Bank(state, player);
                    events.Add(new MatchEvent(MatchEventType.Banked, player.Id, player.Position, banked, item.Kind));
                    break;
                }
                case PowerUpKind.Arrow:
                {
                    var changed = ArrowResolver.Apply(state, player, item.ArrowDirection);
                    events.Add(new MatchEvent(MatchEventType.ArrowUsed, player.Id, player.Position, changed, item.Kind));
                    break;
                }
                case PowerUpKind.Speed:
                {
                    player.SpeedTicksRemaining = Math.Max(player.SpeedTicksRemaining, config.SpeedDurationTicks);
                    events.Add(new MatchEvent(MatchEventType.SpeedActivated, player.Id, player.Position, config.SpeedDurationTicks, item.Kind));
                    break;
                }
                case PowerUpKind.Missile:
                {
                    var target = SelectMissileTarget(state, player);
                    if (target != null)
                    {
                        target.StunTicksRemaining = Math.Max(target.StunTicksRemaining, config.MissileStunTicks);
                        events.Add(new MatchEvent(MatchEventType.MissileFired, player.Id, player.Position, config.MissileStunTicks, item.Kind, target.Id));
                        events.Add(new MatchEvent(MatchEventType.PlayerStunned, target.Id, target.Position, config.MissileStunTicks, item.Kind, player.Id));
                    }
                    break;
                }
            }

            state.Items.Remove(item);
            events.Add(new MatchEvent(MatchEventType.ItemConsumed, player.Id, player.Position, 0, item.Kind));
            return true;
        }

        public static PlayerState SelectMissileTarget(MatchState state, PlayerState attacker)
        {
            PlayerState best = null;
            for (var i = 0; i < state.Players.Count; i++)
            {
                var candidate = state.Players[i];
                if (candidate.Id == attacker.Id)
                    continue;

                if (best == null || IsBetterTarget(state, attacker, candidate, best))
                    best = candidate;
            }
            return best;
        }

        private static bool IsBetterTarget(MatchState state, PlayerState attacker, PlayerState candidate, PlayerState currentBest)
        {
            if (candidate.Score != currentBest.Score)
                return candidate.Score > currentBest.Score;

            var candidateTiles = state.Board.CountOwnedBy(candidate.Id);
            var bestTiles = state.Board.CountOwnedBy(currentBest.Id);
            if (candidateTiles != bestTiles)
                return candidateTiles > bestTiles;

            var candidateDistance = Manhattan(attacker.Position, candidate.Position);
            var bestDistance = Manhattan(attacker.Position, currentBest.Position);
            if (candidateDistance != bestDistance)
                return candidateDistance < bestDistance;

            return candidate.Id < currentBest.Id;
        }

        private static int Manhattan(GridPos a, GridPos b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }
    }
}
