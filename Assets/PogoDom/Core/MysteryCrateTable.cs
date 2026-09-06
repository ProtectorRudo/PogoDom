using System;

namespace PogoDom.Core
{
    public enum MysteryCrateTableId
    {
        PowerMixV1 = 0
    }

    /// <summary>
    /// Versioned mystery-crate payload tables. Payload is rolled when the crate
    /// spawns and stored in ItemState so opening the crate consumes no hidden RNG.
    /// That keeps offline/server replay deterministic even if other actions occur
    /// between spawn and pickup.
    /// </summary>
    public static class MysteryCrateTable
    {
        public static PowerUpKind Roll(MysteryCrateTableId table, IRandomSource random)
        {
            if (random == null) throw new ArgumentNullException(nameof(random));

            switch (table)
            {
                case MysteryCrateTableId.PowerMixV1:
                    return RollPowerMixV1(random.NextInt(0, 100));
                default:
                    throw new ArgumentOutOfRangeException(nameof(table), table, "Unknown mystery crate table.");
            }
        }

        public static bool IsValidPayload(PowerUpKind kind)
        {
            return kind == PowerUpKind.Arrow ||
                   kind == PowerUpKind.Speed ||
                   kind == PowerUpKind.Missile ||
                   kind == PowerUpKind.Padlock;
        }

        private static PowerUpKind RollPowerMixV1(int roll)
        {
            // Frozen v1 distribution. Create a new table id instead of changing it.
            if (roll < 35) return PowerUpKind.Arrow;    // 35%
            if (roll < 60) return PowerUpKind.Speed;    // 25%
            if (roll < 85) return PowerUpKind.Missile;  // 25%
            return PowerUpKind.Padlock;                 // 15%
        }
    }
}
