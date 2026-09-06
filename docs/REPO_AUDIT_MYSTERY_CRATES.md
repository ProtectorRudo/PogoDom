# Repo audit transplant — Mystery power crates

## What was useful

### `AgusCrow/pogo-pandemonium`

Its central chest creates a strong hotspot: touching it converts currently painted, unregistered territory into permanent score, then the chest disappears and respawns later. Arrow and Missile exist as separate random pickups. This confirmed the value of separating **score conversion** from **combat power**.

### `Alekssasho/PogoPainter`

`BonusManager` replenishes checkpoints/bonuses only on free cells and keeps checkpoints spatially separated. `Checkpoint::apply` converts the player's currently painted board cells into points; `Arrow::apply` paints a ray from the player. This is the same useful loop in a second independent implementation.

## PogoDom adaptation

PogoDom already had the strongest part as `BankCrate`: touching it banks owned territory into confirmed score. M0.16 keeps that mechanic intact and changes only how combat powers are surfaced in an experimental ruleset:

- loose Arrow / Speed / Missile / Padlock pickups are removed from `pogodom-crates-v1`;
- two Mystery Crates are maintained on the board;
- each crate receives a hidden power payload **at spawn time**;
- the payload is stored in `ItemState` and revealed only on opening;
- no button is added: landing opens and activates the crate automatically;
- bots value a Mystery Crate as an unknown opportunity and never inspect its hidden payload;
- the crate shell is now a stable presentation seam for future crate skins/VFX without changing gameplay.

## Frozen PowerMixV1

- Arrow 35%
- Speed 25%
- Missile 25%
- Padlock 15%

Do not edit those percentages after release. A different distribution must receive a new `MysteryCrateTableId` and a new ruleset id.

## Why payload is pre-rolled

Rolling on pickup would make the result depend on every RNG call that happened before a player reached the crate. Pre-rolling at spawn makes the hidden content part of deterministic battle state, which is safer for signed offline replay and server verification.

## What was intentionally rejected

- React/database state as gameplay authority;
- frame-rate-dependent `Math.random()` spawning;
- extra action buttons;
- a mystery crate that can secretly produce a Bank effect (banking remains readable and strategic);
- TNT inside the mystery power pool (arena hazards remain telegraphed rather than surprise punishment).
