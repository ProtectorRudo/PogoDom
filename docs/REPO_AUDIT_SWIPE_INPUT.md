# Repo audit transplant — one-thumb swipe input

## Source behavior audited

`Alekssasho/PogoPainter` uses a simple and effective mobile contract:

- touch starts anywhere on screen;
- a drag shorter than roughly 50 px is ignored;
- on release, the drag angle maps to one cardinal direction;
- the latest direction persists and the game server samples it on the next 0.5 s tick.

That simplicity is worth keeping. A joystick or separate action buttons would add friction without adding strategic depth.

## PogoDom improvement

The previous Unity wrapper was already a faithful port but waited until finger-up before changing `CurrentDirection`. On a 0.5 s auto-bounce cadence, a player can visibly cross the intended threshold before a tick and still miss that bounce simply because the finger has not been lifted yet.

M0.17 keeps the same gesture and threshold but commits once **as soon as the threshold is crossed**:

1. press anywhere;
2. move at least 45 px;
3. dominant axis resolves to Up/Right/Down/Left immediately;
4. that gesture cannot emit a second direction;
5. release rearms the recognizer;
6. the chosen direction persists until another swipe/keyboard direction replaces it.

Exact 45-degree ties resolve horizontally, matching the audited Cocos angle boundary.

## Architecture

`SwipeGestureTracker` lives in the engine-independent Core so its semantics are headless-testable and deterministic. `SwipeDirectionInput` is only a Unity adapter for touch, mouse and keyboard.

No input RNG, frame-dependent gameplay state or network call is introduced. The match still samples one persisted direction at each deterministic battle tick.
