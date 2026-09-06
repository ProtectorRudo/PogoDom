# PogoDom Headless Experiment Lab

GitHub can now compare complete deterministic matches across isolated rulesets before Unity is available.

## Variants

- `launch`: current conservative battle baseline.
- `loop`: launch + robust enclosure capture.
- `chaos`: launch + telegraphed TNT arena director.
- `padlock`: launch + automatic territory shield.
- `fusion`: all three experiments enabled together. Fusion is a stress/interoperability target, **not** a launch recommendation.

## Metrics

The lab reports winner score, margin, close-finish rate, blocked movement, lead changes, steals, banks, power usage, enclosure frequency/size, hazard frequency/destruction and Padlock activation/protected-paint frequency.

CI runs 250 launch matches and 100 of every experimental family on every PR. It fails on invalid state, excessive movement blocking or when an enabled experimental system never actually occurs.

## What the numbers can and cannot prove

They can prove that a system executes, remains deterministic, composes with other systems, does not deadlock and produces measurable competition/interaction.

They **cannot prove fun, clarity, visual readability or rematch urge**. Those remain Unity playtest gates. We deliberately refuse to promote `loop`, `chaos`, `padlock` or `fusion` to launch solely because a metric moves in a favorable direction.

The purpose of this lab is to arrive at the first Unity session with fewer broken hypotheses and several already-certified alternatives ready for immediate A/B playtesting.
