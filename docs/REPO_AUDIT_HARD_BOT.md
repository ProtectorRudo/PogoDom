# Repo audit transplant — Hard bot

## Source ideas audited

### `bitsmag/squares`

The RL environment is useful as a **policy-design reference**, not as a runtime dependency. It exposes a compact board observation (ownership channels + specials), player position/direction/score, and reward shaping around:

- score delta;
- newly claimed territory;
- avoiding idle streaks;
- score margin against opponents;
- terminal winning margin;
- speed-special pickup.

Its training loop depends on Python/Gym/PPO and an HTTP bridge to the game server. Shipping that stack inside a mobile match would be unnecessary, slower, harder to verify and hostile to offline deterministic replay.

### `Alekssasho/PogoPainter`

Its bot chases the nearest checkpoint/bonus and greedily chooses a cardinal step that reduces Euclidean distance. That is a good lightweight fallback idea, but it does not reason about danger, score conversion or multi-step opportunities.

### `AgusCrow/pogo-painter-web`

Its bot scores candidate adjacent tiles using ownership, power-ups, player collisions, nearby friendly territory and randomness. Those ingredients are useful, but the implementation is one-step and server/database oriented.

## PogoDom adaptation

`HardBotBrain` keeps the useful signals while changing the architecture:

- pure C#, local and offline;
- deterministic tie-breaking for verified replays;
- three-step local lookahead;
- explicit hazard avoidance;
- banking urgency from unbanked territory and late-match state;
- item value that changes with score state;
- immediate enclosure awareness;
- anti-loop/path revisit penalty;
- no network calls, Python runtime or trained-model dependency.

`BotDifficulty.Hard` no longer aliases Medium.

## Certification

The headless suite now includes a rotating four-player difficulty lab: one Hard bot versus three identically-personalized Medium bots, with the Hard starting corner rotated every match. The gate requires zero invalid states plus a measurable advantage over the 25% four-player baseline. This proves the difficulty tier is mechanically distinct; it does **not** prove that the tier feels fair or fun to humans.
