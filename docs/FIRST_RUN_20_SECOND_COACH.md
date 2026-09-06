# First-run: teach the game in under 20 seconds

PogoDom must be understandable almost immediately. The first match therefore teaches through play instead of stopping the player with a tutorial screen.

## Three-step coach

The local player sees at most three non-blocking hints:

1. **DESLIZÁ** — one finger changes direction; jumping is automatic.
2. **PINTÁ Y ROBÁ** — landing on tiles claims territory and can steal rival territory.
3. **BANKEÁ** — the purple bank crate converts held territory into score.

The coach advances from actual local-player `MatchEvent` values. Bot movement, bot painting and bot banking cannot advance the local tutorial.

## Hard product rules

- Maximum lifetime: **20 seconds**.
- `CoachDirective.IsBlocking` is always false.
- No modal tutorial scene.
- No extra gameplay buttons.
- No pause or `Time.timeScale` change.
- The overlay disappears immediately after the local player banks, or automatically when the 20-second cap is reached.
- Touch/swipe input continues underneath the overlay.

## Runtime integration

M0.39 adds a direct event-feed API to `FirstRunCoach`, allowing the already-isolated M0.36 presentation stream to drive the tutorial without exposing or mutating MatchRunner state.

`PogoDomFirstRunCoachOverlay` auto-installs beside the prototype root and renders a compact bottom card inside `Screen.safeArea`. Safe-area coordinates are converted correctly from Unity screen coordinates to IMGUI's top-left coordinate system.

Time advances with `Time.unscaledDeltaTime` so the 20-second comprehension budget remains wall-clock bounded even if a future presentation layer changes time scaling. The coach itself never changes time scaling.

## Automated gates

Headless tests certify that:

- the coach times out at 20 seconds;
- it never blocks;
- local move -> paint/steal -> bank progresses the three steps;
- the presentation event feed works without a `TickResult` wrapper;
- bot events cannot progress the local coach.

## Real-phone gate

Before calling first-run UX finished we still need to observe first-time players on real phones and answer one question:

> Can they explain "deslizo, pinto/robo y bankeo para sumar" after no more than 20 seconds, without us explaining it?

If not, copy, positioning or visual affordances must change — not the one-thumb control philosophy.
