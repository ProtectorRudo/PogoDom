# M0.23 — One-tap rematch and deterministic A/B replay

The first Unity matrix is only useful if a tester can play several consecutive matches without reopening the scene or manually reconstructing state.

This milestone adds a lightweight prototype loop:

- after a match, the HUD shows the local player's final placement;
- **REMATCH** immediately rebuilds battle state in-place and advances to a deterministic new seed;
- **REPLAY SAME SEED** rebuilds with the exact same seed, allowing Crates V1 and Crates V2 (or any other pair) to be compared under the same random starting conditions;
- changing `playtestMode` in the Inspector and choosing same-seed replay applies the new profile while preserving the seed;
- the board/player GameObjects are reused, while transient item/hazard views are cleared and rebuilt;
- swipe direction resets to Up and any partially tracked gesture is cancelled, preventing a previous match's input from leaking into the next one.

`PlaytestSeedSequence` lives in engine-independent Core and is tested for deterministic output and no repeats over the first 10,000 rematches. Runtime integration remains a Unity prototype surface; headless CI proves the Core contract but does not replace an actual Unity compilation/play session.
