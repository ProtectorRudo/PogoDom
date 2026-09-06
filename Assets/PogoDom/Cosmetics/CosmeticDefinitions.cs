using System;
using System.Collections.Generic;
using PogoDom.Core;

namespace PogoDom.Cosmetics
{
    public interface ICosmeticDefinition
    {
        CosmeticId Id { get; }
        string DisplayName { get; }
        CosmeticKind Kind { get; }
        CosmeticRarity Rarity { get; }
        string AssetKey { get; }
    }

    public sealed class CharacterDefinition : ICosmeticDefinition
    {
        public CosmeticId Id { get; }
        public string DisplayName { get; }
        public CosmeticKind Kind => CosmeticKind.Character;
        public CosmeticRarity Rarity { get; }
        public string AssetKey { get; }
        public string RigFamily { get; }
        public AvatarRigCapability Capabilities { get; }
        public CosmeticId DefaultPogoId { get; }
        public IReadOnlyList<CosmeticId> SignatureSkillVisualIds { get; }

        public CharacterDefinition(
            CosmeticId id,
            string displayName,
            CosmeticRarity rarity,
            string assetKey,
            string rigFamily,
            CosmeticId defaultPogoId,
            IReadOnlyList<CosmeticId> signatureSkillVisualIds = null,
            AvatarRigCapability capabilities = AvatarRigCapabilities.StandardHumanoid)
        {
            Id = id;
            DisplayName = Required(displayName, nameof(displayName));
            AssetKey = Required(assetKey, nameof(assetKey));
            RigFamily = Required(rigFamily, nameof(rigFamily)).ToLowerInvariant();
            Rarity = rarity;
            Capabilities = capabilities;
            DefaultPogoId = defaultPogoId;
            SignatureSkillVisualIds = signatureSkillVisualIds ?? Array.Empty<CosmeticId>();
        }

        public bool Supports(AvatarRigCapability required)
        {
            return AvatarRigCapabilities.Supports(Capabilities, required);
        }

        private static string Required(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(name + " cannot be empty.", name);
            return value.Trim();
        }
    }

    public sealed class PogoDefinition : ICosmeticDefinition
    {
        public CosmeticId Id { get; }
        public string DisplayName { get; }
        public CosmeticKind Kind => CosmeticKind.Pogo;
        public CosmeticRarity Rarity { get; }
        public string AssetKey { get; }
        public string RigFamily { get; }

        public PogoDefinition(CosmeticId id, string displayName, CosmeticRarity rarity, string assetKey, string rigFamily)
        {
            Id = id;
            DisplayName = Required(displayName);
            AssetKey = Required(assetKey);
            RigFamily = Required(rigFamily).ToLowerInvariant();
        }

        public bool Supports(string rigFamily) => RigFamily == "*" || RigFamily == (rigFamily ?? string.Empty).Trim().ToLowerInvariant();

        private static string Required(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Value cannot be empty.");
            return value.Trim();
        }
    }

    public sealed class SimpleCosmeticDefinition : ICosmeticDefinition
    {
        public CosmeticId Id { get; }
        public string DisplayName { get; }
        public CosmeticKind Kind { get; }
        public CosmeticRarity Rarity { get; }
        public string AssetKey { get; }
        public AvatarRigCapability RequiredCapabilities { get; }

        public SimpleCosmeticDefinition(
            CosmeticId id,
            string displayName,
            CosmeticKind kind,
            CosmeticRarity rarity,
            string assetKey,
            AvatarRigCapability requiredCapabilities = AvatarRigCapability.None)
        {
            if (kind == CosmeticKind.Character || kind == CosmeticKind.Pogo || kind == CosmeticKind.SkillVisual)
                throw new ArgumentException("Use the specialized definition for this kind.", nameof(kind));
            if (string.IsNullOrWhiteSpace(displayName) || string.IsNullOrWhiteSpace(assetKey))
                throw new ArgumentException("Display name and asset key are required.");
            Id = id;
            DisplayName = displayName.Trim();
            Kind = kind;
            Rarity = rarity;
            AssetKey = assetKey.Trim();
            RequiredCapabilities = requiredCapabilities;
        }

        public bool Supports(CharacterDefinition character)
        {
            return character != null && character.Supports(RequiredCapabilities);
        }
    }

    public sealed class SkillVisualDefinition : ICosmeticDefinition
    {
        public CosmeticId Id { get; }
        public string DisplayName { get; }
        public CosmeticKind Kind => CosmeticKind.SkillVisual;
        public CosmeticRarity Rarity { get; }
        public string AssetKey { get; }
        public SkillVisualTrigger Trigger { get; }
        public PowerUpKind PowerContext { get; }
        public AvatarRigCapability RequiredCapabilities { get; }

        public SkillVisualDefinition(
            CosmeticId id,
            string displayName,
            CosmeticRarity rarity,
            string assetKey,
            SkillVisualTrigger trigger,
            PowerUpKind powerContext = PowerUpKind.None,
            AvatarRigCapability requiredCapabilities = AvatarRigCapability.None)
        {
            if (string.IsNullOrWhiteSpace(displayName) || string.IsNullOrWhiteSpace(assetKey))
                throw new ArgumentException("Display name and asset key are required.");
            Id = id;
            DisplayName = displayName.Trim();
            Rarity = rarity;
            AssetKey = assetKey.Trim();
            Trigger = trigger;
            PowerContext = powerContext;
            RequiredCapabilities = requiredCapabilities;
        }

        public bool Supports(CharacterDefinition character)
        {
            return character != null && character.Supports(RequiredCapabilities);
        }
    }
}
