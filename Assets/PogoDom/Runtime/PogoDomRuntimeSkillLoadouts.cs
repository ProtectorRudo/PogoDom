using System;
using System.Collections.Generic;
using PogoDom.Cosmetics;
using PogoDom.Core;
using UnityEngine;

namespace PogoDom.Runtime
{
    /// <summary>
    /// Runtime bridge between inventory-style loadouts and battle presentation.
    /// The prototype seeds four deterministic visual kits so every bot/player can
    /// demonstrate the pipeline before account inventory is connected.
    /// Production code can replace any player loadout without changing Core.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PogoDomRuntimeSkillLoadouts : MonoBehaviour
    {
        private readonly Dictionary<int, CosmeticLoadout> _loadouts = new Dictionary<int, CosmeticLoadout>();
        private CosmeticCatalog _catalog;
        private bool _initialized;

        private static readonly SkillVisualTrigger[] PrototypeTriggers =
        {
            SkillVisualTrigger.Bank,
            SkillVisualTrigger.CrateOpen,
            SkillVisualTrigger.Arrow,
            SkillVisualTrigger.Speed,
            SkillVisualTrigger.Missile,
            SkillVisualTrigger.Padlock,
            SkillVisualTrigger.AreaCapture,
            SkillVisualTrigger.ShieldBlock,
            SkillVisualTrigger.HazardBlast,
            SkillVisualTrigger.Stun
        };

        public CosmeticCatalog Catalog
        {
            get
            {
                EnsureInitialized();
                return _catalog;
            }
        }

        public SkillVisualDefinition Resolve(int playerId, MatchEvent e)
        {
            EnsureInitialized();
            CosmeticLoadout loadout;
            if (!_loadouts.TryGetValue(playerId, out loadout)) return null;
            return SkillVisualResolver.Resolve(_catalog, loadout, e);
        }

        public void SetLoadout(int playerId, CosmeticLoadout loadout)
        {
            if (loadout == null) throw new ArgumentNullException(nameof(loadout));
            EnsureInitialized();
            _loadouts[playerId] = loadout;
        }

        public CosmeticLoadout GetLoadout(int playerId)
        {
            EnsureInitialized();
            CosmeticLoadout loadout;
            return _loadouts.TryGetValue(playerId, out loadout) ? loadout : null;
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (_initialized) return;
            _initialized = true;
            _catalog = new CosmeticCatalog();

            for (var playerId = 0; playerId < 4; playerId++)
            {
                var loadout = new CosmeticLoadout(
                    new CosmeticId("prototype-character-" + playerId),
                    new CosmeticId("prototype-pogo-" + playerId));

                var rarity = playerId == 2 ? CosmeticRarity.Legendary :
                             playerId == 0 ? CosmeticRarity.Epic :
                             playerId == 3 ? CosmeticRarity.Epic : CosmeticRarity.Rare;

                for (var i = 0; i < PrototypeTriggers.Length; i++)
                {
                    var trigger = PrototypeTriggers[i];
                    var id = new CosmeticId("prototype-p" + playerId + "-" + trigger.ToString().ToLowerInvariant());
                    var definition = new SkillVisualDefinition(
                        id,
                        "Prototype P" + (playerId + 1) + " " + trigger,
                        rarity,
                        "prototype/skills/p" + playerId + "/" + trigger.ToString().ToLowerInvariant(),
                        trigger);
                    _catalog.Add(definition);
                    loadout.SkillVisualIds.Add(id);
                }
                _loadouts[playerId] = loadout;
            }
        }
    }
}
