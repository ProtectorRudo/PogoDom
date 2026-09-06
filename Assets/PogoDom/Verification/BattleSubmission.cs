using System;
using System.Collections.Generic;
using PogoDom.Core;

namespace PogoDom.Verification
{
    public readonly struct DirectionChange
    {
        public int Tick { get; }
        public Direction Direction { get; }

        public DirectionChange(int tick, Direction direction)
        {
            Tick = tick;
            Direction = direction;
        }
    }

    public sealed class BattleSubmission
    {
        public SignedMatchTicket SignedTicket { get; }
        public IReadOnlyList<DirectionChange> Inputs { get; }

        public BattleSubmission(SignedMatchTicket signedTicket, IReadOnlyList<DirectionChange> inputs)
        {
            SignedTicket = signedTicket ?? throw new ArgumentNullException(nameof(signedTicket));
            Inputs = inputs ?? Array.Empty<DirectionChange>();
        }
    }
}
