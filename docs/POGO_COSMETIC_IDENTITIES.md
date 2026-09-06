# Pogo cosmetic identities

The pogo is not only a movement prop. In PogoDom it is part of the character silhouette and therefore a first-class cosmetic surface.

## Six launch families

The six avatar archetypes receive matching pogo families:

- **Hero -> HeroSport**: wider sport foot, aero fins, bright badge.
- **Trickster -> TricksterCoil**: compact foot, exaggerated coil/ring language, bell accent.
- **Tech -> TechPulse**: pulse core, twin rails, neon accent.
- **Mascot -> MascotBubble**: soft round foot/core and oversized grips.
- **Street -> StreetDeck**: long deck-like foot and street badge.
- **Captain -> CaptainCrest**: guard posts, crest and centered core.

The four competitors in the current battle prototype use the first four distinct families. Street and Captain are already defined for roster/locker work.

## Competitive integrity

`PogoSilhouetteProfile` contains presentation proportions and accent budgets only. It deliberately has no speed, jump height, collision, hitbox, score, stun, duration, damage or range field.

Changing pogo cosmetics may alter:

- rendered foot width;
- rendered spring thickness;
- rendered handle silhouette;
- collider-free accent geometry;
- emissive decoration.

It must never alter:

- auto-bounce cadence;
- grid movement;
- collision resolution;
- power-up duration;
- player score;
- bot decisions;
- deterministic replay.

## Runtime prototype

`PogoDomPogoCosmeticDirector` waits for the existing avatar visual rig, finds each `PogoSocket` and applies a cosmetic decorator underneath the visual hierarchy.

The decorator stores the neutral pogo's original transform scales before applying a profile. Re-applying the same family is a no-op; changing family first restores the neutral proportions and removes the previous decoration, preventing cumulative scale drift.

All added primitives are collider-free through the existing avatar primitive helper.

## Automated gates

Headless tests certify that:

- exactly six launch pogo families exist;
- every family has a unique silhouette signature;
- visual proportions stay inside bounded phone-readable ranges;
- the profile type exposes no competitive-looking stat fields;
- the four live prototype player slots receive four different pogo families.

## Authored asset gate

Procedural shapes are scaffolding, not launch art. When final pogo models arrive, each model must preserve the family silhouette at actual phone camera distance and use the M0.33 `PogoSocket` contract. A beautiful pogo that becomes indistinguishable from another at gameplay scale fails the gate.
