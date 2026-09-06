# Event-driven skill VFX contract

PogoDom treats battle rules and battle presentation as separate systems.

## Source of truth

`MatchRunner` produces deterministic `MatchEvent` values. The Unity bootstrap emits those events only after the tick has already resolved movement, scoring, items, hazards and board state. Presentation subscribers are read-only consumers of the result.

A cosmetic or VFX failure must never abort the match. The runtime presentation feed isolates subscribers and logs their exception instead of allowing the visual layer to interrupt deterministic gameplay.

## Routing

The presentation path is:

`MatchEvent -> VisualEventPolicy -> SkillVisualResolver -> avatar/world socket -> pooled burst + optional camera impulse`

`VisualEventPolicy` owns the hard feedback budget. It decides whether an event is Ambient, Readable or Spectacle and caps particle count, radius, lifetime and camera impulse for mobile safety.

`SkillVisualResolver` only selects the equipped presentation for the stable event trigger. Exact power context wins over a generic visual, independent of equip order.

Cosmetic rarity may change authored appearance, but it does not increase the gameplay-derived VFX budget and never changes movement, score, RNG, collision, target selection, stun duration, territory or meta progress.

## Noise rules

Normal tile stealing is Ambient. The board already communicates ownership with tile pop/glow, so a legendary cosmetic does not turn every ordinary steal into a particle burst or camera shake.

Readable and Spectacle events may spawn pooled bursts. When the pool is saturated, routine feedback cannot evict an active Spectacle burst. A new Spectacle moment may replace a lower-impact burst.

The quality scaler can reduce decorative particle counts further for Lite and Balanced devices, but never scales gameplay objects or required hazard telegraphs away.

## Prototype loadouts

Until account inventory is connected, the Unity prototype seeds deterministic visual kits for the four competitors. This exists only to exercise the end-to-end pipeline. Production inventory can replace a player's loadout without modifying Core.

## Current automated gates

Headless tests certify that:

- real battle events map to stable visual triggers;
- large banks/captures receive bounded Spectacle feedback;
- exact crate power visuals beat generic crate visuals regardless of loadout order;
- cosmetic rarity cannot mutate event budgets;
- ordinary tile steals remain Ambient even with a Legendary visual;
- gameplay and cosmetics remain separated by the existing competitive-integrity suite.

## Real Unity boundary

Headless certification does not prove Unity particle rendering, shader compatibility, device frame time, camera feel or authored asset quality. Those remain real-Unity / real-phone gates.

Before calling this layer production-ready we still need to validate at minimum:

1. no missed or duplicated VFX under rapid event batches;
2. stable pool reuse during TNT/capture/late-game spectacle;
3. readable effects at 9:16 and landscape framing;
4. acceptable frame time on low/mid/high Android targets;
5. authored character/pogo assets using the M0.33 socket contract.
