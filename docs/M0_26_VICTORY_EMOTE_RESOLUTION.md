# M0.26 — Winner celebration resolution

M0.24 created procedural victory choreography and M0.25 decides which player wins the post-match showcase. M0.26 closes the cosmetic selection seam: given the winner's existing `CosmeticLoadout`, PogoDom can now resolve the exact victory-emote asset that should be rendered.

## Rules

- An equipped `VictoryEmoteId` always wins over the fallback.
- An empty victory slot falls back to `victory-starter-bounce`.
- Procedural celebrations such as `victory-aura-67` expose their deterministic choreography definition.
- Future authored AnimationClip/Playable celebrations use the same slot and resolver through their `AssetKey`; no second inventory system is required.
- A trail, pogo, skill visual or other non-victory cosmetic can never be accidentally dispatched as a winner celebration.

This remains presentation-only and has no dependency on score calculation, match RNG, power timing or city progression.
