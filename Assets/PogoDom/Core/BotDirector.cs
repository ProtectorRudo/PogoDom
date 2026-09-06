namespace PogoDom.Core
{
    public sealed class BotDirector
    {
        private readonly EasyBotBrain _easy = new EasyBotBrain();
        private readonly MediumBotBrain _medium = new MediumBotBrain();
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
                    return _medium.ChooseDirection(state, bot, config, random);
            }
        }
    }
}
