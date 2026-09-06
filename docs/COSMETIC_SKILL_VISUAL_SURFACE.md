# Cosmetic skill-visual surface

PogoDom monetizes **presentation**, never the competitive outcome. The Core decides what happened; Cosmetics decides how the already-decided event looks for the equipped player.

## Current cosmetic event surface

Existing: jump, landing, tile steal, bank, Arrow, Speed, Missile, victory, defeat.

M0.19 adds presentation slots for:

- Mystery Crate opening;
- Padlock activation;
- enclosed-area capture;
- a protected steal attempt;
- TNT blast;
- stun impact.

A `SkillVisualDefinition` may be generic for a trigger or restricted to a `PowerUpKind`. Exact power context wins over the generic fallback. Example: a player can equip one generic Mystery Crate opening and a special Dragon reveal only when the crate contains Missile.

## Deterministic equipment slots

Only one cosmetic is equipped for the same `(trigger, powerContext)` slot. Equipping another replaces it. This removes order-dependent rendering and gives store/loadout UI an unambiguous slot model.

## Examples that are safe to sell

- Missile: dragon, comet, ghost, lightning projectile skin;
- Arrow: paint wave, petals, electricity, pixel blast;
- Speed: trail/afterimage package;
- Padlock: bubble, city shield, crystal dome;
- Area capture: color shockwave / city crest burst;
- Crate reveal: stars, hologram, confetti, themed portal;
- stun impact: dizzy stars, comic burst, glitch effect;
- victory: dance/emote already remains a separate cosmetic slot.

All examples must preserve hit timing, area, duration, score, stun and every other gameplay value. Visual rarity can change spectacle, not power.
