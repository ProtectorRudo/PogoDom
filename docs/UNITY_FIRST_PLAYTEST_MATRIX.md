# PogoDom — first Unity playtest matrix

The headless lab can reject broken or obviously unhealthy rule combinations. It cannot certify feel. This matrix makes the first real Unity session deliberately small and comparable.

## Candidate order

1. **Base** — reference: paint, steal, BankCrates, Arrow, Speed, Missile.
2. **Loop V2** — same base plus neutral-only enclosed-area fill.
3. **Padlock V2** — same base plus shorter/rarer automatic protection.
4. **Crates V1** — BankCrates stay explicit; combat powers arrive through mystery crates.
5. **Chaos V2** — lower-frequency telegraphed TNT; test last because it adds the most visual noise.

Do not test Fusion first. The headless numbers already showed that stacking every system can widen outcomes even though the simulation remains technically valid.

## How to switch

On `PogoDomPrototypeBootstrap`, choose `playtestMode` in the Inspector. The client profile is regression-tested against the corresponding frozen verification ruleset so Unity and server replay use the same values.

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
- **frustration source?** losing should be attributable to a readable event, not hidden RNG.

## Promotion rule

A headless metric never promotes a mechanic by itself. A candidate can move toward launch only after it preserves one-thumb clarity and produces stronger or equal rematch desire than Base. Crates, Loop, Padlock and Chaos remain independently switchable until that happens.
