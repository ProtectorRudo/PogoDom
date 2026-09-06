# M0.9 — Padlock transplant without a second thumb

The audited `AgusCrow/pogo-pandemonium/PogoPadlock.tsx` contains a useful strategic idea: protected territory creates a short defensive window and changes where rivals want to move.

Its implementation is not suitable for PogoDom: it exposes an explicit **Use Padlock** action and a separate **Fire Missile** button, while its missile can recolor all unlocked rival tiles at once. That breaks our one-hand/no-button rule and creates an oversized swing.

## PogoDom adaptation

- Padlock is a board pickup.
- Landing on it activates protection automatically.
- Default duration is 8 seconds.
- Current unbanked territory cannot be repainted by rival landings, Arrow rays or enclosure capture while protection is active.
- Players still move through those cells normally; the lock changes ownership rules, not movement.
- Neutral arena TNT is intentionally not blocked: Padlock is PvP defense, not invulnerability.
- Banked score is unchanged and remains the victory metric.
- Medium bots recognize protected rival cells as low-value moves and can value the Padlock more when they hold lots of exposed territory.

## Experiment isolation

`pogodom-padlock-v1` freezes the mechanic for deterministic replay. The launch ruleset remains unchanged until actual Unity playtesting proves the mechanic improves clarity and rematch urge.
