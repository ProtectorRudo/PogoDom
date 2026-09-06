# Competitive combat readability

PogoDom is intentionally fast, but fast must not mean confusing. A player should understand **who did what to whom** in well under a second without reading text.

## Essential cause/effect cues

Three interactions receive explicit target readability in M0.37:

- **Missile fired**: a short curved trace connects the attacker to the Core-selected target.
- **Player stunned**: the stunned player receives a strong impact ring.
- **Padlock block**: the defending player receives a cyan protection ring, not the thief.

The source/target meaning is derived only from already-resolved `MatchEvent` fields. Presentation never re-selects a missile target and never infers a different winner/defender.

## Timing contract

Combat readability effects are intentionally short (<= 0.40 s). They may overlap normal auto-bounce gameplay, but they never pause, slow or block input and never change `Time.timeScale`.

## Allocation contract

Missile traces are pooled and capped at four concurrent instances for the four-player launch arena. If that pool is saturated, the trace closest to expiry is reused. This can change only what is drawn, never Core targeting, stun state or score.

Target rings are one reusable component per player root. Repeated impacts refresh that player's ring instead of continuously allocating new objects.

## Relationship to cosmetic VFX

M0.36 handles expressive cosmetic spectacle. M0.37 handles **essential competitive readability**.

A Legendary skin may make a missile look more memorable, but it must not remove or reverse the attacker-to-target relationship. Lite quality may reduce decorative spectacle; it must not delete the essential missile/stun/shield cue.

## Automated gates

Headless tests certify:

- missile source and target match Core event semantics;
- stun events reverse the Core fields correctly (`PlayerId` = stunned target, `SecondaryPlayerId` = attacker);
- padlock feedback highlights the defender, not the thief;
- invalid/self-targeted events produce no cue;
- readability cues remain short and non-blocking.

## Real Unity boundary

The following still require Unity/phone validation:

- line shader visibility under the selected render pipeline;
- readability on 9:16 phones at actual arena camera distance;
- overlap with M0.36 skill bursts and trails;
- color/contrast accessibility;
- frame time while several effects fire in the same tick.
