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
                var owner = state.Board.OwnerAt(pos);
                if (owner != player.Id && !IsProtectedByRival(state, owner, player.Id))
                {
                    painted++;
                    state.Board.SetOwner(pos, player.Id);
                }

                var next = state.Board.Step(pos, direction);
                if (next == pos) break;
                pos = next;
            }

            return painted;
        }

        private static bool IsProtectedByRival(MatchState state, int ownerId, int attackerId)
        {
            if (ownerId < 0 || ownerId == attackerId) return false;
            var owner = state.PlayerById(ownerId);
            return owner != null && owner.HasPadlock;
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
