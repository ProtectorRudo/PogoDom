# M0.45 — Adaptive visual quality without gameplay compromise

## Problem found during audit

M0.43 pooled equivalent toon materials, but the existing quality scaler still used `renderer.materials` when shrinking outlines. In Unity that accessor can instantiate renderer-local material copies, silently undoing the pooling work. This milestone removes that conflict before real-phone profiling.

## Material-safe quality scaling

Outline quality now uses `MaterialPropertyBlock` per renderer/material slot. The authored/shared toon material stays shared and outline scaling is calculated from the shared material's base width, so repeated quality application is idempotent rather than multiplying the previous result.

## Aura density

Attachment auras are presentation decoration, not gameplay information. Quality budgets now cap visible aura orbiters:

- Lite: 1;
- Balanced: 3;
- Showcase: 6.

Players, tiles, pickups, hazards and the core avatar silhouette are never removed.

## Adaptive governor

When quality is `Auto`, PogoDom measures a smoothed unscaled frame time after a short warmup. A single explosion or loading hitch is not enough to react.

- sustained >=28 ms frame time must persist about 2.5 seconds;
- critical >=40 ms pressure may downgrade after about 0.9 seconds;
- quality can only move Showcase -> Balanced -> Lite during a live session;
- it never upgrades mid-match, preventing distracting visual oscillation;
- after a downgrade there is a cooldown before another decision.

Gameplay, tick rate, input, bot thinking and physics are untouched. Only presentation budget changes.

## CI source guard

Headless certification now fails if Runtime presentation code introduces:

- an assignment to `Time.timeScale`;
- runtime addition of Rigidbody/Collider physics components;
- `renderer.materials` cloning inside the visual quality scaler.

This does not replace a Unity compile/profile, but it turns several product invariants into permanent repository gates.

## Real-device acceptance gate

On representative Android hardware we still need to profile normal play and screen recording. Auto quality is accepted only if it protects frame pacing without producing noticeable tier changes, removing essential information or making the Lite presentation look cheap/unreadable.
