# Visual GitHub audit — M0.31

Date: 2026-09-06

Goal: find reusable visual ideas/code for PogoDom without turning the project into a dependency pile or copying art from another game.

## 1. ColinLeung-NiloCat/UnityURPToonLitShaderExample

Repository: `ColinLeung-NiloCat/UnityURPToonLitShaderExample`

License: MIT.

Useful ideas found:
- URP-specific toon lighting instead of relying on a generic Standard material;
- an explicit outline pass rather than post-process edge detection;
- small reusable HLSL helpers;
- a shader structure compatible with a stylized character-focused mobile look.

PogoDom transplant:
- implemented our own compact `PogoDom/ToonLit` shader with banded main-light shading, rim, emission and optional inverted-hull outline;
- did **not** copy the repository wholesale;
- outlines are restricted to avatars and important pickups by the runtime upgrader to protect clarity and mobile cost.

Why this is valuable: PogoDom needs saturated player colors to survive small-screen clips. Physically realistic lighting tends to muddy those colors; controlled toon bands keep them readable.

## 2. Unity Animation Rigging package / mirrors

Repository audited: `needle-mirror/com.unity.animation.rigging`

License: Unity Companion License for Unity-dependent projects.

Useful ideas:
- RigBuilder-style layered animation;
- Two Bone IK / Multi Aim / constraint-based secondary posing;
- rig effectors that can sit above authored clips instead of requiring a unique clip for every cosmetic combination.

PogoDom decision:
- do not vendor the full package into M0.31 while the target Unity version is not yet locked;
- our avatar contract now exposes stable sockets and keeps procedural presentation separate from gameplay;
- when real FBX avatars arrive, Animation Rigging is the preferred candidate for hand-to-handle correction, head/aim polish and accessory-safe poses.

## 3. Unity C# animation jobs samples

Repository audited: `needle-mirror/com.unity.animation.cs-jobs-samples`.

Useful direction:
- animation layers can be composed procedurally rather than baking every combination;
- presentation corrections can stay outside battle rules.

PogoDom decision:
- keep the current procedural celebration layer and avatar presentation layer independent;
- reserve Animation Jobs for later only if profiler data shows a real need. M0 does not need this complexity yet.

## 4. dentedpixel/LeanTween

Repository audited as a possible source for juice/tween orchestration.

Useful direction:
- lightweight tween-driven presentation is appropriate for tile pops, HUD feedback and short result sequences.

PogoDom decision:
- no dependency imported in M0.31. The repository root did not expose an immediately auditable license file during this pass, and PogoDom only needs a few tiny exponential/Mathf interpolation functions today;
- implemented the required squash/stretch, tile pop and camera impulse directly to keep runtime dependencies at zero.

## 5. Existing Pogo repositories

The Pogo Painter / Pogo Pandemonium family remains the gameplay reference, not the visual target. Their sprites/web primitives are useful for understanding rule readability but are not a premium 2026 mobile art direction.

PogoDom must therefore reuse their **clarity**, not their rendering.

## Visual architecture decision

The visual stack is deliberately layered:

`Core battle state` -> `Match events / rendered transforms` -> `PogoDom presentation components` -> `cosmetic loadout / skill visual` -> `Unity renderers/VFX`.

Nothing points back from presentation into Core.

That protects all of these properties simultaneously:
- signed deterministic replay remains valid;
- cosmetics cannot create pay-to-win;
- we can replace procedural avatars with premium FBX models later;
- we can replace procedural skill VFX with authored VFX later;
- we can change shaders without changing rulesets;
- a low-end device can disable expensive presentation without changing the match.

## What was implemented in M0.31

- `PogoAvatarVisualRig`: procedural stylized avatar fallback, pogo, contact shadow, trail, squash/stretch, tilt and cosmetic sockets.
- `PogoProceduralSkillSocket`: runtime fallback for equipped skill visuals.
- `PogoDomViralVisualDirector`: auto-installs on the prototype and upgrades arena framing, tiles, corner beacons, backdrop, pickups and camera response.
- `PogoDomTileJuice`: ownership-change pop/glow.
- `PogoPickupJuice`: pickup pulse/halo.
- `PogoDomCameraJuice`: short decaying micro impulse only after large visual tile changes.
- `PogoDom/ToonLit`: mobile-oriented stylized URP shader.
- `PogoDomToonUpgrade`: stronger avatar/pickup outlines while leaving the board clean.
- New cosmetic loadout slots: `Headwear`, `BackAccessory`, `Aura`.
- Existing skill visuals remain trigger/context-specific and presentation-only.

## Known validation boundary

Headless CI can compile and test the cosmetic/loadout contract, but it cannot compile UnityEngine runtime scripts or the URP shader. These visual components must be compiled and judged inside the chosen Unity version when Unity access returns. Until then they are implemented source, not visually certified output.
