# M0.41 — Avatar attachment identity kits

## Why this milestone exists

PogoDom's characters must remain readable at phone scale while also supporting a large cosmetic economy. A skin alone is not enough: the same character needs recognizable headwear, back pieces, aura language, trails and landing signatures without changing competitive behavior.

## Launch visual families

Six complete presentation kits are defined and deliberately use different silhouettes/motion signatures:

- Hero — sport cap / sport pack / orbit ring / dash trail / clean landing ring.
- Trickster — split crown / twin fins / split orbit / zigzag trail / split landing ring.
- Tech — antenna / battery / hex pulse / pulse trail / tech landing ring.
- Mascot — ears / tail / bubble orbit / bubble trail / bubble-pop landing.
- Street — beanie / cape / graffiti orbit / ribbon trail / street stamp.
- Captain — crest / banner / crest orbit / royal trail / crown burst.

The current four-player prototype receives Hero, Trickster, Tech and Mascot so the pipeline can be judged immediately once Unity is available.

## Runtime behavior

`PogoDomAvatarAttachmentDirector` waits for the base avatar rig, then decorates only presentation sockets. Existing temporary cap/backpack geometry is disabled instead of stacking duplicate accessories. Aura objects live outside gameplay geometry, trails reuse the existing visual trail socket, and landing effects are reusable rings rather than per-bounce allocations.

## Competitive integrity

`AvatarAttachmentVisualProfile` contains only visual family/proportion values. Headless tests explicitly reject competitive-looking properties such as speed, jump, score, damage, stun, hitbox, collision, power or cooldown.

These objects must never own colliders, rigidbodies, movement, scoring or Core state.

## Real-device acceptance gate

M0.41 is architecturally complete but not visually certified until a real Unity/mobile run proves:

1. all four prototype players can be distinguished without reading their names;
2. headwear/back pieces remain readable but never cover the board;
3. auras do not hide neighboring tiles or pickups;
4. landing signatures read in under a second and do not become repetitive noise;
5. Lite/Balanced/Showcase still maintain stable frame pacing;
6. switching to authored FBX cosmetics requires replacing assets, not battle code.
