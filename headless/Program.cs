using System;
using System.Collections.Generic;
using PogoDom.Core;

internal static class Program
{
    private sealed class Aggregate
    {
        public int Matches;
        public int InvalidStates;
        public long Ticks;
        public long Moved;
        public long Blocked;
        public long Steals;
        public long Banks;
        public long Arrows;
        public long Speeds;
        public long Missiles;
        public long Stuns;
        public long LeadChanges;
        public long CloseFinishes;
        public long WinnerScore;
        public long Margin;
    }

    private static int Main(string[] args)
    {
        var matches = ReadInt(args, "--matches", 500);
        var seed = (uint)ReadInt(args, "--seed", 20260905);
        var strict = HasArg(args, "--assert");
        var aggregate = new Aggregate();

        for (var m = 0; m < matches; m++)
        {
            var config = new MatchConfig();
            var state = MatchFactory.CreateBotLab(config);
            var random = new XorShiftRandom(seed + (uint)(m * 7919));
            var runner = new MatchRunner(config, random);
            runner.Initialize(state);

            var previousLeader = MatchOutcome.Leader(state);
            var safety = 0;
            while (!state.IsFinished && safety++ < 10000)
            {
                var result = runner.Tick(state);
                aggregate.Ticks++;

                for (var e = 0; e < result.Events.Count; e++)
                {
                    var ev = result.Events[e];
                    switch (ev.Type)
                    {
                        case MatchEventType.PlayerMoved: aggregate.Moved++; break;
                        case MatchEventType.PlayerBlocked: aggregate.Blocked++; break;
                        case MatchEventType.TileStolen: aggregate.Steals++; break;
                        case MatchEventType.Banked: aggregate.Banks++; break;
                        case MatchEventType.ArrowUsed: aggregate.Arrows++; break;
                        case MatchEventType.SpeedActivated: aggregate.Speeds++; break;
                        case MatchEventType.MissileFired: aggregate.Missiles++; break;
                        case MatchEventType.PlayerStunned: aggregate.Stuns++; break;
                    }
                }

                var leader = MatchOutcome.Leader(state);
                if (previousLeader != null && leader != null && previousLeader.Id != leader.Id)
                    aggregate.LeadChanges++;
                previousLeader = leader;

                if (!PlayersUnique(state) || !ItemsUnique(state))
                {
                    aggregate.InvalidStates++;
                    break;
                }
            }

            if (safety >= 10000 || !state.IsFinished)
                aggregate.InvalidStates++;

            var standings = MatchOutcome.Standings(state);
            if (standings.Count >= 2)
            {
                var margin = standings[0].Score - standings[1].Score;
                aggregate.WinnerScore += standings[0].Score;
                aggregate.Margin += margin;
                if (margin <= 5) aggregate.CloseFinishes++;
            }
            aggregate.Matches++;
        }

        Print(aggregate);

        if (!strict)
            return 0;

        var blockedRatio = aggregate.Moved + aggregate.Blocked == 0
            ? 1.0
            : aggregate.Blocked / (double)(aggregate.Moved + aggregate.Blocked);

        if (aggregate.InvalidStates != 0) return Fail("invalid states detected");
        if (aggregate.Banks == 0) return Fail("bank crates were never used");
        if (aggregate.Speeds == 0) return Fail("speed pickups were never used");
        if (aggregate.Missiles == 0 || aggregate.Stuns == 0) return Fail("missile/stun loop never occurred");
        if (aggregate.Steals == 0) return Fail("no tile stealing occurred");
        if (blockedRatio >= 0.55) return Fail("more than 55% of movement phases are blocked");
        return 0;
    }

    private static int Fail(string reason)
    {
        Console.Error.WriteLine("SIM ASSERTION FAILED: " + reason);
        return 2;
    }

    private static void Print(Aggregate a)
    {
        var blockedRatio = a.Moved + a.Blocked == 0 ? 0 : a.Blocked / (double)(a.Moved + a.Blocked);
        Console.WriteLine("POGODOM HEADLESS LAB");
        Console.WriteLine("matches=" + a.Matches);
        Console.WriteLine("invalid_states=" + a.InvalidStates);
        Console.WriteLine("avg_winner_score=" + (a.Matches == 0 ? 0 : a.WinnerScore / (double)a.Matches).ToString("0.00"));
        Console.WriteLine("avg_margin=" + (a.Matches == 0 ? 0 : a.Margin / (double)a.Matches).ToString("0.00"));
        Console.WriteLine("close_finish_rate=" + (a.Matches == 0 ? 0 : a.CloseFinishes / (double)a.Matches).ToString("P1"));
        Console.WriteLine("blocked_phase_ratio=" + blockedRatio.ToString("P1"));
        Console.WriteLine("lead_changes_per_match=" + (a.Matches == 0 ? 0 : a.LeadChanges / (double)a.Matches).ToString("0.00"));
        Console.WriteLine("steals_per_match=" + (a.Matches == 0 ? 0 : a.Steals / (double)a.Matches).ToString("0.00"));
        Console.WriteLine("banks_per_match=" + (a.Matches == 0 ? 0 : a.Banks / (double)a.Matches).ToString("0.00"));
        Console.WriteLine("arrows_per_match=" + (a.Matches == 0 ? 0 : a.Arrows / (double)a.Matches).ToString("0.00"));
        Console.WriteLine("speeds_per_match=" + (a.Matches == 0 ? 0 : a.Speeds / (double)a.Matches).ToString("0.00"));
        Console.WriteLine("missiles_per_match=" + (a.Matches == 0 ? 0 : a.Missiles / (double)a.Matches).ToString("0.00"));
    }

    private static bool PlayersUnique(MatchState state)
    {
        var seen = new HashSet<GridPos>();
        for (var i = 0; i < state.Players.Count; i++)
            if (!seen.Add(state.Players[i].Position)) return false;
        return true;
    }

    private static bool ItemsUnique(MatchState state)
    {
        var seen = new HashSet<GridPos>();
        for (var i = 0; i < state.Items.Count; i++)
            if (!seen.Add(state.Items[i].Position)) return false;
        return true;
    }

    private static bool HasArg(string[] args, string key)
    {
        for (var i = 0; i < args.Length; i++) if (args[i] == key) return true;
        return false;
    }

    private static int ReadInt(string[] args, string key, int fallback)
    {
        for (var i = 0; i + 1 < args.Length; i++)
        {
            if (args[i] == key)
            {
                int value;
                if (int.TryParse(args[i + 1], out value)) return value;
            }
        }
        return fallback;
    }
}
