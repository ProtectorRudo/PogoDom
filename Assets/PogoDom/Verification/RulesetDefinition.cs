using System;
using System.Collections.Generic;
using PogoDom.Core;

namespace PogoDom.Verification
{
    public sealed class RulesetDefinition
    {
        public string Id { get; }
        public int BoardWidth { get; }
        public int BoardHeight { get; }
        public float TickSeconds { get; }
        public float MatchSeconds { get; }
        public int TargetBankCrates { get; }
        public int TargetArrows { get; }
        public int TargetSpeedPickups { get; }
        public int TargetMissiles { get; }
        public int MinimumBankCrateChebyshevDistance { get; }
        public int ArrowRotationIntervalTicks { get; }
        public int BankThresholdForBots { get; }
        public int BankRespawnDelayTicks { get; }
        public int ArrowRespawnDelayTicks { get; }
        public int SpeedRespawnDelayTicks { get; }
        public int MissileRespawnDelayTicks { get; }
        public int SpeedDurationTicks { get; }
        public int MissileStunTicks { get; }

        public RulesetDefinition(string id, MatchConfig config)
        {
            Id = TicketToken.Normalize(id, nameof(id));
            if (config == null) throw new ArgumentNullException(nameof(config));
            BoardWidth = config.BoardWidth;
            BoardHeight = config.BoardHeight;
            TickSeconds = config.TickSeconds;
            MatchSeconds = config.MatchSeconds;
            TargetBankCrates = config.TargetBankCrates;
            TargetArrows = config.TargetArrows;
            TargetSpeedPickups = config.TargetSpeedPickups;
            TargetMissiles = config.TargetMissiles;
            MinimumBankCrateChebyshevDistance = config.MinimumBankCrateChebyshevDistance;
            ArrowRotationIntervalTicks = config.ArrowRotationIntervalTicks;
            BankThresholdForBots = config.BankThresholdForBots;
            BankRespawnDelayTicks = config.BankRespawnDelayTicks;
            ArrowRespawnDelayTicks = config.ArrowRespawnDelayTicks;
            SpeedRespawnDelayTicks = config.SpeedRespawnDelayTicks;
            MissileRespawnDelayTicks = config.MissileRespawnDelayTicks;
            SpeedDurationTicks = config.SpeedDurationTicks;
            MissileStunTicks = config.MissileStunTicks;
        }

        public MatchConfig CreateConfig()
        {
            return new MatchConfig
            {
                BoardWidth = BoardWidth,
                BoardHeight = BoardHeight,
                TickSeconds = TickSeconds,
                MatchSeconds = MatchSeconds,
                TargetBankCrates = TargetBankCrates,
                TargetArrows = TargetArrows,
                TargetSpeedPickups = TargetSpeedPickups,
                TargetMissiles = TargetMissiles,
                MinimumBankCrateChebyshevDistance = MinimumBankCrateChebyshevDistance,
                ArrowRotationIntervalTicks = ArrowRotationIntervalTicks,
                BankThresholdForBots = BankThresholdForBots,
                BankRespawnDelayTicks = BankRespawnDelayTicks,
                ArrowRespawnDelayTicks = ArrowRespawnDelayTicks,
                SpeedRespawnDelayTicks = SpeedRespawnDelayTicks,
                MissileRespawnDelayTicks = MissileRespawnDelayTicks,
                SpeedDurationTicks = SpeedDurationTicks,
                MissileStunTicks = MissileStunTicks
            };
        }
    }

    public sealed class RulesetRegistry
    {
        private readonly Dictionary<string, RulesetDefinition> _rulesets = new Dictionary<string, RulesetDefinition>(StringComparer.Ordinal);

        public void Add(RulesetDefinition ruleset)
        {
            if (ruleset == null) throw new ArgumentNullException(nameof(ruleset));
            _rulesets.Add(ruleset.Id, ruleset);
        }

        public RulesetDefinition Get(string id)
        {
            RulesetDefinition value;
            if (!_rulesets.TryGetValue(id, out value)) return null;
            return value;
        }

        public static RulesetRegistry CreateCurrent()
        {
            var registry = new RulesetRegistry();
            registry.Add(new RulesetDefinition("launch-m0-2", new MatchConfig()));
            return registry;
        }
    }
}
