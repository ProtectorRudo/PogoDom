# M0.43 — Viral visual performance guardrails

## Goal

A visually strong mobile game is useless if recording a clip causes frame-time spikes. This milestone attacks a concrete source of waste in the prototype: equivalent toon renderers previously received separate runtime Material instances.

## Material pooling

`PogoDomToonUpgrade` now uses a material signature and reuses one `PogoDom/ToonLit` material for renderers with the same visual parameters. It works through `sharedMaterial` rather than `renderer.material`, avoiding an unnecessary per-renderer material clone.

The pool key includes quantized base color, emission, rim strength and outline width. A periodic low-frequency rescan catches late avatar attachments, rebuilt skill visuals and newly spawned pickups.

### Hard safety boundary

Material pooling must never merge visuals whose color changes independently.

`VisualRuntimeBudgetPolicy` therefore rejects board `Tile_*`, tile glow, TNT telegraphs, skill bursts and combat trace/ring state from static material sharing. The base 8x8 territory grid keeps fully independent owner coloring.

## Runtime visual audit

`PogoDomVisualBudgetAuditor` measures the presentation hierarchy every five seconds and reports budget overruns for:

- unique pooled toon materials;
- mesh renderers;
- line renderers;
- trails;
- particle systems.

Budgets scale with Lite / Balanced / Showcase. They are development alarms, not gameplay switches: the auditor never removes players, board state, pickups or hazard telegraphs.

## Presentation determinism

`VisualRuntimeBudgetPolicy.StablePresentationHash` adds a process-independent FNV-1a hash for deterministic visual phase offsets. Presentation code that must reproduce the same clip across processes must use this instead of `string.GetHashCode`, whose value is not a replay contract.

## Source audit

PogoDom's toon direction continues to be informed by the small URP toon example in `ColinLeung-NiloCat/UnityURPToonLitShaderExample`. The audited tutorial repository explicitly states that the example is MIT licensed. PogoDom keeps its own compact shader/runtime layer rather than importing the commercial NiloToon package.

## Real-device acceptance gate

This work is structurally useful before Unity is available, but performance is not certified until a real phone profile proves:

1. stable frame pacing while four avatars, trails and pickups are visible;
2. no burst of material allocations when powers spawn;
3. no visual regression in independent tile ownership colors;
4. acceptable GPU cost for outlines/toon passes while screen recording;
5. Lite mode remains readable rather than merely cheaper.
