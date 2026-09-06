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
            runner.Initialize(state);

            // Initialize() deliberately discards spawn events, but nobody has moved yet,
            // so the initial population can be assessed directly without ambiguity.
            ObserveInitialCrates(state, config, ref spawned, ref unsafeNearPlayer, ref unbalancedContest, ref crowdedCrates);

            var safety = 0;
            while (!state.IsFinished && safety++ < 10000)
            {
                // ItemSpawner can replenish both before and after movement. Snapshot the
                // state before Tick(), then replay the emitted event order so a crate is
                // judged against the player positions that existed at its exact spawn.
                // Looking only at state after Tick() incorrectly calls a safe spawn unsafe
                // when a player lands beside the crate later in the same bounce.
                var playerPositions = SnapshotPlayerPositions(state);
                var activeCrates = SnapshotCratePositions(state);

                var result = runner.Tick(state);
                ObserveTickEvents(
                    result.Events,
                    playerPositions,
                    activeCrates,
                    config,
                    ref spawned,
                    ref opened,
                    ref arrow,
                    ref speed,
                    ref missile,
                    ref padlock,
                    ref invalid,
                    ref unsafeNearPlayer,
                    ref unbalancedContest,
                    ref crowdedCrates);

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

    private static void ObserveInitialCrates(
        MatchState state,
        MatchConfig config,
        ref int spawned,
        ref int unsafeNearPlayer,
        ref int unbalancedContest,
        ref int crowdedCrates)
    {
        var players = SnapshotPlayerPositions(state);
        var crates = SnapshotCratePositions(state);
        foreach (var crate in crates)
        {
            var others = new HashSet<GridPos>(crates);
            others.Remove(crate);
            spawned++;
            AssessSpawn(crate, players, others, config, ref unsafeNearPlayer, ref unbalancedContest, ref crowdedCrates);
        }
    }

    private static void ObserveTickEvents(
        List<MatchEvent> events,
        Dictionary<int, GridPos> playerPositions,
        HashSet<GridPos> activeCrates,
        MatchConfig config,
        ref int spawned,
        ref int opened,
        ref int arrow,
        ref int speed,
        ref int missile,
        ref int padlock,
        ref int invalid,
        ref int unsafeNearPlayer,
        ref int unbalancedContest,
        ref int crowdedCrates)
    {
        for (var i = 0; i < events.Count; i++)
        {
            var ev = events[i];
            switch (ev.Type)
            {
                case MatchEventType.ItemSpawned:
                    if (ev.ItemKind != PowerUpKind.MysteryCrate) break;
                    spawned++;
                    AssessSpawn(ev.Position, playerPositions, activeCrates, config, ref unsafeNearPlayer, ref unbalancedContest, ref crowdedCrates);
                    activeCrates.Add(ev.Position);
                    break;

                case MatchEventType.ItemConsumed:
                    if (ev.ItemKind == PowerUpKind.MysteryCrate)
                        activeCrates.Remove(ev.Position);
                    break;

                case MatchEventType.PlayerMoved:
                case MatchEventType.PlayerBlocked:
                    if (ev.PlayerId >= 0)
                        playerPositions[ev.PlayerId] = ev.Position;
                    break;

                case MatchEventType.CrateOpened:
                    opened++;
                    switch (ev.ItemKind)
                    {
                        case PowerUpKind.Arrow: arrow++; break;
                        case PowerUpKind.Speed: speed++; break;
                        case PowerUpKind.Missile: missile++; break;
                        case PowerUpKind.Padlock: padlock++; break;
                        default: invalid++; break;
                    }
                    break;
            }
        }
    }

    private static void AssessSpawn(
        GridPos position,
        Dictionary<int, GridPos> playerPositions,
        HashSet<GridPos> otherCrates,
        MatchConfig config,
        ref int unsafeNearPlayer,
        ref int unbalancedContest,
        ref int crowdedCrates)
    {
        var closest = int.MaxValue;
        var second = int.MaxValue;
        foreach (var pair in playerPositions)
        {
            var distance = Manhattan(position, pair.Value);
            if (distance < config.MysteryCrateMinimumPlayerManhattanDistance)
                unsafeNearPlayer++;
            if (distance < closest) { second = closest; closest = distance; }
            else if (distance < second) second = distance;
        }

        if (second != int.MaxValue && second - closest > config.MysteryCrateMaximumClosestPlayerDistanceGap)
            unbalancedContest++;

        foreach (var other in otherCrates)
        {
            if (Chebyshev(position, other) < config.MysteryCrateMinimumCrateChebyshevDistance)
            {
                crowdedCrates++;
                break;
            }
        }
    }

    private static Dictionary<int, GridPos> SnapshotPlayerPositions(MatchState state)
    {
        var positions = new Dictionary<int, GridPos>();
        for (var i = 0; i < state.Players.Count; i++)
            positions[state.Players[i].Id] = state.Players[i].Position;
        return positions;
    }

    private static HashSet<GridPos> SnapshotCratePositions(MatchState state)
    {
        var positions = new HashSet<GridPos>();
        for (var i = 0; i < state.Items.Count; i++)
            if (state.Items[i].Kind == PowerUpKind.MysteryCrate)
                positions.Add(state.Items[i].Position);
        return positions;
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
