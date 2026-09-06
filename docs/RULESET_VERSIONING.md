# Ruleset versioning is gameplay history

A signed offline ticket binds a battle to a ruleset id. Therefore a ruleset id must be immutable: `launch-m0-2` played next year must replay exactly as `launch-m0-2` plays today.

Previously the registry built old ids from `new MatchConfig()` and toggled one feature. That was convenient but unsafe: a future tuning change to a default could silently change historical replays while keeping the same ruleset id.

PogoDom now uses explicit versioned presets. Every value is written into `RulesetPresets`, and CI uses reflection to fail if a new public `MatchConfig` property is added without being represented in `RulesetDefinition`.

Rules for future development:

1. Never edit a shipped preset to rebalance it.
2. Create a new id (`...-v2`) for any behavior/timing/count change.
3. Keep old definitions available while tickets/replays may reference them.
4. Experimental presets may be deleted only before they have been issued outside development.
5. The server derives results from the ruleset snapshot; clients never declare authoritative score.
