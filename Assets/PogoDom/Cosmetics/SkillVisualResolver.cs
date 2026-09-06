using PogoDom.Core;

namespace PogoDom.Cosmetics
{
    public static class SkillVisualResolver
    {
        public static SkillVisualDefinition Resolve(CosmeticCatalog catalog, CosmeticLoadout loadout, MatchEvent e)
        {
            var trigger = FromEvent(e);
            if (!trigger.HasValue) return null;

            SkillVisualDefinition generic = null;
            for (var i = 0; i < loadout.SkillVisualIds.Count; i++)
            {
                var visual = catalog.Get<SkillVisualDefinition>(loadout.SkillVisualIds[i]);
                if (visual.Trigger != trigger.Value) continue;

                // Exact gameplay context always wins over a generic visual, so a
                // player may equip one general crate/open effect plus a special
                // Missile-crate effect without loadout order changing the result.
                if (visual.PowerContext == e.ItemKind && e.ItemKind != PowerUpKind.None)
                    return visual;
                if (visual.PowerContext == PowerUpKind.None && generic == null)
                    generic = visual;
            }
            return generic;
        }

        private static SkillVisualTrigger? FromEvent(MatchEvent e)
        {
            switch (e.Type)
            {
                case MatchEventType.TileStolen: return SkillVisualTrigger.TileSteal;
                case MatchEventType.TileProtected: return SkillVisualTrigger.ShieldBlock;
                case MatchEventType.EnclosureCaptured: return SkillVisualTrigger.AreaCapture;
                case MatchEventType.Banked: return SkillVisualTrigger.Bank;
                case MatchEventType.CrateOpened: return SkillVisualTrigger.CrateOpen;
                case MatchEventType.ArrowUsed: return SkillVisualTrigger.Arrow;
                case MatchEventType.SpeedActivated: return SkillVisualTrigger.Speed;
                case MatchEventType.MissileFired: return SkillVisualTrigger.Missile;
                case MatchEventType.PadlockActivated: return SkillVisualTrigger.Padlock;
                case MatchEventType.PlayerStunned: return SkillVisualTrigger.Stun;
                case MatchEventType.HazardDetonated: return SkillVisualTrigger.HazardBlast;
                default: return null;
            }
        }
    }
}
