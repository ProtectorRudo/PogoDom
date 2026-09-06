# M0.21 — Mystery Crates V2 fair spawn

## Why

Crates V1 validated the hidden pre-rolled power loop, but its placement inherited the generic V1 rejection sampler. That preserved determinism but could hand a high-impact crate to a player simply because the random cell happened to be adjacent.

The audited Alekssasho/PogoPainter bonus manager already demonstrated a useful principle: valuable board objects should respect spatial separation. PogoDom keeps that idea and adapts it to a four-player mobile arena rather than copying the old runtime.

## V2 contract

`pogodom-crates-v2` keeps the exact same power economy as `pogodom-crates-v1` and changes only `RulesetBehaviorVersion` from V1 to V2. This makes the A/B comparison attributable to spawn semantics rather than different power frequencies.

For Mystery Crates under behavior V2:

- never spawn on a player, item, or hazard;
- never spawn within Manhattan distance 1 of any player (minimum distance 2);
- prefer at least Chebyshev distance 2 from another Mystery Crate;
- prefer cells where the two closest players differ by at most one movement step, so the crate is contestable instead of gifted;
- if the arena is congested, relax contestability first, then crate-to-crate separation;
- never relax player safety;
- choose deterministically from the surviving candidate set with the match RNG.

V1 placement remains in its own untouched code path because old signed replays depend on the previous RNG-consumption contract.

## Certification

The core test suite checks V2 spatial invariants across 200 seeds. The crate lab now runs both V1 and V2 over 200 bot matches, reports near-player, contest-gap and crate-spacing violations, and requires zero V2 violations while also preserving payload diversity, match completion and the no-loose-pickup rule.

No candidate is promoted to launch by this change. Human Unity playtesting still decides whether the fairer placement feels better rather than merely looking better in headless statistics.
