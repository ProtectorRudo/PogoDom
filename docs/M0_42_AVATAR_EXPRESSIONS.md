# M0.42 — Avatar expression reactions

PogoDom's avatars should not feel like colored chess pieces. Important battle events now have a short, readable character reaction layer.

## Event language

- Bank: excited; a large bank becomes proud.
- Enclosure capture: proud.
- Mystery crate open: surprised.
- Missile / Arrow: attacker gets an attack expression.
- Stun: the actual victim gets a hurt reaction.
- Padlock activation / blocked steal: the protected player gets a shielded reaction.
- Speed: excited.

Routine movement, paint and tile stealing intentionally do **not** animate the face. Those events happen too often; reacting to every tile would turn character personality into noise.

## Prototype face

The current procedural avatar fallback uses the existing eyes/pupils and adds lightweight brows plus a mouth marker. Reactions are envelope-driven and always return to neutral in <= 0.85 seconds. No pause, slow motion, input lock or gameplay-root motion is allowed.

This is intentionally an adapter layer. Authored FBX avatars can later implement the same expression cues using blendshapes, bones or texture swaps without changing Core or event semantics.

## Acceptance gate when Unity/mobile is available

- Expressions must still read when the avatar is small on a phone.
- Hurt/attack/shield must never obscure the board or pickup silhouettes.
- The face must return cleanly to neutral after interrupted/replaced reactions.
- Reactions should add personality to clips without becoming constant visual chatter.
- Future authored characters must consume the same `AvatarExpressionCue` contract.
