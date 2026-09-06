using System;

namespace PogoDom.Meta
{
    public readonly struct CityId : IEquatable<CityId>
    {
        public string Value { get; }

        public CityId(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("City id cannot be empty.", nameof(value));
            Value = value.Trim().ToLowerInvariant();
        }

        public bool Equals(CityId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is CityId other && Equals(other);
        public override int GetHashCode() => Value == null ? 0 : Value.GetHashCode();
        public override string ToString() => Value ?? string.Empty;
        public static bool operator ==(CityId left, CityId right) => left.Equals(right);
        public static bool operator !=(CityId left, CityId right) => !left.Equals(right);
    }

    public readonly struct NationId : IEquatable<NationId>
    {
        public string Value { get; }

        public NationId(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Nation id cannot be empty.", nameof(value));
            Value = value.Trim().ToLowerInvariant();
        }

        public bool Equals(NationId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is NationId other && Equals(other);
        public override int GetHashCode() => Value == null ? 0 : Value.GetHashCode();
        public override string ToString() => Value ?? string.Empty;
        public static bool operator ==(NationId left, NationId right) => left.Equals(right);
        public static bool operator !=(NationId left, NationId right) => !left.Equals(right);
    }
}
