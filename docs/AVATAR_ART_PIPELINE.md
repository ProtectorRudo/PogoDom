# PogoDom avatar art pipeline

M0.33 turns the avatar idea into an import contract. The goal is to let the art become much more ambitious without ever making cosmetics fragile or competitive hitboxes dishonest.

## Launch silhouette families

The first art set should deliberately cover six different silhouette families instead of six variations of the same little human:

| profile | visual read | dominant silhouette device |
| --- | --- | --- |
| `hero` | clean flagship competitor | top crest / visor |
| `trickster` | mischievous rival | strong side width / fins or hair |
| `tech` | futuristic competitor | tall antenna / top accent |
| `mascot` | cute non-humanoid icon | oversized head / ears / compact body |
| `street` | attitude / fashion | asymmetry / cap / one-sided accessory |
| `captain` | status / leadership | crown-like top shape / broad upper body |

The normalized profile numbers in `AvatarArchetypeProfiles` are art-direction targets, not gameplay dimensions. They exist to stop the launch roster from collapsing into six same-shaped characters with different colors.

## Required prefab structure

Every avatar declares capabilities. `AvatarPrefabContract` converts those capabilities into exact transform names.

Base sockets required for every avatar:

- `PogoSocket`
- `SkillSocket`
- `TrailSocket`
- `LandingFxSocket`
- `EmoteRoot`

Conditional sockets:

- `HeadwearSocket` when headwear is supported;
- `BackAccessorySocket` when back cosmetics are supported;
- `AuraSocket` when aura cosmetics are supported;
- `PogoGripTargetL` and `PogoGripTargetR` when hand-to-handle IK is supported.

The current procedural fallback from M0.31 was missing the explicit aura/grip contract even though the inventory already exposed those ideas. `PogoAvatarSocketRegistry` now creates those missing anchors for the generated M0 avatars. Final authored prefabs must contain them themselves.

## Unity validation gate

When Unity access returns, select the imported avatar root and use:

`PogoDom > Art > Validate Selected Avatar > Standard Humanoid`

or

`PogoDom > Art > Validate Selected Avatar > Mascot`

The validator checks:

- required named sockets;
- at least one renderer;
- no Colliders;
- no Rigidbodies;
- no embedded Camera;
- warns about embedded Lights;
- Humanoid Animator when humanoid retargeting/hands are declared;
- actual left/right hand bone mapping;
- non-uniform root scale warning.

A visual prefab is not allowed to carry collision/physics. Gameplay occupancy remains the deterministic grid cell from Core. This is how a huge mascot, a skinny robot and a crowned captain can look wildly different without gaining a larger or smaller gameplay hitbox.

## Skills and attachment freedom

The final character can be visually extreme because the loadout no longer assumes all avatars are human. Cosmetics declare what they need. Examples:

- crown -> `HeadwearSocket`;
- cape -> `BackAccessorySocket`;
- energy halo -> `AuraSocket`;
- palms-up viral emote -> `Hands | FullBodyEmote`;
- hand-cast lightning skill -> `Hands`;
- body shockwave -> no hand requirement and can work on a mascot.

The player keeps ownership of every cosmetic even if the currently equipped avatar cannot render it. Character swapping only removes incompatible items from the active loadout.

## Art quality target

At gameplay camera distance, silhouette wins over facial detail. The first question for every model is not “does the face look good close up?” but “can I tell which character this is in a 2-second phone clip?”

Each launch avatar should therefore pass these human checks:

1. recognizable from a solid-black silhouette at gameplay size;
2. recognizable without relying on player color;
3. pogo remains visible as a separate collectible object;
4. headwear and back attachment zones remain readable;
5. skill VFX can originate from body/hand/pogo without covering the next landing tile;
6. victory emote remains readable in roughly two seconds;
7. no cosmetic changes gameplay footprint.

GitHub can enforce the contract. Unity and a real phone still decide whether the finished model is beautiful enough to ship.
