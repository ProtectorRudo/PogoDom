# PogoDom adaptive rivals — M0.29

## Product goal

Bots must be **entertaining opponents**, not moving score generators and not hidden rubber-band cheats. The player should be able to read a personality after a few matches:

- **Aggressive** pressures the current leader's visible territory and values missiles more.
- **Greedy** contests visible pickups and hotspots.
- **Banker** becomes increasingly interested in converting an exposed paint run into safe score.
- **Balanced** switches jobs based on public match state.
- **Chaotic** accepts more hazard/collision risk and has more decision jitter.

The classic human match already uses complementary Greedy / Aggressive / Banker opponents, so M0.29 improves the behavior under those identities rather than adding three interchangeable difficulty clones.

## Fairness boundary

Adaptive rivals receive no information that a human player could not infer from the arena/HUD:

- current scores / leader;
- visible tile owners;
- visible item positions and item kind;
- player positions;
- visible hazard telegraphs;
- remaining match time.

They do **not** inspect a Mystery Crate's hidden payload, do not gain speed/damage/score multipliers, do not target somebody merely because `IsHuman == true`, and do not modify city/nation contribution.

Late-match adaptation is decision-only. A trailing rival pressures current leader territory; a leader with meaningful unbanked territory values the nearest bank more. There is no synthetic score catch-up.

## Repository transplant

The audited `bitsmag/squares` RL environment reinforces a useful principle: treat the board, own state, specials and score as the observation, then optimize choices from that observation. Its Gym environment exposes board ownership/special masks and agent position/direction/speed/score instead of magical hidden opponent data.

PogoDom already distilled that work into `HardBotBrain` local lookahead. For the normal three-rival match we intentionally do **not** run the hardest search policy everywhere. `RivalBotBrain` instead takes the observation/reward idea and combines it with our existing personality heuristics so normal opponents remain legible and fallible.

## Replay compatibility

Changing a bot decision changes the entire deterministic match. Therefore M0.29 does not replace legacy Medium logic in place.

- V1 rulesets -> original `MediumBotBrain`.
- V2 rulesets -> original `MediumBotBrain`.
- `pogodom-rivals-v1` -> `RulesetBehaviorVersion.V3` -> `RivalBotBrain`.

The rival ruleset is otherwise identical to `launch-m0-2`. This makes first human A/B testing clean: same map, items, timings and scores; only opponent decision semantics differ.

## Headless gate

CI now runs 100 complete `rivals` matches in addition to the launch baseline and existing variant labs. It rejects invalid states, dead gameplay loops, excessive movement blocking or a rival simulation with no leader changes.

Simulation can prove consistency and expose aggregate behavior. It cannot prove that being chased by the aggressive rival is fun. Final promotion still requires real one-thumb phone play.
