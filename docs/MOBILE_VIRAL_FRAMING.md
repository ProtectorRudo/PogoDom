# Mobile + viral camera framing

M0.34 removes the old assumption that a fixed landscape greybox camera is good enough. PogoDom must remain readable on the device it is built for and must also survive the 9:16 crop used by Shorts, Reels and TikTok.

## Rule

The arena is always more important than scenery. The camera may reveal more city backdrop when space allows, but it may never crop a legal landing tile to make the shot look cinematic.

`ViralCameraFramingPolicy` is pure math. Given board width/depth and device aspect ratio it computes a conservative camera distance from a frozen field-of-view/pitch pair. Portrait uses a smaller safe-screen fraction than landscape so there is breathing room for the HUD, outlines, trails and winner/readability elements.

Current baseline:

- gameplay vertical FOV: 39 degrees;
- pitch: 48 degrees;
- portrait safe width: 88%;
- portrait safe height: 74%;
- landscape safe width: 90%;
- landscape safe height: 82%.

These are playtest defaults, not sacred art constants. They can be tuned after a real-device review, but the policy/invariants remain.

## Runtime

`PogoDomAdaptiveCameraFraming` finds the actual generated tile bounds, asks the pure policy for a fit and moves the camera before the visual director installs micro camera juice. This means the existing shake/impact system inherits the correct portrait-safe base pose instead of snapping back to the original fixed greybox position.

The camera keeps perspective rather than switching to orthographic. The board therefore preserves the toy-sport 3D depth introduced in M0.31 while gaining reliable small-screen composition.

## Viral composition gates

A 9:16 gameplay shot passes only when:

1. all playable cells remain inside frame;
2. all four competitors can be identified without the HUD;
3. the active pickup/threat layer remains inside frame;
4. a normal trail or outline cannot be clipped by routine movement;
5. top/bottom UI does not cover the next landing decision;
6. city scenery remains secondary and may be cropped first;
7. camera impulse is relative to the fitted base pose and never becomes a permanent offset.

## What GitHub can certify

Headless tests prove that portrait never asks for a closer fit than landscape for the same arena, larger boards cannot move the camera closer, taller presentation actors cannot move it closer, and invalid geometry is rejected.

The Unity component and the final visual composition still require Unity/device validation. M0.34 is the implementation and guardrail, not a claim that we have already seen the final phone image.
