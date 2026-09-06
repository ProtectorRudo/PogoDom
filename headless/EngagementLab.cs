using System;
using System.Collections.Generic;
using PogoDom.Core;
using PogoDom.Session;
using PogoDom.Verification;

internal static class EngagementLab
{
    private sealed class Aggregate
    {
        public string Variant;
        public int Matches;
        public int InvalidStates;
        public long TotalTicks;
        public long ActivityBeatTicks;
        public long SpectacleBeatTicks;
        public double SumLongestActivityQuietSeconds;
        public double SumLongestSpectacleQuietSeconds;
        public double MaxLongestActivityQuietSeconds;
        public double MaxLongestSpectacleQuietSeconds;
        public int MatchesWithSpectacle10;
        public int MatchesWithSpectacle15;
        public long SpectacleQuietTicks10;
        public long SpectacleQuietTicks15;
        public int SpectacleSpans10;
        public int SpectacleSpans15;
    }

    private static int Main(string[] args)
    {
        var matches = ReadInt(args, "--matches", 250);
        var seed = (uint)ReadInt(args, "--seed", 20260906);
        var variant = ReadString(args, "--variant", "launch").ToLowerInvariant();
        var strict = HasArg(args, "--assert");
        var aggregate = new Aggregate { Variant = variant };

        for (var m = 0; m < matches; m++)
        {
            var config = CreateConfig(variant);
            if (config == null) return Fail("unknown variant: " + variant);

            var state = MatchFactory.CreateBotLab(config);
            var runner = new MatchRunner(config, new XorShiftRandom(seed + (uint)(m * 6151)));
            var engagement = new MatchEngagementTracker(config.TickSeconds);
            runner.Initialize(state);

            var safety = 0;
            while (!state.IsFinished && safety++ < 10000)
            {
                var tick = runner.Tick(state);
                engagement.Observe(state.Tick - 1, state, tick);
                if (!StateValid(state))
                {
                    aggregate.InvalidStates++;
                    break;
                }
            }

            if (safety >= 10000 || !state.IsFinished)
                aggregate.InvalidStates++;

            var report = engagement.Complete(state.Tick);
            Accumulate(aggregate, report);
            aggregate.Matches++;
        }

        Print(aggregate);
        return strict ? AssertHealthy(aggregate) : 0;
    }

    private static void Accumulate(Aggregate a, MatchEngagementReport report)
    {
        a.TotalTicks += report.TotalTicks;
        a.ActivityBeatTicks += report.ActivityBeatTicks;
        a.SpectacleBeatTicks += report.SpectacleBeatTicks;
        a.SumLongestActivityQuietSeconds += report.LongestActivityQuietSeconds;
        a.SumLongestSpectacleQuietSeconds += report.LongestSpectacleQuietSeconds;
        if (report.LongestActivityQuietSeconds > a.MaxLongestActivityQuietSeconds)
            a.MaxLongestActivityQuietSeconds = report.LongestActivityQuietSeconds;
        if (report.LongestSpectacleQuietSeconds > a.MaxLongestSpectacleQuietSeconds)
            a.MaxLongestSpectacleQuietSeconds = report.LongestSpectacleQuietSeconds;

        var spans10 = report.CountQuietSpansAtLeast(EngagementTier.Spectacle, 10f);
        var spans15 = report.CountQuietSpansAtLeast(EngagementTier.Spectacle, 15f);
        if (spans10 > 0) a.MatchesWithSpectacle10++;
        if (spans15 > 0) a.MatchesWithSpectacle15++;
        a.SpectacleSpans10 += spans10;
        a.SpectacleSpans15 += spans15;
        a.SpectacleQuietTicks10 += report.QuietTicksInsideSpansAtLeast(EngagementTier.Spectacle, 10f);
        a.SpectacleQuietTicks15 += report.QuietTicksInsideSpansAtLeast(EngagementTier.Spectacle, 15f);
    }

    private static MatchConfig CreateConfig(string variant)
    {
        switch (variant)
        {
            case "launch": return RulesetPresets.LaunchM02();
            case "rivals": return RulesetPresets.RivalsV1();
            case "loop2": return RulesetPresets.LoopV2();
            case "chaos2": return RulesetPresets.ChaosV2();
            case "padlock2": return RulesetPresets.PadlockV2();
            case "crates2": return RulesetPresets.CratesV2();
            default: return null;
        }
    }

