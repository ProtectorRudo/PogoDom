using System;
using System.Collections.Generic;
using PogoDom.Core;
using PogoDom.Verification;

internal static class CrateLab
{
    private static int Main(string[] args)
    {
        var matches = ReadInt(args, "--matches", 200);
        var seed = (uint)ReadInt(args, "--seed", 20260906);
        var variant = ReadString(args, "--variant", "v1").ToLowerInvariant();
        var strict = HasArg(args, "--assert");
        var opened = 0;
        var spawned = 0;
        var arrow = 0;
        var speed = 0;
        var missile = 0;
        var padlock = 0;
        var invalid = 0;
        var unsafeNearPlayer = 0;
        var unbalancedContest = 0;
        var crowdedCrates = 0;

        for (var m = 0; m < matches; m++)
        {
            var config = variant == "v2" ? RulesetPresets.CratesV2() : RulesetPresets.CratesV1();
            var state = MatchFactory.CreateBotLab(config);
            var runner = new MatchRunner(config, new XorShiftRandom(seed + (uint)(m * 7919)));
            var seenCrates = new HashSet<int>();
            runner.Initialize(state);
            ObserveNewCrates(state, config, seenCrates, ref spawned, ref unsafeNearPlayer, ref unbalancedContest, ref crowdedCrates);

            var safety = 0;
            while (!state.IsFinished && safety++ < 10000)
            {
                var result = runner.Tick(state);
                ObserveNewCrates(state, config, seenCrates, ref spawned, ref unsafeNearPlayer, ref unbalancedContest, ref crowdedCrates);

                for (var i = 0; i < result.Events.Count; i++)
                {
                    var ev = result.Events[i];
                    if (ev.Type != MatchEventType.CrateOpened) continue;
                    opened++;
                    switch (ev.ItemKind)
                    {
                        case PowerUpKind.Arrow: arrow++; break;
                        case PowerUpKind.Speed: speed++; break;
                        case PowerUpKind.Missile: missile++; break;
                        case PowerUpKind.Padlock: padlock++; break;
                        default: invalid++; break;
                    }
                }

                if (!StateValid(state) || HasLooseCombatPickup(state))
                {
                    invalid++;
                    break;
                }
            }
            if (!state.IsFinished || safety >= 10000) invalid++;
        }

        Console.WriteLine("POGODOM MYSTERY CRATE LAB");
        Console.WriteLine("variant=" + variant);
        Console.WriteLine("matches=" + matches);
        Console.WriteLine("invalid_states=" + invalid);
        Console.WriteLine("crates_spawned=" + spawned);
        Console.WriteLine("crates_opened=" + opened);
        Console.WriteLine("crates_per_match=" + (matches == 0 ? 0 : opened / (double)matches).ToString("0.00"));
        Console.WriteLine("unsafe_near_player_spawns=" + unsafeNearPlayer);
        Console.WriteLine("unbalanced_contest_spawns=" + unbalancedContest);
        Console.WriteLine("crowded_crate_spawns=" + crowdedCrates);
        Console.WriteLine("arrow_payloads=" + arrow);
        Console.WriteLine("speed_payloads=" + speed);
        Console.WriteLine("missile_payloads=" + missile);
        Console.WriteLine("padlock_payloads=" + padlock);

        if (!strict) return 0;
        if (variant != "v1" && variant != "v2") return Fail("unknown variant: " + variant);
        if (invalid != 0) return Fail("invalid crate state or loose combat pickup detected");
        if (opened < matches) return Fail("crate loop is too inactive: fewer than one opening per match");
        if (arrow == 0 || speed == 0 || missile == 0 || padlock == 0) return Fail("not every crate payload appeared in the deterministic sample");
        if (variant == "v2" && unsafeNearPlayer != 0) return Fail("V2 spawned a mystery crate too close to a player");
        if (variant == "v2" && unbalancedContest != 0) return Fail("V2 spawned an uncontestable mystery crate despite strict candidate availability");
        if (variant == "v2" && crowdedCrates != 0) return Fail("V2 spawned mystery crates too close together");
        return 0;
    }

    private static void ObserveNewCrates(
        MatchState state,
        MatchConfig config,
        HashSet<int> seen,
        ref int spawned,
        ref int unsafeNearPlayer,
        ref int unbalancedContest,
        ref int crowdedCrates)
    {
        for (var i = 0; i < state.Items.Count; i++)
        {
            var item = state.Items[i];
            if (item.Kind != PowerUpKind.MysteryCrate || !seen.Add(item.Id)) continue;
            spawned++;

            var closest = int.MaxValue;
            var second = int.MaxValue;
            for (var p = 0; p < state.Players.Count; p++)
            {
                var distance = Manhattan(item.Position, state.Players[p].Position);
                if (distance < config.MysteryCrateMinimumPlayerManhattanDistance) unsafeNearPlayer++;
                if (distance < closest) { second = closest; closest = distance; }
                else if (distance < second) second = distance;
            }
            if (second != int.MaxValue && second - closest > config.MysteryCrateMaximumClosestPlayerDistanceGap)
                unbalancedContest++;

            for (var j = 0; j < state.Items.Count; j++)
            {
                var other = state.Items[j];
                if (other.Id == item.Id || other.Kind != PowerUpKind.MysteryCrate) continue;
                if (Chebyshev(item.Position, other.Position) < config.MysteryCrateMinimumCrateChebyshevDistance)
                {
                    crowdedCrates++;
                    break;
                }
            }
        }
    }

    private static bool HasLooseCombatPickup(MatchState state)
    {
        for (var i = 0; i < state.Items.Count; i++)
        {
            var kind = state.Items[i].Kind;
            if (kind == PowerUpKind.Arrow || kind == PowerUpKind.Speed || kind == PowerUpKind.Missile || kind == PowerUpKind.Padlock)
                return true;
        }
        return false;
    }

    private static bool StateValid(MatchState state)
    {
        var players = new HashSet<GridPos>();
        for (var i = 0; i < state.Players.Count; i++) if (!players.Add(state.Players[i].Position)) return false;
        var items = new HashSet<GridPos>();
        for (var i = 0; i < state.Items.Count; i++) if (!items.Add(state.Items[i].Position)) return false;
        return true;
    }

    private static int Manhattan(GridPos a, GridPos b) => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    private static int Chebyshev(GridPos a, GridPos b) => Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));
    private static int Fail(string reason) { Console.Error.WriteLine("CRATE LAB FAILED: " + reason); return 2; }
    private static bool HasArg(string[] args, string key) { for (var i = 0; i < args.Length; i++) if (args[i] == key) return true; return false; }
    private static int ReadInt(string[] args, string key, int fallback) { for (var i = 0; i + 1 < args.Length; i++) if (args[i] == key) { int value; if (int.TryParse(args[i + 1], out value)) return value; } return fallback; }
    private static string ReadString(string[] args, string key, string fallback) { for (var i = 0; i + 1 < args.Length; i++) if (args[i] == key) return args[i + 1]; return fallback; }
}
