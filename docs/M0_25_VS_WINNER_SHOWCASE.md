# M0.25 — VS / Winner Showcase

PogoDom is a four-player game, so pretending the match itself is 1v1 would be misleading. The post-match presentation can still create a strong **VS** story by focusing on the only rival that matters at the finish.

## Deterministic framing

`WinnerShowcaseResolver` consumes the same canonical `MatchOutcome.Standings` used by gameplay results.

- If the human wins, the focal rival is the runner-up: **you vs the player you just beat**.
- If the human loses, the focal rival is the winner: **you vs the player to beat next time**.
- The winning player owns the celebration cue.
- Human placement remains available so the other two players are not erased from the real result.

No new ranking logic exists in presentation. Ties inherit the exact score → unbanked territory → player-id ordering already defined by Core.

## First mobile timing hypothesis

The first headless presentation contract is intentionally short:

- 0.25 s: result freeze/read;
- 0.45 s: VS reveal may begin;
- 0.60 s: winner celebration may begin;
- 1.45 s: primary rematch CTA may appear;
- 2.40 s: target celebration window, matching the first Aura 67 choreography.

These are **not certified game-feel values**. They live in `Session`, not Core, specifically so Unity/mobile testing can tune them later without changing signed battle replay semantics.

## Why this pairs with M0.24

M0.24 created the cosmetic choreography engine and the first procedural viral celebration, `Aura 67`. M0.25 decides **who is visually framed, who celebrates, and who the human sees as the rival**. The actual equipped victory cosmetic remains a Cosmetics concern; Session never grants competitive power or selects paid content.
