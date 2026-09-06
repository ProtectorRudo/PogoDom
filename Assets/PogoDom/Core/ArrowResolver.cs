namespace PogoDom.Core
{
    public static class ArrowResolver
    {
        public static int Apply(MatchState state, PlayerState player, Direction direction)
        {
            var painted = 0;
            var pos = player.Position;

            while (state.Board.Contains(pos))
            {
                if (state.Board.OwnerAt(pos) != player.Id)
                    painted++;
                state.Board.SetOwner(pos, player.Id);

                var next = state.Board.Step(pos, direction);
                if (next == pos)
                    break;
                pos = next;
            }

            return painted;
        }

        public static Direction RotateClockwise(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: return Direction.Right;
                case Direction.Right: return Direction.Down;
                case Direction.Down: return Direction.Left;
                case Direction.Left: return Direction.Up;
                default: return Direction.Up;
            }
        }
    }
}
