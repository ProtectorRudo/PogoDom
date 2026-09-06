# PogoDom viral visual target

## Current assessment

The M0 greybox is not a viral visual target. Its battle rules are readable, but raw cubes and capsule players are only validation geometry. PogoDom must remain instantly readable while looking distinctive enough that a 2–5 second vertical clip can be recognized without a logo.

M0.31 establishes the first executable visual direction without changing a single competitive rule.

## Arena: toy-sport city stage, not a cluttered city map

The battle board remains the hero. The city/nation meta lives outside the match. During a match, urban identity is expressed around the board rather than placed on top of gameplay cells.

Visual hierarchy:

1. saturated territory tiles;
2. four highly readable avatars and their pogo silhouettes;
3. power crates / hazards;
4. dark arena frame and rails;
5. low-contrast city silhouette outside the playable grid.

The implementation adds layered tiles, a dark inset plinth, rails, four player-colored corner beacons and a low-poly city backdrop behind the arena. Tile ownership changes produce a fast scale/emission pop. Four or more tile changes in one presentation frame can produce a small camera impulse, making arrows, captures and destructive moments feel stronger without affecting simulation.

## Avatar target

An avatar must be recognizable at gameplay camera distance from silhouette before facial detail. Every avatar presentation rig therefore exposes independent sockets for:

- character body / skin;
- pogo;
- headwear;
- back accessory;
- aura;
- trail;
- landing FX;
- skill visual;
- victory/emote root.

M0.31 replaces the capsule-only look with a procedural toy-like avatar fallback: oversized expressive head, visible eyes/hands, outfit color, pogo hardware, contact shadow, short trail, squash/stretch, travel tilt and pogo compression. Four prototype silhouettes prove that one rig presentation can support a hero/visor shape, side-fin rival, antenna/future shape and mascot/ear shape.

These procedural bodies are not final shipped character art. Their purpose is to make the Unity playtest visually meaningful before final FBX characters arrive and to define the sockets those characters must expose.

## Skills and cosmetics

Characters can now equip these presentation slots independently: `Headwear`, `BackAccessory` and `Aura`, in addition to existing character, pogo, trail, landing FX, victory emote and trigger/context-specific `SkillVisual` slots.

Skill visuals remain presentation-only. A legendary missile skin may look like a dragon or electric comet, but it cannot modify target choice, damage, score, speed, stun duration or any other battle rule. The existing deterministic skill-visual resolver remains the authority for which cosmetic is selected.

`PogoProceduralSkillSocket` is the fallback when an authored VFX prefab is not yet available. It creates an animated skill core/orbit pattern and can remap its style from the equipped `SkillVisualDefinition`. Authored effects can later replace the fallback without changing inventory or gameplay contracts.

## Rendering language

`PogoDom/ToonLit` is a compact URP toon shader with:

- two-band readable lighting;
- colored shadow tone;
- restrained rim light;
- optional emissive color;
- optional inverted-hull outline.

Outlines are intentionally targeted at avatars and important pickups, not every tile/environment object. The arena already has geometric separation between tiles; outlining the entire board would waste mobile fill/draw calls and make the image noisy.

The runtime toon upgrader preserves existing colors, then gives avatars a stronger rim/outline and pickups a smaller outline. If the shader is unavailable, PogoDom keeps the normal URP/Standard fallback instead of breaking the match.

## Game-feel rules

Presentation may exaggerate what happened; it may never invent what happened.

Allowed:
- squash/stretch;
- pogo spring compression;
- tile pop;
- short trails;
- glow/rim;
- micro camera impulse after a large visible board event;
- cosmetic skill VFX;
- victory celebration.

Forbidden:
- camera motion that prevents reading the next landing cell;
- persistent screen shake;
- VFX covering destination tiles;
- a cosmetic suggesting a larger gameplay hitbox;
- a paid cosmetic changing battle state;
- city props covering the grid.

## Viral visual gates for the first Unity review

A build does not pass visually just because it looks better than the greybox. It should satisfy all of these during human review:

- In a paused screenshot, a new viewer can identify the four competitors and territory ownership immediately.
- At normal phone size, every avatar has a distinct silhouette.
- A tile steal is visually obvious without reading text.
- A big capture / arrow / crate / missile moment looks strong enough to survive a 9:16 crop.
- Important FX finish before they obscure the next decision.
- The arena feels premium while the grid stays more visually important than scenery.
- Cosmetics and skills are visible but never make a player look mechanically larger or faster than they are.
- A winner celebration is readable in roughly 2 seconds and REMATCH remains the dominant next action.

The final judge is still a real phone playtest. GitHub can certify architecture and competitive integrity, not taste.
