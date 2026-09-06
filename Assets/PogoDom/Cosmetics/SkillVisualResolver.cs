using PogoDom.Core;

namespace PogoDom.Cosmetics
{
    public static class SkillVisualResolver
    {
        public static SkillVisualDefinition Resolve(CosmeticCatalog catalog, CosmeticLoadout loadout, MatchEvent e)
        {
            var trigger = VisualEventPolicy.TriggerFor(e);
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
    }
}
