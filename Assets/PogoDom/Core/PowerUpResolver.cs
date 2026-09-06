using System;
using System.Collections.Generic;

namespace PogoDom.Core
{
    public static class PowerUpResolver
    {
        public static bool ApplyItemUnderPlayer(MatchState state, PlayerState player, MatchConfig config, List<MatchEvent> events)
        {
            var item = state.ItemAt(player.Position);
            if (item == null) return false;

            var effectivePower = item.Kind;
            if (item.IsMysteryCrate)
            {
                effectivePower = item.ContainedPower;
                if (!MysteryCrateTable.IsValidPayload(effectivePower))
                    throw new InvalidOperationException("Mystery crate contains an invalid payload.");

                // Presentation can reveal the payload only at the instant the crate opens.
                events.Add(new MatchEvent(
                    MatchEventType.CrateOpened,
                    player.Id,
                    player.Position,
                    0,
                    effectivePower));
            }

            ApplyPower(state, player, config, events, effectivePower, item.ArrowDirection);

            state.Items.Remove(item);
            events.Add(new MatchEvent(MatchEventType.ItemConsumed, player.Id, player.Position, 0, item.Kind));
            return true;
        }

        private static void ApplyPower(
            MatchState state,
            PlayerState player,
            MatchConfig config,
            List<MatchEvent> events,
            PowerUpKind power,
            Direction arrowDirection)
        {
            switch (power)
            {
                case PowerUpKind.BankCrate:
                {
                    var banked = BankingResolver.Bank(state, player);
                    events.Add(new MatchEvent(MatchEventType.Banked, player.Id, player.Position, banked, power));
                    break;
                }
                case PowerUpKind.Arrow:
                {
                    var changed = ArrowResolver.Apply(state, player, arrowDirection);
                    events.Add(new MatchEvent(MatchEventType.ArrowUsed, player.Id, player.Position, changed, power));
                    break;
                }
                case PowerUpKind.Speed:
                {
                    player.SpeedTicksRemaining = Math.Max(player.SpeedTicksRemaining, config.SpeedDurationTicks);
                    events.Add(new MatchEvent(MatchEventType.SpeedActivated, player.Id, player.Position, config.SpeedDurationTicks, power));
                    break;
                }
                case PowerUpKind.Missile:
                {
                    var target = SelectMissileTarget(state, player);
                    if (target != null)
                    {
                        target.StunTicksRemaining = Math.Max(target.StunTicksRemaining, config.MissileStunTicks);
                        events.Add(new MatchEvent(MatchEventType.MissileFired, player.Id, player.Position, config.MissileStunTicks, power, target.Id));
                        events.Add(new MatchEvent(MatchEventType.PlayerStunned, target.Id, target.Position, config.MissileStunTicks, power, player.Id));
                    }
                    break;
                }
                case PowerUpKind.Padlock:
                {
                    player.PadlockTicksRemaining = Math.Max(player.PadlockTicksRemaining, config.PadlockDurationTicks);
                    events.Add(new MatchEvent(MatchEventType.PadlockActivated, player.Id, player.Position, config.PadlockDurationTicks, power));
                    break;
                }
                default:
                    throw new InvalidOperationException("Unsupported pickup power: " + power);
            }
        }

        public static PlayerState SelectMissileTarget(MatchState state, PlayerState attacker)
        {
            PlayerState best = null;
            for (var i = 0; i < state.Players.Count; i++)
            {
                var candidate = state.Players[i];
                if (candidate.Id == attacker.Id) continue;
                if (best == null || IsBetterTarget(state, attacker, candidate, best)) best = candidate;
            }
            return best;
        }

        private static bool IsBetterTarget(MatchState state, PlayerState attacker, PlayerState candidate, PlayerState currentBest)
        {
            if (candidate.Score != currentBest.Score) return candidate.Score > currentBest.Score;
            var candidateTiles = state.Board.CountOwnedBy(candidate.Id);
            var bestTiles = state.Board.CountOwnedBy(currentBest.Id);
            if (candidateTiles != bestTiles) return candidateTiles > bestTiles;
            var candidateDistance = Manhattan(attacker.Position, candidate.Position);
            var bestDistance = Manhattan(attacker.Position, currentBest.Position);
            if (candidateDistance != bestDistance) return candidateDistance < bestDistance;
            return candidate.Id < currentBest.Id;
        }

        private static int Manhattan(GridPos a, GridPos b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }
    }
}
