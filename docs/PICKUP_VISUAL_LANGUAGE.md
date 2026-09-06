# Launch pickup visual language

A PogoDom player should not need to read a label to know which object is worth chasing. Color helps, but silhouette and motion are the primary language.

## Six launch identities

- **BankCrate -> BankVault**: strapped vault/crate silhouette with a coin/core accent.
- **MysteryCrate -> MysteryRelic**: rotating relic/diamond with orbit-like accent pieces.
- **Arrow -> DirectionArrow**: literal directional shaft + chevron. Its identity layer never spins away from the gameplay direction.
- **Speed -> SpeedBurst**: energetic core with fins/spike and the strongest safe bob/spin motion.
- **Missile -> MissileRocket**: nose, fins and flame silhouette.
- **Padlock -> PadlockShield**: lock body + shackle silhouette.

Every launch pickup owns a unique `PickupVisualArchetype`; tests fail if two launch powers collapse onto the same archetype.

## Motion contract

Pickup motion is deliberately bounded so the board remains the dominant gameplay surface:

- bob amplitude <= 0.08 world units;
- identity spin <= 100 degrees/second;
- pulse scale <= 8%;
- at most four accent parts per pickup identity.

The Arrow identity has zero additional spin because orientation is gameplay information.

## Runtime behavior

`PogoDomPickupIdentityDirector` auto-installs beside the prototype bootstrap. Initial pickups are decorated immediately; replacement pickups are discovered on a low-frequency direct-child scan and decorated once.

The identity geometry is presentation-only and uses collider-free child primitives. It does not replace the actual `ItemState`, pickup position, spawn rules or hit logic.

Motion phase uses a stable name hash rather than process-randomized `string.GetHashCode()`, so replaying the same visible object naming produces the same starting phase.

## Future assets

These procedural forms are a readability prototype, not final art. Final authored models/icons should preserve the same six silhouette families and motion hierarchy. A prettier asset is not accepted if it makes Bank/Mystery/Arrow/Speed/Missile/Padlock harder to distinguish at phone size.

## Real Unity boundary

Phone validation must still confirm:

1. the six objects are distinguishable in peripheral vision at the real camera distance;
2. emissive accents do not overpower player colors or TNT telegraphs;
3. all identities remain clear in 9:16 portrait;
4. the extra child renderers are acceptable on Lite hardware;
5. Arrow geometry points in the same visible direction as its Core `ArrowDirection`.
