using System;

namespace PogoDom.Core
{
    public sealed class XorShiftRandom : IRandomSource
    {
        private uint _state;

        public XorShiftRandom(uint seed)
        {
            _state = seed == 0 ? 0xA341316Cu : seed;
        }

        public uint NextUInt()
        {
            var x = _state;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            _state = x;
            return x;
        }

        public int NextInt(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
                throw new ArgumentOutOfRangeException(nameof(maxExclusive));

            var range = (uint)(maxExclusive - minInclusive);
            return minInclusive + (int)(NextUInt() % range);
        }

        public float NextFloat01()
        {
            return (NextUInt() & 0x00FFFFFFu) / 16777216f;
        }
    }
}
