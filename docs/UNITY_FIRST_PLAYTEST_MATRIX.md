# PogoDom — first Unity playtest matrix

The headless lab can reject broken or obviously unhealthy rule combinations. It cannot certify feel. This matrix makes the first real Unity session deliberately small and comparable.

## Candidate order

1. **Base** — reference: paint, steal, BankCrates, Arrow, Speed, Missile.
2. **Crates V1** — same bank loop, but combat powers arrive through mystery crates using the historical generic spawn behavior.
3. **Crates V2** — identical crate economy and payloads, but mystery crates spawn away from players, separated and contestable. Test immediately after V1 so placement feel is the only meaningful difference.
4. **Loop V2** — same base plus neutral-only enclosed-area fill.
5. **Padlock V2** — same base plus shorter/rarer automatic protection.
6. **Chaos V2** — lower-frequency telegraphed TNT; test last because it adds the most visual noise.

Do not test Fusion first. The headless numbers already showed that stacking every system can widen outcomes even though the simulation remains technically valid.

## How to switch

On `PogoDomPrototypeBootstrap`, choose `playtestMode` in the Inspector. `CratesV2` now appears beside `CratesV1`. The client profile is regression-tested against the corresponding frozen verification ruleset so Unity and server replay use the same values.

For the Crates A/B specifically, keep the same `seed` and play V1 then V2 back-to-back before changing anything else. The payload economy is intentionally identical; the question is whether V2 removes the feeling that a strong crate was simply gifted to somebody.

## Non-negotiable controls

- auto-bounce remains 0.5 s;
- swipe anywhere on screen;
- direction commits when the gesture crosses threshold, not on finger-up;
- no joystick;
- no power button;
- pickups/crates trigger automatically on landing.

## What the first human session must answer

For each candidate, play at least three consecutive matches before changing mode and record:

- **understood without explanation?** target: core loop obvious inside 20 s;
- **missed-input feeling?** there should be no recurring “I swiped but it jumped the old way” complaint;
- **visual cause/effect?** player can say why a tile/area/score changed;
- **bot credibility?** opponents feel intentional, not random or psychic;
- **rematch urge?** would the player immediately tap another match without being asked;
- **frustration source?** losing should be attributable to a readable event, not hidden RNG;
- **crate fairness?** in V2, a crate should feel contestable rather than pre-awarded by spawn position.

## Promotion rule

A headless metric never promotes a mechanic by itself. A candidate can move toward launch only after it preserves one-thumb clarity and produces stronger or equal rematch desire than Base. For the crate family, V2 must also feel at least as exciting as V1 while reducing “that box was handed to them” moments. Crates, Loop, Padlock and Chaos remain independently switchable until that happens.
