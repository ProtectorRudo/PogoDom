# Avatar + skill visual contract

M0.32 hardens the visual system so PogoDom can support memorable, very different avatar silhouettes without letting cosmetics corrupt competitive rules or break on unusual rigs.

## Why this exists

PogoDom should be able to ship humanoid heroes, robots, mascots, blob-like characters and future experimental silhouettes while still selling/equipping pogos, headwear, back accessories, auras, trails, landing FX, skill visuals and victory emotes.

A cosmetic is not valid merely because the inventory owns it. It must also be compatible with the equipped avatar rig.

## Rig capabilities

Characters now declare an explicit `AvatarRigCapability` mask. Current capabilities are:

- `HeadwearSocket`
- `BackAccessorySocket`
- `AuraSocket`
- `Hands`
- `PogoHandleIk`
- `FullBodyEmote`
- `FaceExpression`
- `HumanoidRetargeting`

Two presets are included:

- `StandardHumanoid`: full capability set for premium humanoid avatars.
- `Mascot`: head/back/aura/full-body emote support without pretending it has humanoid hands, facial retargeting or handle IK.

This is intentionally capability-based instead of character-name-based. A future robot can support hands and IK without being a human; a circular mascot can reject a hand-dependent emote without needing special-case code.

## Cosmetics and skill visuals

`SimpleCosmeticDefinition` and `SkillVisualDefinition` can declare required rig capabilities. Existing cosmetics remain compatible by default because their requirement is `None`.

Examples:

- a normal aura can require only `AuraSocket`;
- a cap can require `HeadwearSocket`;
- a cape can require `BackAccessorySocket`;
- an Aura-67-like palms-up celebration can require `Hands | FullBodyEmote`;
- a hand-cast lightning missile visual can require `Hands`;
- a body-ring energy effect can require no hands and therefore work on mascots.

The equip service rejects incompatible items. If the player switches from a humanoid to a mascot, incompatible equipped presentation is removed automatically while compatible cosmetics (for example an aura) remain equipped.

## Event-to-VFX policy

`VisualEventPolicy` is the stable presentation translation for deterministic match events. It assigns a skill trigger, impact tier, lifetime, radius, camera impulse and small particle budget.

Three tiers exist:

- `Ambient`: tiny feedback such as a normal tile steal.
- `Readable`: clearly visible feedback such as crate open, speed, padlock or stun.
- `Spectacle`: short viral-worthy beats such as missile, big bank, enclosure or TNT blast.

Large values may make presentation stronger, but all budgets are clamped. A 10,000-tile synthetic event cannot request thousands of particles or permanent shake.

The policy is read-only. Nothing produced by it feeds back into movement, score, bots, RNG, city progression or signed replay.

## Mobile visual rule

PogoDom does not win by covering the screen with particles. The visual hierarchy remains:

1. next landing / territory ownership;
2. avatar silhouette;
3. important pickup or threat;
4. skill spectacle;
5. scenery.

A Mythic skill visual may look dramatically better than a Common one, but it still obeys the same gameplay footprint and event timing.

## Art pipeline consequence

Every final FBX/avatar prefab should be validated against its declared capabilities. If it says it supports `HeadwearSocket`, the corresponding socket must exist. If it claims `Hands` and `PogoHandleIk`, its rig must expose the required limbs/targets. Mascots should declare only what they genuinely provide.

This lets us make characters visually wild without building a fragile one-off cosmetic system per avatar.
