using System.Collections.Generic;
using PogoDom.Core;

namespace PogoDom.Cosmetics
{
    public static class SkillVisualResolver
    {
        public static SkillVisualDefinition Resolve(CosmeticCatalog catalog, CosmeticLoadout loadout, MatchEvent e)
        {
            var trigger = FromEvent(e);
            if (!trigger.HasValue) return null;

            for (var i = 0; i < loadout.SkillVisualIds.Count; i++)
            {
                var visual = catalog.Get<SkillVisualDefinition>(loadout.SkillVisualIds[i]);
                if (visual.Trigger != trigger.Value) continue;
                if (visual.PowerContext != PowerUpKind.None && visual.PowerContext != e.ItemKind) continue;
                return visual;
            }
            return null;
        }

        private static SkillVisualTrigger? FromEvent(MatchEvent e)
        {
            switch (e.Type)
            {
                case MatchEventType.TileStolen: return SkillVisualTrigger.TileSteal;
                case MatchEventType.Banked: return SkillVisualTrigger.Bank;
                case MatchEventType.ArrowUsed: return SkillVisualTrigger.Arrow;
                case MatchEventType.SpeedActivated: return SkillVisualTrigger.Speed;
                case MatchEventType.MissileFired: return SkillVisualTrigger.Missile;
                default: return null;
            }
        }
    }
}
