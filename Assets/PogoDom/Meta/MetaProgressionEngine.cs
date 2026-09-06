using System;

namespace PogoDom.Meta
{
    public sealed class MetaProgressionEngine
    {
        public int AttackDamagePerWin { get; }
        public int DefenseHealPerWin { get; }

        public MetaProgressionEngine(int attackDamagePerWin = 1, int defenseHealPerWin = 2)
        {
            if (attackDamagePerWin <= 0) throw new ArgumentOutOfRangeException(nameof(attackDamagePerWin));
            if (defenseHealPerWin <= 0) throw new ArgumentOutOfRangeException(nameof(defenseHealPerWin));
            AttackDamagePerWin = attackDamagePerWin;
            DefenseHealPerWin = defenseHealPerWin;
        }

        public ProgressionReceipt Apply(WorldState world, BattleResult result, CampaignContext campaign)
        {
            if (world == null) throw new ArgumentNullException(nameof(world));
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (campaign == null) throw new ArgumentNullException(nameof(campaign));

            var receipt = new ProgressionReceipt();

            if (!result.IsValid) return Reject(receipt, ProgressionRejection.InvalidMatch);
            if (!result.IsHuman) return Reject(receipt, ProgressionRejection.BotResult);
            if (world.WasProcessed(result.MatchId)) return Reject(receipt, ProgressionRejection.DuplicateMatch);

            CityState playerCity;
            NationState playerNation;
            try
            {
                playerCity = world.City(result.CityId);
                playerNation = world.Nation(result.NationId);
            }
            catch
            {
                return Reject(receipt, ProgressionRejection.UnknownIdentity);
            }

            if (playerCity.NationId != result.NationId || campaign.PlayerCity != result.CityId)
                return Reject(receipt, ProgressionRejection.IdentityMismatch);

            if (!ValidateCampaign(world, result, campaign))
                return Reject(receipt, ProgressionRejection.InvalidCampaign);

            if (!world.MarkProcessed(result.MatchId))
                return Reject(receipt, ProgressionRejection.DuplicateMatch);

            ApplyObjectives(world, result, receipt);

            if (result.Won)
            {
                playerCity.GlobalPoints++;
                playerNation.GlobalPoints++;
                receipt.CityPointDelta = 1;
                receipt.NationPointDelta = 1;

                if (campaign.Mode == CampaignMode.Attack && campaign.TargetCity.HasValue)
                    ApplyAttack(world, result.CityId, campaign.TargetCity.Value, receipt);
                else if (campaign.Mode == CampaignMode.Defense)
                    ApplyDefense(playerCity, receipt);
            }

            receipt.Applied = true;
            return receipt;
        }

        private bool ValidateCampaign(WorldState world, BattleResult result, CampaignContext campaign)
        {
            if (campaign.Mode == CampaignMode.None) return true;
            if (campaign.Mode == CampaignMode.Defense) return campaign.PlayerCity == result.CityId;
            if (campaign.Mode != CampaignMode.Attack || !campaign.TargetCity.HasValue) return false;
            if (campaign.TargetCity.Value == result.CityId) return false;

            try
            {
                world.City(campaign.TargetCity.Value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void ApplyAttack(WorldState world, CityId attacker, CityId targetId, ProgressionReceipt receipt)
        {
            var target = world.City(targetId);
            if (target.IsConquered) return;

            var before = target.CurrentHp;
            target.CurrentHp = Math.Max(0, target.CurrentHp - AttackDamagePerWin);
            receipt.TargetHpDelta = target.CurrentHp - before;

            if (target.CurrentHp == 0 && !target.ConqueredByCity.HasValue)
            {
                target.ConqueredByCity = attacker;
                receipt.ConquestTriggered = true;
            }
        }

        private void ApplyDefense(CityState city, ProgressionReceipt receipt)
        {
            if (city.IsConquered) return;
            var before = city.CurrentHp;
            city.CurrentHp = Math.Min(city.MaxHp, city.CurrentHp + DefenseHealPerWin);
            receipt.TargetHpDelta = city.CurrentHp - before;
        }

        private static void ApplyObjectives(WorldState world, BattleResult result, ProgressionReceipt receipt)
        {
            for (var i = 0; i < world.Objectives.Count; i++)
            {
                var objective = world.Objectives[i];
                if (!ScopeMatches(objective, result)) continue;

                var raw = MetricValue(objective.Metric, result);
                var applied = objective.Apply(raw);
                if (applied > 0)
                    receipt.Objectives.Add(new ObjectiveDelta(objective.Id, applied, objective.Current, objective.IsComplete));
            }
        }

        private static bool ScopeMatches(ObjectiveState objective, BattleResult result)
        {
            if (objective.Scope == ObjectiveScope.City)
                return objective.ScopeId == result.CityId.Value;
            if (objective.Scope == ObjectiveScope.Nation)
                return objective.ScopeId == result.NationId.Value;
            return false;
        }

        private static int MetricValue(ObjectiveMetric metric, BattleResult result)
        {
            switch (metric)
            {
                case ObjectiveMetric.MatchesPlayed: return 1;
                case ObjectiveMetric.Wins: return result.Won ? 1 : 0;
                case ObjectiveMetric.ScoreBanked: return result.BankedPoints;
                case ObjectiveMetric.TilesPainted: return result.TilesPainted;
                case ObjectiveMetric.TilesStolen: return result.TilesStolen;
                case ObjectiveMetric.BankCrates: return result.Banks;
                case ObjectiveMetric.ArrowsUsed: return result.Arrows;
                case ObjectiveMetric.SpeedsUsed: return result.Speeds;
                case ObjectiveMetric.MissilesUsed: return result.Missiles;
                default: return 0;
            }
        }

        private static ProgressionReceipt Reject(ProgressionReceipt receipt, ProgressionRejection reason)
        {
            receipt.Applied = false;
            receipt.Rejection = reason;
            return receipt;
        }
    }
}
