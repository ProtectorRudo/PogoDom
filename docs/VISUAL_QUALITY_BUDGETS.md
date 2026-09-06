# Visual quality budgets

M0.35 protects a core product requirement: PogoDom can look premium without turning an ordinary phone into a frame-rate test. Visual quality tiers may reduce decoration; they may never remove gameplay information or alter deterministic state.

## Tiers

`Lite` keeps the complete board, all competitors, pickups, hazard telegraphs, tile ownership feedback and result flow. It reduces decorative city density, disables decorative city lights and pickup halo shells, shortens trails, scales outlines and caps procedural skill orbiters.

`Balanced` is the default middle target for modern phones. It preserves more backdrop, halos, longer trails and larger presentation budgets.

`Showcase` is the full visual target for capable devices and capture/review builds.

The pure `VisualQualityPolicy` also supplies particle multipliers and maximum concurrent spectacle bursts for the event-driven VFX layer. M0.32 event budgets remain the upper bound; quality tiers can only scale them down.

## Auto selection

Auto selection uses conservative graphics memory, system memory and shader-level thresholds. Unknown/zero device reports select Lite rather than gambling frame pacing. This is a starting heuristic, not a permanent device database; real phone profiling can refine it later.

## Non-negotiable readability

The policy explicitly classifies these as essential and therefore not removable by quality scaling:

- playable tiles;
- players;
- bank/mystery crates;
- arrow/speed/missile/padlock pickups;
- hazard telegraphs.

The runtime scaler only operates on presentation objects. It never reads or writes score, movement, item spawn rules, RNG, bot state, city/nation progression or replay verification.

## Why this matters for viral visuals

A visually impressive game that stutters on the phones creating the clips is not a viral visual system. PogoDom should prefer a stable silhouette, readable tile steal and clean 60-ish-fps-feeling motion over an extra city light or fourth orbiting particle.

The final tier thresholds and target frame pacing must be calibrated on real devices when Unity access returns. GitHub can certify monotonic budgets and the fact that essential visual names are protected; it cannot benchmark a GPU from source code.
