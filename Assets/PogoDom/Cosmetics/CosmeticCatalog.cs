using System;
using System.Collections.Generic;

namespace PogoDom.Cosmetics
{
    public sealed class CosmeticCatalog
    {
        private readonly Dictionary<CosmeticId, ICosmeticDefinition> _items = new Dictionary<CosmeticId, ICosmeticDefinition>();

        public IReadOnlyDictionary<CosmeticId, ICosmeticDefinition> Items => _items;

        public void Add(ICosmeticDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (_items.ContainsKey(definition.Id)) throw new InvalidOperationException("Duplicate cosmetic id: " + definition.Id);
            _items.Add(definition.Id, definition);
        }

        public ICosmeticDefinition Get(CosmeticId id)
        {
            ICosmeticDefinition value;
            if (!_items.TryGetValue(id, out value)) throw new KeyNotFoundException("Unknown cosmetic: " + id);
            return value;
        }

        public T Get<T>(CosmeticId id) where T : class, ICosmeticDefinition
        {
            var value = Get(id) as T;
            if (value == null) throw new InvalidOperationException("Cosmetic " + id + " is not a " + typeof(T).Name + ".");
            return value;
        }

        public void Validate()
        {
            foreach (var pair in _items)
            {
                var character = pair.Value as CharacterDefinition;
                if (character == null) continue;

                var pogo = Get<PogoDefinition>(character.DefaultPogoId);
                if (!pogo.Supports(character.RigFamily))
                    throw new InvalidOperationException("Character default pogo is incompatible: " + character.Id);

                for (var i = 0; i < character.SignatureSkillVisualIds.Count; i++)
                    Get<SkillVisualDefinition>(character.SignatureSkillVisualIds[i]);
            }
        }
    }
}
