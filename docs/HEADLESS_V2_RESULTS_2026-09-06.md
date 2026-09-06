# Headless v2 tuning results — 2026-09-06

Source: GitHub Actions `PogoDom Headless Certification`, deterministic seed `20260905`, 100 bot matches per experimental candidate. These numbers are engineering evidence only; they do not replace one-thumb Unity playtesting.

| Metric | Launch reference | Loop v1 | Loop v2 | Chaos v1 | Chaos v2 | Padlock v1 | Padlock v2 |
|---|---:|---:|---:|---:|---:|---:|---:|
| Invalid states | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| Avg winner score | 84.58 | 87.07 | 85.99 | 77.04 | 81.33 | 92.57 | 86.11 |
| Avg margin | 12.40 | 13.15 | 12.49 | 10.47 | 11.60 | 14.72 | 11.45 |
| Close finish rate (<=5 pts) | 32.4% | 23.0% | 35.0% | 36.0% | 27.0% | 24.0% | 30.0% |
| Blocked movement | 4.7% | 4.8% | 4.5% | 4.7% | 4.9% | 5.2% | 4.9% |
| Lead changes / match | 8.93 | 8.81 | 8.83 | 8.54 | 8.90 | 9.16 | 8.54 |
| Steals / match | 343.39 | 351.30 | 350.87 | 309.29 | 321.47 | 280.26 | 313.97 |
| Banks / match | 26.28 | 26.27 | 26.29 | 26.15 | 26.27 | 26.15 | 26.21 |
| Enclosures / match | 0 | 16.19 | 10.17 | 0 | 0 | 0 | 0 |
| Enclosed tiles / match | 0 | 26.45 | 16.28 | 0 | 0 | 0 | 0 |
| TNT blasts / match | 0 | 0 | 0 | 9.00 | 5.00 | 0 | 0 |
| TNT tiles cleared / match | 0 | 0 | 0 | 64.73 | 36.08 | 0 | 0 |
| Padlocks / match | 0 | 0 | 0 | 0 | 0 | 6.84 | 5.63 |
| Protected paint attempts / match | 0 | 0 | 0 | 0 | 0 | 12.88 | 6.33 |

## Director read

### Loop v2 is a materially better Unity candidate than Loop v1

Changing enclosure so it automatically fills **neutral interior only** preserved roughly ten area-capture moments per match while eliminating automatic theft of rival interior. In this deterministic bot sample, the close-finish rate rose from 23% to 35% and the average margin returned close to launch. This is exactly the intended direction: keep the spectacular fill without making the mechanic a snowball cannon.

Decision: keep both v1 and v2 frozen, but prioritize **Loop v2** in the first Unity A/B session.

### Chaos v2 solved frequency but may have over-corrected

V2 reduced TNT from 9 to 5 blasts and territory destruction from ~65 to ~36 tiles per match. That is much more plausible visually. However, v1's unusually high close-finish rate did not survive the lower cadence in this sample.

Decision: neither v1 nor v2 is a launch candidate yet. If we iterate before Unity, the next useful candidate should sit between them (roughly 6–7 meaningful blasts per 75-second match), not return to constant chaos.

### Padlock v2 is clearly healthier than v1

Shortening protection from 8s to 5s and spacing respawn from 9s to 12s nearly halved protected paint attempts (12.88 -> 6.33), restored much of the normal steal rhythm (280.26 -> 313.97), reduced average winning margin (14.72 -> 11.45), and improved close finishes (24% -> 30%).

Decision: prioritize **Padlock v2** over v1 if we test protection in Unity. It is still optional because any defensive mechanic must prove that players understand it instantly.

## Current first Unity battle shortlist

The first playtest should not throw every system onto the board at once. Recommended sequence:

1. `launch-m0-2` — control/reference.
2. `pogodom-loop-v2` — best current expansion candidate.
3. `pogodom-padlock-v2` — separate defensive A/B.
4. Chaos candidate only after the base feel is certified.

The core question remains behavioral, not mathematical: after a 75-second match, does the player immediately want another one?
