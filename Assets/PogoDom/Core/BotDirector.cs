namespace PogoDom.Core
{
    public sealed class BotDirector
    {
        private readonly EasyBotBrain _easy = new EasyBotBrain();
        private readonly MediumBotBrain _medium = new MediumBotBrain();

        public Direction ChooseDirection(MatchState state, PlayerState bot, MatchConfig config, IRandomSource random)
        {
            switch (bot.BotDifficulty)
            {
                case BotDifficulty.Easy:
                    return _easy.ChooseDirection(state, bot, config, random);
                case BotDifficulty.Hard:
                    // Hard intentionally falls back to Medium until the trained/local policy is integrated.
                    // We never call a network service from gameplay.
                    return _medium.ChooseDirection(state, bot, config, random);
                default:
                    return _medium.ChooseDirection(state, bot, config, random);
            }
        }
    }
}
