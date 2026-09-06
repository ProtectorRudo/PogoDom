# Headless baseline — 2026-09-06

Source: GitHub Actions `PogoDom Headless Certification`, deterministic seed `20260905`.

These are engineering signals, **not proof of fun**. Unity playtesting remains the only gate for clarity, feel and rematch urge.

| Metric | Launch (250) | Loop (100) | Chaos (100) | Padlock (100) | Fusion (100) |
|---|---:|---:|---:|---:|---:|
| Invalid states | 0 | 0 | 0 | 0 | 0 |
| Avg winner score | 84.58 | 87.07 | 77.04 | 92.57 | 88.45 |
| Avg margin | 12.40 | 13.15 | 10.47 | 14.72 | 15.11 |
| Close finish rate (<=5 pts) | 32.4% | 23.0% | 36.0% | 24.0% | 25.0% |
| Blocked movement | 4.7% | 4.8% | 4.7% | 5.2% | 4.9% |
| Lead changes / match | 8.93 | 8.81 | 8.54 | 9.16 | 8.91 |
| Steals / match | 343.39 | 351.30 | 309.29 | 280.26 | 257.45 |
| Banks / match | 26.28 | 26.27 | 26.15 | 26.15 | 26.13 |
| Arrows / match | 9.26 | 9.20 | 9.32 | 9.01 | 9.15 |
| Speeds / match | 8.00 | 8.08 | 7.86 | 7.69 | 7.63 |
| Missiles / match | 7.76 | 7.68 | 7.71 | 7.68 | 7.64 |
| Enclosures / match | 0 | 16.19 | 0 | 0 | 13.74 |
| Enclosed tiles / match | 0 | 26.45 | 0 | 0 | 22.99 |
| TNT warnings / match | 0 | 0 | 9.00 | 0 | 9.00 |
| TNT blasts / match | 0 | 0 | 9.00 | 0 | 9.00 |
| TNT tiles cleared / match | 0 | 0 | 64.73 | 0 | 66.46 |
| Padlocks / match | 0 | 0 | 0 | 6.84 | 6.87 |
| Protected paint attempts / match | 0 | 0 | 0 | 12.88 | 11.71 |

## Current director interpretation

### Launch

Remains the conservative reference. It is stable and highly interactive, with a low blocked-movement ratio and frequent territorial exchange.

### Loop v1

The mechanic works and creates many automatic area-fill moments, but the current sample produced fewer close finishes and a slightly larger winning margin than launch. It stays experimental. A future v2 should investigate reducing snowball, for example by filling neutral interior without automatically stealing rival interior.

### Chaos v1

The most interesting headless signal so far: it produced a smaller average margin and a higher close-finish rate than launch. However, nine blasts and roughly 65 cleared tiles per 75-second match may be visually/gameplay excessive. Do not promote v1. A lower-frequency chaos v2 is a strong future A/B candidate.

### Padlock v1

The protection system works and bots interact with it, but it sharply reduces steals and the sample became less close. Eight seconds may be too defensive. Keep v1 frozen for comparison; future tuning should test shorter and/or rarer protection rather than silently changing v1.

### Fusion

All current experimental systems compose without invalid states, which is valuable engineering evidence. The larger average margin means "more mechanics" is not automatically "better game". Fusion is a stress target, not a launch recommendation.

## Decision

No experimental ruleset is promoted to launch from headless numbers alone. The next tuning work can create new versioned candidates while preserving every v1 unchanged. Actual promotion requires one-hand Unity playtesting against the product pillars: understood in <20s, instant rematch desire, visual clarity, competitive bots and zero friction.
