using System;
using System.Collections.Generic;
using PogoDom.Core;

internal static class Program
{
    private sealed class Aggregate
    {
        public string Variant;
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
        public long Enclosures;
        public long EnclosedTiles;
        public long HazardWarnings;
        public long HazardBlasts;
        public long HazardTilesDestroyed;
        public long Padlocks;
        public long ProtectedPaints;
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
        if (HasArg(args, "--difficulty-lab"))
            return RunDifficultyLab(matches, seed, strict);

        var variant = ReadString(args, "--variant", "launch").ToLowerInvariant();
        var aggregate = new Aggregate { Variant = variant };

        for (var m = 0; m < matches; m++)
        {
            var config = CreateConfig(variant);
            if (config == null) return Fail("unknown variant: " + variant);
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
                        case MatchEventType.TileProtected: aggregate.ProtectedPaints++; break;
                        case MatchEventType.EnclosureCaptured: aggregate.Enclosures++; aggregate.EnclosedTiles += ev.Value; break;
                        case MatchEventType.Banked: aggregate.Banks++; break;
                        case MatchEventType.ArrowUsed: aggregate.Arrows++; break;
                        case MatchEventType.SpeedActivated: aggregate.Speeds++; break;
                        case MatchEventType.MissileFired: aggregate.Missiles++; break;
                        case MatchEventType.PadlockActivated: aggregate.Padlocks++; break;
                        case MatchEventType.PlayerStunned: aggregate.Stuns++; break;
                        case MatchEventType.HazardTelegraphed: aggregate.HazardWarnings++; break;
                        case MatchEventType.HazardDetonated: aggregate.HazardBlasts++; aggregate.HazardTilesDestroyed += ev.Value; break;
                    }
                }

