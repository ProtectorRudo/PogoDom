# M0.44 — Launch avatar asset budget

## Why

PogoDom needs memorable characters, attachments, skills and celebrations, but four characters are visible at once on a mobile arena. A beautiful single-character render is not the performance target; four simultaneous competitors plus pickups, territory feedback, VFX and screen recording is.

## Base avatar budget

The authored **base character prefab**, before external skill VFX, must stay within:

- 40,000 triangles;
- 3 SkinnedMeshRenderers;
- 10 material slots;
- 90 unique bones;
- 1 embedded ParticleSystem maximum;
- exactly one Animator maximum;
- 0 Lights;
- 0 Cloth components;
- 0 AudioSources.

These are launch guardrails, not an excuse to spend every budget on every character. Lighter is preferred when silhouette quality is unchanged.

## Why skills are separate

Skill VFX, trails, auras, landing effects and celebrations already have their own sockets/presentation budgets. Packing those systems inside every character prefab duplicates cost and makes cosmetics harder to swap. The validator therefore pushes these effects out of the base FBX/prefab and into PogoDom's existing cosmetic presentation layers.

## Validator upgrades

`PogoDom/Art/Validate Selected Avatar` now checks:

- required sockets are present **and unique**;
- no Collider/Rigidbody/Camera;
- triangle, skinned-renderer, material-slot and unique-bone budgets;
- particle/light/cloth/audio budgets;
- at most one Animator;
- Humanoid declaration and hand bones where required;
- `Animator.applyRootMotion == false` so cosmetics can never own gameplay motion;
- root transform warnings for non-neutral authored roots.

The validation report prints the key asset counts so art iteration can optimize deliberately instead of guessing.

## Real-device acceptance gate

Passing this validator is necessary, not sufficient. Final acceptance still requires four fully dressed characters on the target arena while recording video on representative Android hardware. We will profile CPU, GPU, memory, overdraw and frame pacing before increasing any budget.
