using System;

namespace PogoDom.Core
{
    [Serializable]
    public sealed class PlayerState
    {
        public int Id { get; }
        public string Name { get; }
        public bool IsHuman { get; }
        public GridPos Position { get; internal set; }
        public Direction CurrentDirection { get; internal set; }
        public int Score { get; internal set; }
        public int StunTicksRemaining { get; internal set; }
        public int SpeedTicksRemaining { get; internal set; }
        public int PadlockTicksRemaining { get; internal set; }
        public BotDifficulty BotDifficulty { get; internal set; }
        public BotPersonality BotPersonality { get; internal set; }

        public bool IsStunned => StunTicksRemaining > 0;
        public bool HasSpeed => SpeedTicksRemaining > 0;
        public bool HasPadlock => PadlockTicksRemaining > 0;

        public PlayerState(
            int id,
            string name,
            bool isHuman,
            GridPos position,
            Direction initialDirection,
            BotDifficulty botDifficulty = BotDifficulty.Medium,
            BotPersonality botPersonality = BotPersonality.Balanced)
        {
            Id = id;
            Name = name ?? $"Player {id + 1}";
            IsHuman = isHuman;
            Position = position;
            CurrentDirection = initialDirection;
            BotDifficulty = botDifficulty;
            BotPersonality = botPersonality;
        }
    }
}
