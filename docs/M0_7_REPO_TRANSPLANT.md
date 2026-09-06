# M0.7 — Repo transplant: robust enclosure

## Why this exists

The deeper repo audit found a mechanic worth isolating before we commit it to the launch rules: automatic area capture when a player's color forms a closed boundary.

It preserves the product constitution:

- one hand: no new button;
- understandable: "close a shape and it fills" is visible, not textual;
- addictive: creates large swing moments;
- viral: a sudden multi-tile fill is clip-friendly;
- bots: gives them another tactical objective;
- zero friction: resolution is automatic.

## What was taken and what was rejected

### `bitsmag/squares`

Useful: the idea that a colored circuit can score/capture an interior and that this belongs in the deterministic tick pipeline.

Rejected: its row-by-row interior calculation. That approach can misclassify concave shapes, multiple holes and irregular circuits.

PogoDom replacement: outside-in flood fill. A player's tiles are treated as walls; every non-player tile reachable from a board edge is exterior; the rest is truly enclosed.

### `AgusCrow/pogo-pandemonium` — Pogo-A-Gogo

Useful: automatic area creation as a satisfying consequence of movement.

Rejected: `surroundedCount >= 3` as an enclosure test. A tile being blocked on three local sides is not a proof that the whole region is enclosed.

### `AgusCrow/pogo-pandemonium` — El Pogo Loco

Useful: area capture can coexist with banking and produce a more explosive board.

Rejected: the prototype's random `70%` interior fill. Geometry must be deterministic; the same shape must always produce the same result.

### fairness improvement beyond the source repos

Direct landing paints are applied first for every player. Enclosures are then evaluated from the same board snapshot. If two players claim the same enclosed cell in the same phase, PogoDom leaves it unchanged rather than silently favoring lower player id / iteration order.

## Launch decision

Enclosure is **implemented but not forced into the launch ruleset yet**.

- `launch-m0-2`: current bank-first rules, enclosure OFF.
- `pogodom-loop-v1`: identical tuning, enclosure ON.

This deliberately creates a clean A/B candidate. Unity playtesting will decide whether loop capture becomes default. Until then, both variants are deterministic and server-verifiable.

## Bot adaptation

Medium bots now preview whether a candidate landing closes an area. If it does, the move receives a meaningful tactical bonus. Banker bots value closure slightly more because a large unbanked fill creates a strong reason to race toward the next Bank Crate.

## Viral layer

An enclosure of 3+ tiles can become an `AreaCapture` highlight. This gives the eventual client a direct hook for camera punch, replay snippets, share cards and character skill VFX without changing gameplay balance.