    private static int AssertHealthy(Aggregate a)
    {
        if (a.InvalidStates != 0) return Fail("engagement lab produced invalid states");
        if (a.Matches == 0) return Fail("engagement lab ran zero matches");
        if (a.ActivityBeatTicks == 0) return Fail("engagement detector observed no activity beats");
        if (a.SpectacleBeatTicks == 0) return Fail("engagement detector observed no spectacle beats");
        return 0;
    }

    private static void Print(Aggregate a)
    {
        Console.WriteLine("POGODOM ENGAGEMENT DEAD-ZONE LAB");
        Console.WriteLine("variant=" + a.Variant);
        Console.WriteLine("matches=" + a.Matches);
        Console.WriteLine("invalid_states=" + a.InvalidStates);
        Console.WriteLine("activity_beat_ticks_per_match=" + PerMatch(a.ActivityBeatTicks, a.Matches));
        Console.WriteLine("spectacle_beat_ticks_per_match=" + PerMatch(a.SpectacleBeatTicks, a.Matches));
        Console.WriteLine("avg_longest_activity_quiet_seconds=" + PerMatch(a.SumLongestActivityQuietSeconds, a.Matches));
        Console.WriteLine("avg_longest_spectacle_quiet_seconds=" + PerMatch(a.SumLongestSpectacleQuietSeconds, a.Matches));
        Console.WriteLine("max_activity_quiet_seconds=" + a.MaxLongestActivityQuietSeconds.ToString("0.00"));
        Console.WriteLine("max_spectacle_quiet_seconds=" + a.MaxLongestSpectacleQuietSeconds.ToString("0.00"));
        Console.WriteLine("spectacle_deadzone_10s_match_rate=" + Rate(a.MatchesWithSpectacle10, a.Matches));
        Console.WriteLine("spectacle_deadzone_15s_match_rate=" + Rate(a.MatchesWithSpectacle15, a.Matches));
        Console.WriteLine("spectacle_deadzone_10s_spans_per_match=" + PerMatch(a.SpectacleSpans10, a.Matches));
        Console.WriteLine("spectacle_deadzone_15s_spans_per_match=" + PerMatch(a.SpectacleSpans15, a.Matches));
        Console.WriteLine("spectacle_time_inside_10s_dead_zones=" + Share(a.SpectacleQuietTicks10, a.TotalTicks));
        Console.WriteLine("spectacle_time_inside_15s_dead_zones=" + Share(a.SpectacleQuietTicks15, a.TotalTicks));
    }

    private static bool StateValid(MatchState state)
    {
        var players = new HashSet<GridPos>();
        for (var i = 0; i < state.Players.Count; i++)
            if (!players.Add(state.Players[i].Position)) return false;

        var items = new HashSet<GridPos>();
        for (var i = 0; i < state.Items.Count; i++)
            if (!items.Add(state.Items[i].Position)) return false;

        var hazards = new HashSet<GridPos>();
        for (var i = 0; i < state.Hazards.Count; i++)
        {
            var pos = state.Hazards[i].Position;
            if (!hazards.Add(pos) || items.Contains(pos)) return false;
        }
        return true;
    }

    private static int Fail(string reason)
    {
        Console.Error.WriteLine("ENGAGEMENT ASSERTION FAILED: " + reason);
        return 2;
    }

    private static string PerMatch(long value, int matches) => (matches == 0 ? 0 : value / (double)matches).ToString("0.00");
    private static string PerMatch(double value, int matches) => (matches == 0 ? 0 : value / matches).ToString("0.00");
    private static string Rate(int value, int total) => (total == 0 ? 0 : value / (double)total).ToString("P1");
    private static string Share(long value, long total) => (total == 0 ? 0 : value / (double)total).ToString("P1");

    private static bool HasArg(string[] args, string key)
    {
        for (var i = 0; i < args.Length; i++) if (args[i] == key) return true;
        return false;
    }

    private static int ReadInt(string[] args, string key, int fallback)
    {
        for (var i = 0; i + 1 < args.Length; i++)
            if (args[i] == key)
            {
                int value;
                if (int.TryParse(args[i + 1], out value)) return value;
            }
        return fallback;
    }

    private static string ReadString(string[] args, string key, string fallback)
    {
        for (var i = 0; i + 1 < args.Length; i++)
            if (args[i] == key) return args[i + 1];
        return fallback;
    }
}
