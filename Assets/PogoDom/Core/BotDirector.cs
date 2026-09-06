namespace PogoDom.Core
{
    public sealed class BotDirector
    {
        private readonly EasyBotBrain _easy = new EasyBotBrain();
        private readonly MediumBotBrain _medium = new MediumBotBrain();
        private readonly RivalBotBrain _rival = new RivalBotBrain();
        private readonly HardBotBrain _hard = new HardBotBrain();

        public Direction ChooseDirection(MatchState state, PlayerState bot, MatchConfig config, IRandomSource random)
        {
            switch (bot.BotDifficulty)
            {
                case BotDifficulty.Easy:
                    return _easy.ChooseDirection(state, bot, config, random);
                case BotDifficulty.Hard:
                    return _hard.ChooseDirection(state, bot, config, random);
                default:
                    // Published V1/V2 rulesets keep the original Medium path.
                    // Adaptive rivals are an explicit V3 semantic change so
                    // historical signed replays cannot silently drift.
                    return config.BehaviorVersion >= RulesetBehaviorVersion.V3
                        ? _rival.ChooseDirection(state, bot, config, random)
                        : _medium.ChooseDirection(state, bot, config, random);
            }
        }
    }
}