                var leader = MatchOutcome.Leader(state);
                if (previousLeader != null && leader != null && previousLeader.Id != leader.Id) aggregate.LeadChanges++;
                previousLeader = leader;
                if (!StateValid(state)) { aggregate.InvalidStates++; break; }
            }

            if (safety >= 10000 || !state.IsFinished) aggregate.InvalidStates++;
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
        return strict ? AssertHealthy(aggregate) : 0;
    }

    private static int RunDifficultyLab(int matches, uint seed, bool strict)
    {
        var hardWins = 0;
        var invalidStates = 0;
        long placementTotal = 0;
        long hardScoreTotal = 0;
        long bestMediumScoreTotal = 0;

        for (var m = 0; m < matches; m++)
        {
            var config = new MatchConfig();
            var hardSlot = m % 4;
            var state = MatchFactory.CreateDifficultyLab(config, hardSlot);
            var runner = new MatchRunner(config, new XorShiftRandom(seed + (uint)(m * 3571)));
            runner.Initialize(state);

            var safety = 0;
            while (!state.IsFinished && safety++ < 10000)
            {
                runner.Tick(state);
                if (!StateValid(state))
                {
                    invalidStates++;
                    break;
                }
            }
            if (safety >= 10000 || !state.IsFinished) invalidStates++;

            var standings = MatchOutcome.Standings(state);
            var hardPlacement = standings.Count;
            var hardScore = 0;
            var bestMedium = 0;
            for (var i = 0; i < standings.Count; i++)
            {
                var standing = standings[i];
                if (standing.PlayerId == hardSlot)
                {
                    hardPlacement = i + 1;
                    hardScore = standing.Score;
                    if (i == 0) hardWins++;
                }
                else if (standing.Score > bestMedium)
                {
                    bestMedium = standing.Score;
                }
            }

            placementTotal += hardPlacement;
            hardScoreTotal += hardScore;
            bestMediumScoreTotal += bestMedium;
        }

        var winRate = matches == 0 ? 0 : hardWins / (double)matches;
        var avgPlacement = matches == 0 ? 0 : placementTotal / (double)matches;
        var avgHardScore = matches == 0 ? 0 : hardScoreTotal / (double)matches;
        var avgBestMedium = matches == 0 ? 0 : bestMediumScoreTotal / (double)matches;

        Console.WriteLine("POGODOM HARD BOT LAB");
        Console.WriteLine("matches=" + matches);
        Console.WriteLine("invalid_states=" + invalidStates);
        Console.WriteLine("hard_win_rate=" + winRate.ToString("P1"));
        Console.WriteLine("avg_hard_placement=" + avgPlacement.ToString("0.00"));
        Console.WriteLine("avg_hard_score=" + avgHardScore.ToString("0.00"));
        Console.WriteLine("avg_best_medium_score=" + avgBestMedium.ToString("0.00"));

        if (!strict) return 0;
        if (invalidStates != 0) return Fail("hard bot lab produced invalid states");
        if (winRate < 0.30) return Fail("hard bot did not establish a measurable advantage over 25% four-player baseline");
        if (avgPlacement > 2.25) return Fail("hard bot average placement is not stronger than the medium field");
        return 0;
    }

    private static MatchConfig CreateConfig(string variant)
    {
        switch (variant)
        {
            case "launch": return new MatchConfig();
            case "rivals": return new MatchConfig { BehaviorVersion = RulesetBehaviorVersion.V3 };
            case "loop": return new MatchConfig { EnableEnclosureCapture = true };
            case "loop2": return new MatchConfig { EnableEnclosureCapture = true, EnclosureCapturePolicy = EnclosureCapturePolicy.NeutralOnly };
            case "chaos": return new MatchConfig { EnableArenaChaos = true };
            case "chaos2": return new MatchConfig { EnableArenaChaos = true, TntInitialDelayTicks = 20, TntSpawnIntervalTicks = 30 };
            case "padlock": return new MatchConfig { EnablePadlockPower = true };
            case "padlock2": return new MatchConfig { EnablePadlockPower = true, PadlockDurationTicks = 10, PadlockRespawnDelayTicks = 24 };
            case "fusion": return new MatchConfig { EnableEnclosureCapture = true, EnableArenaChaos = true, EnablePadlockPower = true };
            default: return null;
        }
    }

    private static int AssertHealthy(Aggregate a)
    {
        var blockedRatio = a.Moved + a.Blocked == 0 ? 1.0 : a.Blocked / (double)(a.Moved + a.Blocked);
        if (a.InvalidStates != 0) return Fail("invalid states detected");
        if (a.Banks == 0) return Fail("bank crates were never used");
        if (a.Speeds == 0) return Fail("speed pickups were never used");
        if (a.Missiles == 0 || a.Stuns == 0) return Fail("missile/stun loop never occurred");
        if (a.Steals == 0) return Fail("no tile stealing occurred");
        if (blockedRatio >= 0.55) return Fail("more than 55% of movement phases are blocked");
        if (a.Variant == "rivals" && a.LeadChanges == 0) return Fail("adaptive rivals produced no leader pressure or lead changes");
        if ((a.Variant == "loop" || a.Variant == "loop2" || a.Variant == "fusion") && a.Enclosures == 0) return Fail("loop variant never produced an enclosure");
        if ((a.Variant == "chaos" || a.Variant == "chaos2" || a.Variant == "fusion") && a.HazardBlasts == 0) return Fail("chaos variant never detonated a hazard");
        if ((a.Variant == "padlock" || a.Variant == "padlock2" || a.Variant == "fusion") && a.Padlocks == 0) return Fail("padlock variant never activated a shield");
        return 0;
    }

    private static int Fail(string reason) { Console.Error.WriteLine("SIM ASSERTION FAILED: " + reason); return 2; }

    private static void Print(Aggregate a)
    {
        var blockedRatio = a.Moved + a.Blocked == 0 ? 0 : a.Blocked / (double)(a.Moved + a.Blocked);
        Console.WriteLine("POGODOM HEADLESS LAB");
        Console.WriteLine("variant=" + a.Variant);
        Console.WriteLine("matches=" + a.Matches);
        Console.WriteLine("invalid_states=" + a.InvalidStates);
        Console.WriteLine("avg_winner_score=" + PerMatch(a.WinnerScore, a.Matches));
        Console.WriteLine("avg_margin=" + PerMatch(a.Margin, a.Matches));
        Console.WriteLine("close_finish_rate=" + (a.Matches == 0 ? 0 : a.CloseFinishes / (double)a.Matches).ToString("P1"));
        Console.WriteLine("blocked_phase_ratio=" + blockedRatio.ToString("P1"));
        Console.WriteLine("lead_changes_per_match=" + PerMatch(a.LeadChanges, a.Matches));
        Console.WriteLine("steals_per_match=" + PerMatch(a.Steals, a.Matches));
        Console.WriteLine("banks_per_match=" + PerMatch(a.Banks, a.Matches));
        Console.WriteLine("arrows_per_match=" + PerMatch(a.Arrows, a.Matches));
        Console.WriteLine("speeds_per_match=" + PerMatch(a.Speeds, a.Matches));
        Console.WriteLine("missiles_per_match=" + PerMatch(a.Missiles, a.Matches));
        Console.WriteLine("enclosures_per_match=" + PerMatch(a.Enclosures, a.Matches));
        Console.WriteLine("enclosed_tiles_per_match=" + PerMatch(a.EnclosedTiles, a.Matches));
        Console.WriteLine("hazard_warnings_per_match=" + PerMatch(a.HazardWarnings, a.Matches));
        Console.WriteLine("hazard_blasts_per_match=" + PerMatch(a.HazardBlasts, a.Matches));
        Console.WriteLine("hazard_tiles_destroyed_per_match=" + PerMatch(a.HazardTilesDestroyed, a.Matches));
        Console.WriteLine("padlocks_per_match=" + PerMatch(a.Padlocks, a.Matches));
        Console.WriteLine("protected_paints_per_match=" + PerMatch(a.ProtectedPaints, a.Matches));
    }

    private static string PerMatch(long value, int matches) => (matches == 0 ? 0 : value / (double)matches).ToString("0.00");

    private static bool StateValid(MatchState state)
    {
        var players = new HashSet<GridPos>();
        for (var i = 0; i < state.Players.Count; i++) if (!players.Add(state.Players[i].Position)) return false;
        var items = new HashSet<GridPos>();
        for (var i = 0; i < state.Items.Count; i++) if (!items.Add(state.Items[i].Position)) return false;
        var hazards = new HashSet<GridPos>();
        for (var i = 0; i < state.Hazards.Count; i++)
        {
            var pos = state.Hazards[i].Position;
            if (!hazards.Add(pos) || items.Contains(pos)) return false;
        }
        return true;
    }

    private static bool HasArg(string[] args, string key) { for (var i = 0; i < args.Length; i++) if (args[i] == key) return true; return false; }
    private static int ReadInt(string[] args, string key, int fallback) { for (var i = 0; i + 1 < args.Length; i++) if (args[i] == key) { int value; if (int.TryParse(args[i + 1], out value)) return value; } return fallback; }
    private static string ReadString(string[] args, string key, string fallback) { for (var i = 0; i + 1 < args.Length; i++) if (args[i] == key) return args[i + 1]; return fallback; }
}
