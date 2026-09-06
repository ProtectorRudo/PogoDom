using System;
using System.Collections.Generic;

namespace PogoDom.Cosmetics
{
    public sealed class CosmeticInventory
    {
        private readonly HashSet<CosmeticId> _owned = new HashSet<CosmeticId>();

        public bool Owns(CosmeticId id) => _owned.Contains(id);

        public bool Unlock(CosmeticId id)
        {
            return _owned.Add(id);
        }
    }

    public sealed class CosmeticLoadout
    {
        public CosmeticId CharacterId { get; internal set; }
        public CosmeticId PogoId { get; internal set; }
        public CosmeticId TrailId { get; internal set; }
        public CosmeticId LandingFxId { get; internal set; }
        public CosmeticId VictoryEmoteId { get; internal set; }
        public List<CosmeticId> SkillVisualIds { get; } = new List<CosmeticId>();

        public CosmeticLoadout(CosmeticId characterId, CosmeticId pogoId)
        {
            CharacterId = characterId;
            PogoId = pogoId;
        }
    }

    public sealed class CosmeticEquipService
    {
        private readonly CosmeticCatalog _catalog;
        private readonly CosmeticInventory _inventory;

        public CosmeticEquipService(CosmeticCatalog catalog, CosmeticInventory inventory)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
        }

        public void EquipCharacter(CosmeticLoadout loadout, CosmeticId characterId)
        {
            RequireOwned(characterId);
            var character = _catalog.Get<CharacterDefinition>(characterId);
            loadout.CharacterId = characterId;

            var currentPogo = _catalog.Get<PogoDefinition>(loadout.PogoId);
            if (!currentPogo.Supports(character.RigFamily))
            {
                RequireOwned(character.DefaultPogoId);
                loadout.PogoId = character.DefaultPogoId;
            }
        }

        public void EquipPogo(CosmeticLoadout loadout, CosmeticId pogoId)
        {
            RequireOwned(pogoId);
            var character = _catalog.Get<CharacterDefinition>(loadout.CharacterId);
            var pogo = _catalog.Get<PogoDefinition>(pogoId);
            if (!pogo.Supports(character.RigFamily))
                throw new InvalidOperationException("Pogo is incompatible with the equipped character rig.");
            loadout.PogoId = pogoId;
        }

        public void EquipSimple(CosmeticLoadout loadout, CosmeticId id)
        {
            RequireOwned(id);
            var item = _catalog.Get(id);
            switch (item.Kind)
            {
                case CosmeticKind.Trail: loadout.TrailId = id; break;
                case CosmeticKind.LandingFx: loadout.LandingFxId = id; break;
                case CosmeticKind.VictoryEmote: loadout.VictoryEmoteId = id; break;
                default: throw new InvalidOperationException("Use the dedicated equip path for " + item.Kind + ".");
            }
        }

        public void EquipSkillVisual(CosmeticLoadout loadout, CosmeticId id)
        {
            RequireOwned(id);
            _catalog.Get<SkillVisualDefinition>(id);
            if (!loadout.SkillVisualIds.Contains(id)) loadout.SkillVisualIds.Add(id);
        }

        private void RequireOwned(CosmeticId id)
        {
            if (!_inventory.Owns(id)) throw new InvalidOperationException("Cosmetic is not owned: " + id);
        }
    }
}
