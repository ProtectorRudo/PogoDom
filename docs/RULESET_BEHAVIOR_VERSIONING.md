# Ruleset behavior versioning

Freezing numeric `MatchConfig` values is necessary but not sufficient for deterministic historical replay. Shared code can change semantics even when every number is identical. Example: changing how an item spawn cell is selected could produce a different match for an old signed input log.

M0.20 adds a second axis: `RulesetBehaviorVersion`.

## Contract

Every published ruleset snapshots both:

1. its full numeric/feature configuration;
2. the shared algorithm behavior version.

All rulesets published so far are explicitly pinned to `V1`.

## Future rule

If a future change alters deterministic battle semantics in a shared resolver, spawner, collision rule, target selector, banking rule, RNG consumption order, or equivalent system:

- do **not** rewrite the V1 branch;
- add/use `RulesetBehaviorVersion.V2` (or later);
- branch the changed algorithm by behavior version where necessary;
- publish a new ruleset id;
- keep verification able to replay V1 input logs forever.

Presentation-only changes, cosmetics and Unity interpolation do not require a behavior bump because they do not participate in verified battle state.

The reflection freeze test continues to ensure every public `MatchConfig` property is captured by `RulesetDefinition`, so the behavior version itself cannot be accidentally omitted from signed replay reconstruction.
