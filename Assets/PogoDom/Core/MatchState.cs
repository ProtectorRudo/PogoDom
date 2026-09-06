using System.Collections.Generic;

namespace PogoDom.Core
{
    public sealed class MatchState
    {
        public BoardState Board { get; }
        public List<PlayerState> Players { get; }
        public List<ItemState> Items { get; }
        public List<ArenaHazard> Hazards { get; }
        public int Tick { get; internal set; }
        public float RemainingSeconds { get; internal set; }

        public bool IsFinished => RemainingSeconds <= 0f;

        public MatchState(BoardState board, List<PlayerState> players, float remainingSeconds)
        {
            Board = board;
            Players = players;
            Items = new List<ItemState>();
            Hazards = new List<ArenaHazard>();
            RemainingSeconds = remainingSeconds;
        }

        public PlayerState PlayerById(int id)
        {
            for (var i = 0; i < Players.Count; i++)
            {
                if (Players[i].Id == id)
                    return Players[i];
            }
            return null;
        }

        public ItemState ItemAt(GridPos position)
        {
            for (var i = 0; i < Items.Count; i++)
            {
                if (Items[i].Position == position)
                    return Items[i];
            }
            return null;
        }

        public ArenaHazard HazardAt(GridPos position)
        {
            for (var i = 0; i < Hazards.Count; i++)
            {
                if (Hazards[i].Position == position)
                    return Hazards[i];
            }
            return null;
        }
    }
}
