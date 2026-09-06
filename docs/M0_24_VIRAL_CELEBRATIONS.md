# M0.24 — Viral victory celebrations

## Why this exists

PogoDom needs post-match moments that are readable in one second, memorable enough to clip, and monetizable without changing battle power. Victory celebrations are therefore a dedicated cosmetic surface, not gameplay state.

The first procedural choreography is **Aura 67**: both palms are presented upward while the hands alternate height in a see-saw rhythm. It intentionally contains no song, voice line, branded character, downloaded animation or third-party visual asset.

## Repository audit

The implementation direction was chosen after auditing several open Unity animation approaches:

- `umiyuki/HumanoidHandPoseHelper` — MIT. Useful proof that `HumanPoseHandler` + humanoid muscle values make hand poses reusable across humanoid characters. This is the closest technical match to portable emotes.
- `EricHu33/uPlayableAnimation` — MIT. Strong future option for authored celebration clips because Playables scale better than a giant AnimatorController and can blend runtime clips dynamically.
- `Unity-Technologies/animation-jobs-samples` — Unity Companion License. Excellent reference for weighted masks, clip mixing, two-bone IK and full-body IK, but heavier than needed for the first viral emote layer.
- `EggyStudio/Unity.Humanoid.PoseController` — MPL-2.0. Confirms runtime humanoid-muscle posing is practical. No source from that package is copied into PogoDom.
- procedural IK repositories such as `Sopiro/Unity-Procedural-Animation` are useful for locomotion and environment contact, but are unnecessary dependency weight for a short victory gesture.

## Decision: the useful gold

Do **not** import another animation framework yet. PogoDom now owns a tiny engine-independent choreography layer:

1. `VictoryCelebrationDefinition` is a real `VictoryEmote` cosmetic.
2. `VictoryCelebrationSampler` converts time into normalized intent channels.
3. `Aura 67` uses six alternating hand accents over 2.4 seconds, palms-up throughout the readable middle of the gesture.
4. `HumanoidVictoryCelebrationDriver` maps those channels to Unity Mecanim muscles with `HumanPoseHandler` and resolves muscles by name rather than hard-coded indices.
5. The visual root is optional so celebrations never need to move the gameplay/collision root.
6. Future non-humanoid characters can consume the same normalized channels through a two-hand or root-only adapter.

## Competitive integrity

A celebration lives entirely in `Cosmetics`/`Runtime`. It has no access to score, movement, collision, RNG, power-up timing, city progression or match rules. Equipping `victory-aura-67` only fills the existing `VictoryEmoteId` loadout slot.

## What is certified without Unity

Headless CI compiles the new cosmetic model and tests that:

- the 67 choreography alternates hand dominance;
- both palms remain presented during the readable gesture;
- start/end are neutral to avoid snapping;
- sampling is deterministic;
- the celebration can be owned/equipped as a victory cosmetic;
- invalid definitions are rejected.

The Unity adapter itself still requires a real Unity compile and visual calibration on the final avatar rig. Its muscle amplitudes are deliberately serialized/tunable rather than treated as visually certified values.
