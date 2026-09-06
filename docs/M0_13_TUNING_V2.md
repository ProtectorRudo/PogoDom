# M0.13 — Versioned tuning candidates from the first headless baseline

The first A/B lab made three problems visible enough to justify new candidates. V1 rules remain immutable.

## Loop v2 — preserve the spectacular fill, reduce snowball

Loop v1 captured every unprotected tile inside a closed region, including rival territory. The repo inspirations used even more aggressive/fragile enclosure behavior. PogoDom v2 keeps the robust outside-in geometry but changes the reward: **automatic enclosure fills neutral interior only**. Rival tiles still require direct landing/Arrow pressure.

Hypothesis: retain the instantly understandable "I closed it and it filled" viral moment while reducing large automatic steals and winner-margin expansion.

## Chaos v2 — less confetti, more event

Chaos v1 generated about nine TNT blasts per 75-second headless match. V2 keeps the same telegraph, 3x3 blast and one-bounce stun but changes cadence:

- first warning: 10 seconds;
- subsequent warnings: at most every 15 seconds.

Hypothesis: each TNT becomes a meaningful route decision rather than background noise, while preserving the close-finish benefit observed in v1.

## Padlock v2 — shorter defensive interruption

Padlock v1 protected territory for 8 seconds and respawned after 9 seconds. It worked but reduced territorial stealing strongly. V2 changes only defensive availability:

- duration: 5 seconds;
- respawn delay: 12 seconds.

Hypothesis: keep a readable clutch-defense moment without suppressing the game's core paint/steal rhythm.

## Promotion rule

V2 candidates are new ruleset IDs. V1 is never edited. Headless metrics may reject obviously poor candidates, but only actual one-thumb Unity playtesting can promote a candidate to launch.
