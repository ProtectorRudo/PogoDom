# Engagement dead-zone guardrails

M0.30 adds a deterministic, analytics-only way to reject match variants that drift toward long stretches with no meaningful beat. It does not change battle rules, scoring, bots, RNG or signed replay behavior.

## Two levels of activity

`Activity` is deliberately broad: ordinary territorial stealing plus all higher-value events. It answers whether the board is mechanically doing something.

`Spectacle` is deliberately stricter. It includes big banks (8+), arrows, missiles, stuns, enclosure captures of 3+ tiles, TNT warnings/blasts, padlock activation, mystery-crate openings and lead changes. It answers whether something a player is likely to notice has happened.

This distinction matters because PogoDom can produce hundreds of tile steals in a 75-second match. Counting every steal as proof of entertainment would hide dull stretches.

## Baseline measured on 2026-09-06

Each candidate below ran 150 deterministic bot matches with seed family `20260906 + matchIndex * 6151`. All produced zero invalid states and zero 15-second spectacle dead zones.

| Variant | Spectacle beat ticks / match | Avg longest spectacle quiet span | Max spectacle quiet span | Matches with 10s dead zone | Matches with 15s dead zone |
| --- | ---: | ---: | ---: | ---: | ---: |
| Launch | 34.30 | 5.85s | 10.0s | 1.3% | 0% |
| Rivals V1 | 33.23 | 6.28s | 10.0s | 1.3% | 0% |
| Loop V2 | 35.63 | 5.68s | 10.0s | 0.7% | 0% |
| Chaos V2 | 41.70 | 5.26s | 9.0s | 0% | 0% |
| Padlock V2 | 38.92 | 5.50s | 8.5s | 0% | 0% |
| Crates V2 | 29.84 | 6.51s | 11.0s | 2.0% | 0% |

The data does **not** prove which mode is most fun. It only gives us a reproducible warning system. Chaos V2 has the densest spectacle but can still be too noisy; Crates V2 is the quietest of these candidates but remains inside the current envelope. Human playtesting decides fun.

## CI product guardrails

For the deterministic CI sample, every measured candidate must satisfy all three:

- average longest spectacle quiet span <= **7.5 seconds**;
- no more than **5%** of matches may contain a spectacle dead zone of at least **10 seconds**;
- **zero** matches may contain a spectacle dead zone of at least **15 seconds**.

These are regression guardrails, not automatic game-design targets. A future candidate that fails is not automatically rejected forever, but the change must be investigated instead of silently shipping a slower match rhythm.

## Why this stays outside battle rules

The tracker consumes `MatchState` and `TickResult` after each deterministic tick. Nothing from the tracker is fed into bot decisions, item spawns, score, movement or meta progression. Removing the tracker therefore leaves the exact same simulated match.
