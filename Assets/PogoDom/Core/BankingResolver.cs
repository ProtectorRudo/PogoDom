namespace PogoDom.Core
{
    public static class BankingResolver
    {
        public static int Bank(MatchState state, PlayerState player)
        {
            var banked = 0;
            for (var y = 0; y < state.Board.Height; y++)
            {
                for (var x = 0; x < state.Board.Width; x++)
                {
                    var pos = new GridPos(x, y);
                    if (state.Board.OwnerAt(pos) != player.Id)
                        continue;

                    state.Board.SetOwner(pos, TileState.NeutralOwner);
                    banked++;
                }
            }

            player.Score += banked;
            return banked;
        }
    }
}
