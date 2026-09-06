# Viral replay recipes

## Why PogoDom should not record video during every match

The battle core is already deterministic: a ruleset + seed + the human's sparse direction changes can reconstruct the same match. That is much cheaper and more useful than continuously recording transforms or video on every phone.

M0.28 turns highlight detection into a deterministic **render recipe**:

1. `MatchHighlightTracker` stores the exact simulation tick for each important moment.
2. `ViralReplayPlanner` picks the strongest timestamped moment deterministically.
3. The future renderer simulates from tick 0 with the same seed/inputs but stays visually hidden during `WarmupTicks`.
4. Only a short window around the highlight is rendered.
5. Photo finishes and comeback wins can append the existing winner showcase + equipped victory celebration.

The default share target is portrait 9:16. Core highlight replay windows are capped at 4.5 seconds before any result celebration is appended.

## Repository audit

Several public replay approaches were compared before this design:

### InputVCR

Useful idea: record player input and feed the recorded input back through the same gameplay path. It also adds transform synchronization to correct drift in non-deterministic/frame-time-sensitive games.

**PogoDom transplant:** input replay, not its per-frame recording architecture. PogoDom already owns a fixed-tick deterministic core and only needs direction changes, so transform correction should be unnecessary if replay integrity remains green.

### Angus-Fan/unityReplay

Useful idea: timestamp input events and replay them at the correct moment. The project notes that sampling input only in `FixedUpdate` can miss presses, which reinforces PogoDom's existing separation between touch gesture capture and fixed battle ticks.

**PogoDom transplant:** preserve high-frequency gesture capture, but commit only the direction that reaches a logical battle tick.

### StepanLem/Unity-Replay-System

Useful idea: generic record/replay data abstractions that work in builds.

**Why not import:** it records arbitrary component values and brings extra serialization/setup dependencies. That solves a harder, more general problem than PogoDom has. Our signed deterministic replay is smaller and already doubles as anti-cheat verification.

### Deterministic ECS/replay frameworks

Frameworks such as trecs demonstrate the value of fixed-step simulation, isolated input, state snapshots and desync detection.

**PogoDom transplant:** keep deterministic inputs and add snapshots later only if replay warm-up becomes measurable on real phones. Do not replace the battle architecture with an ECS just to obtain a replay feature we already possess.

## Viral-first context windows

The planner intentionally gives each event different context:

- Big Bank: show the route into the bank and the payoff.
- Leader Missile: enough pre-roll to establish the target, then impact.
- Area Capture: show the closure and fill.
- Arena Blast: show telegraph + detonation.
- Late Lead Change: more context because the ranking flip is the story.
- Photo Finish / Comeback Win: end on the result and append winner celebration.

These are first hypotheses. Human phone tests decide final cuts.

## Important boundary

M0.28 does **not** claim to export an MP4 yet. It creates the deterministic source window and presentation metadata needed for a future renderer/exporter. Actual video encoding, OS share sheet integration and performance belong to the Unity/device gate.
