using System.Collections.Generic;

namespace PogoDom.Core
{
    public static class MatchFactory
    {
        public static MatchState CreateClassicPrototype(MatchConfig config)
        {
            var board = new BoardState(config.BoardWidth, config.BoardHeight);
            var maxX = config.BoardWidth - 1;
            var maxY = config.BoardHeight - 1;

            var players = new List<PlayerState>
            {
                new PlayerState(0, "YOU", true,  new GridPos(0, 0), Direction.Up),
                new PlayerState(1, "BOT A", false, new GridPos(0, maxY), Direction.Right, BotDifficulty.Medium, BotPersonality.Greedy),
                new PlayerState(2, "BOT B", false, new GridPos(maxX, maxY), Direction.Down, BotDifficulty.Medium, BotPersonality.Aggressive),
                new PlayerState(3, "BOT C", false, new GridPos(maxX, 0), Direction.Left, BotDifficulty.Medium, BotPersonality.Banker)
            };

            return new MatchState(board, players, config.MatchSeconds);
        }

        public static MatchState CreateBotLab(MatchConfig config)
        {
            var board = new BoardState(config.BoardWidth, config.BoardHeight);
            var maxX = config.BoardWidth - 1;
            var maxY = config.BoardHeight - 1;

            var players = new List<PlayerState>
            {
                new PlayerState(0, "BALANCED", false, new GridPos(0, 0), Direction.Up, BotDifficulty.Medium, BotPersonality.Balanced),
                new PlayerState(1, "GREEDY", false, new GridPos(0, maxY), Direction.Right, BotDifficulty.Medium, BotPersonality.Greedy),
                new PlayerState(2, "AGGRESSIVE", false, new GridPos(maxX, maxY), Direction.Down, BotDifficulty.Medium, BotPersonality.Aggressive),
                new PlayerState(3, "BANKER", false, new GridPos(maxX, 0), Direction.Left, BotDifficulty.Medium, BotPersonality.Banker)
            };

            return new MatchState(board, players, config.MatchSeconds);
        }
    }
}
