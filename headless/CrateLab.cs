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
        var strict = HasArg(args, "--assert");
        var opened = 0;
        var arrow = 0;
        var speed = 0;
        var missile = 0;
        var padlock = 0;
        var invalid = 0;

        for (var m = 0; m < matches; m++)
        {
            var config = RulesetPresets.CratesV1();
            var state = MatchFactory.CreateBotLab(config);
            var runner = new MatchRunner(config, new XorShiftRandom(seed + (uint)(m * 7919)));
            runner.Initialize(state);

            var safety = 0;
            while (!state.IsFinished && safety++ < 10000)
            {
                var result = runner.Tick(state);
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
        Console.WriteLine("matches=" + matches);
        Console.WriteLine("invalid_states=" + invalid);
        Console.WriteLine("crates_opened=" + opened);
        Console.WriteLine("crates_per_match=" + (matches == 0 ? 0 : opened / (double)matches).ToString("0.00"));
        Console.WriteLine("arrow_payloads=" + arrow);
        Console.WriteLine("speed_payloads=" + speed);
        Console.WriteLine("missile_payloads=" + missile);
        Console.WriteLine("padlock_payloads=" + padlock);

        if (!strict) return 0;
        if (invalid != 0) return Fail("invalid crate state or loose combat pickup detected");
        if (opened < matches) return Fail("crate loop is too inactive: fewer than one opening per match");
        if (arrow == 0 || speed == 0 || missile == 0 || padlock == 0) return Fail("not every v1 crate payload appeared in the deterministic sample");
        return 0;
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

    private static int Fail(string reason) { Console.Error.WriteLine("CRATE LAB FAILED: " + reason); return 2; }
    private static bool HasArg(string[] args, string key) { for (var i = 0; i < args.Length; i++) if (args[i] == key) return true; return false; }
    private static int ReadInt(string[] args, string key, int fallback) { for (var i = 0; i + 1 < args.Length; i++) if (args[i] == key) { int value; if (int.TryParse(args[i + 1], out value)) return value; } return fallback; }
}
