using System;

namespace PogoDom.Cosmetics
{
    public readonly struct CosmeticId : IEquatable<CosmeticId>
    {
        public string Value { get; }

        public CosmeticId(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Cosmetic id cannot be empty.", nameof(value));
            Value = value.Trim().ToLowerInvariant();
        }

        public bool Equals(CosmeticId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is CosmeticId other && Equals(other);
        public override int GetHashCode() => Value == null ? 0 : Value.GetHashCode();
        public override string ToString() => Value ?? string.Empty;
        public static bool operator ==(CosmeticId left, CosmeticId right) => left.Equals(right);
        public static bool operator !=(CosmeticId left, CosmeticId right) => !left.Equals(right);
    }

    public enum CosmeticRarity
    {
        Common = 0,
        Rare = 1,
        Epic = 2,
        Legendary = 3,
        Mythic = 4
    }

    public enum CosmeticKind
    {
        Character = 0,
        Pogo = 1,
        Trail = 2,
        LandingFx = 3,
        VictoryEmote = 4,
        SkillVisual = 5
    }

    public enum SkillVisualTrigger
    {
        Jump = 0,
        Landing = 1,
        TileSteal = 2,
        Bank = 3,
        Arrow = 4,
        Speed = 5,
        Missile = 6,
        Victory = 7,
        Defeat = 8
    }

    public enum AcquisitionKind
    {
        Starter = 0,
        Coins = 1,
        Gems = 2,
        Event = 3,
        Achievement = 4,
        Founder = 5
    }
}
