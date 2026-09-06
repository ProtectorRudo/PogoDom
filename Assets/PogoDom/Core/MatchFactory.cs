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
                new PlayerState(0, "YOU", true,  new GridPos(0, 0),       Direction.Up),
                new PlayerState(1, "BOT A", false, new GridPos(0, maxY), Direction.Right),
                new PlayerState(2, "BOT B", false, new GridPos(maxX, maxY), Direction.Down),
                new PlayerState(3, "BOT C", false, new GridPos(maxX, 0), Direction.Left)
            };

            return new MatchState(board, players, config.MatchSeconds);
        }
    }
}
