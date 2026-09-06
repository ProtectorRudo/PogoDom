# PogoDom victory celebration collection

## Product rule

Celebrations exist to make winning memorable and shareable without slowing the route to the next match. They are presentation-only cosmetics. They must never alter score, movement, stun, power-up timing, city contribution or deterministic replay.

The initial procedural collection deliberately stays small:

| Id | Name | Rarity | Minimum rig | Duration |
| --- | --- | --- | --- | ---: |
| `victory-starter-bounce` | Victory Bounce | Common | RootOnly | 1.8 s |
| `victory-pogo-bow` | Pogo Bow | Rare | RootOnly | 1.7 s |
| `victory-sky-pop` | Sky Pop | Rare | TwoHands | 1.9 s |
| `victory-side-sway` | Side Sway | Epic | TwoHands | 2.1 s |
| `victory-aura-67` | Aura 67 | Legendary | TwoHands | 2.4 s |
| `victory-crown-pulse` | Crown Pulse | Mythic | Humanoid | 2.5 s |

`Aura 67` is the meme-like alternating-palms choreography introduced in M0.24. The other motions are original PogoDom gestures designed around different readable silhouettes so the inventory does not become six recolors of the same dance.

## Rig safety

PogoDom expects a mixed roster. Some future mascots may be humanoid, some may only expose two hand anchors, and some may only support a visual root transform.

`CelebrationRigCapability` is therefore part of the content contract:

`RootOnly < TwoHands < Humanoid`

When an equipped procedural celebration needs more capability than the current avatar exposes, presentation uses a compatible starter fallback for that render only. It does **not** unequip the player's cosmetic or mutate ownership. This prevents a funny skin from becoming unusable because its skeleton differs.

## Repository audit decision

The public Unity animation references audited before this milestone point to three useful patterns:

1. `HumanPoseHandler` is a practical retargeting seam for normalized humanoid poses.
2. Unity Playables are the right future seam for authored AnimationClip emotes, runtime queues and blend layers without a gigantic AnimatorController.
3. Animation C# Jobs / layer mixers are useful later for masked upper-body blends and IK, but importing a large dependency before real Unity profiling adds risk without proving player value.

PogoDom therefore keeps the procedural collection dependency-free today, while `VictoryCelebrationSelection.AssetKey` remains the common path for later authored clips/Playables.

## First Unity gate

When Unity access returns, do not approve an emote because its math is green. Check each celebration on at least:

- one normal humanoid;
- one exaggerated/chibi humanoid;
- one non-humanoid/root-only fallback;
- portrait phone framing;
- winner showcase timing with the REMATCH CTA already visible.

The visual test decides final amplitudes and camera crop. Headless tests only certify determinism, duration, compatibility and non-gameplay boundaries.
