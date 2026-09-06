# M0.8 — Chaos transplant, adapted for PogoDom

## Source audit

`AgusCrow/pogo-pandemonium/ElPogoLoco.tsx` contains a useful chaos vocabulary: TNT, missiles, arrows and a moving chaos actor. The prototype, however, chooses an action randomly every five seconds, mutates the board from React state and can erase territory with little warning.

PogoDom keeps **the useful tension**, not that implementation.

## PogoDom adaptation

The first chaos experiment is a neutral, deterministic TNT director:

1. No proprietary chaos character is required.
2. TNT appears as a telegraphed arena warning.
3. Default warning lasts 2 seconds / 4 bounces at the current 0.5-second tick.
4. Explosion clears a 3x3 area of **unbanked paint only**.
5. Confirmed score is untouchable.
6. A caught player misses exactly one bounce.
7. Medium bots see the warning and usually route away; Chaotic bots take more risk.
8. Spawn selection prefers a cell whose blast has meaningful painted impact, then resolves ties with seeded RNG.

This is intentionally more legible and fair than an invisible/random board mutation while still creating clips and last-second path decisions.

## What was rejected from Pogo Padlock

The audited `PogoPadlock.tsx` exposes explicit buttons for firing a missile and activating a lock, and its missile can recolor every unlocked opponent tile in one action. That is too much control/UI surface for PogoDom's one-thumb constitution and too swingy for a 75-second match.

Padlock remains a future candidate, but if promoted it must auto-trigger from landing/context, never require a second gameplay button.

## Ruleset isolation

- `launch-m0-2`: chaos OFF.
- `pogodom-loop-v1`: enclosure ON, chaos OFF.
- `pogodom-chaos-v1`: chaos ON, enclosure OFF.

The feature is implemented and server-verifiable, but not promoted to launch gameplay before Unity playtesting.
