using System;
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

        public static MatchState CreateDifficultyLab(MatchConfig config, int hardSlot)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (hardSlot < 0 || hardSlot > 3) throw new ArgumentOutOfRangeException(nameof(hardSlot));

            var board = new BoardState(config.BoardWidth, config.BoardHeight);
            var maxX = config.BoardWidth - 1;
            var maxY = config.BoardHeight - 1;
            var starts = new[]
            {
                new GridPos(0, 0),
                new GridPos(0, maxY),
                new GridPos(maxX, maxY),
                new GridPos(maxX, 0)
            };
            var directions = new[] { Direction.Up, Direction.Right, Direction.Down, Direction.Left };
            var players = new List<PlayerState>(4);

            for (var i = 0; i < 4; i++)
            {
                var difficulty = i == hardSlot ? BotDifficulty.Hard : BotDifficulty.Medium;
                players.Add(new PlayerState(
                    i,
                    i == hardSlot ? "HARD" : "MEDIUM " + i,
                    false,
                    starts[i],
                    directions[i],
                    difficulty,
                    BotPersonality.Balanced));
            }

            return new MatchState(board, players, config.MatchSeconds);
        }
    }
}
