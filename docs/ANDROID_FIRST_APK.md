# POGODOM — First Android APK gate

POGODOM is pinned to the same Unity editor used by CIVIDOM:

- Unity Editor: `6000.3.23f1`
- Revision: `09d2ecc7fb28`
- Android output: development APK
- Orientation: portrait
- Application ID: `com.protectorrudo.pogodom`
- Minimum Android API: 24
- URP package: `17.3.0`

## Required Unity Hub modules

For Unity `6000.3.23f1`, install **Android Build Support** and keep its child modules enabled:

- Android SDK & NDK Tools
- OpenJDK

Do not point POGODOM at an arbitrary external SDK/NDK for the first gate. Use the versions installed by Unity Hub so editor/toolchain compatibility is controlled.

## First open

1. Clone or pull `ProtectorRudo/PogoDom`.
2. In Unity Hub use **Add > Add project from disk** and select the repository root (the folder containing `Assets`, `Packages` and `ProjectSettings`).
3. Open it specifically with Unity `6000.3.23f1`.
4. Let Package Manager finish resolving packages and let Unity finish the first script import.
5. If Console has red compile errors, stop and capture the **first** red error with its full stack/file/line. Do not randomly change packages or Project Settings.

## One-click APK

When the Console is clean:

1. Unity menu: `PogoDom > Build > 1 - Prepare Unity Project`.
2. Press Play once. The scene is generated at `Assets/PogoDom/Scenes/PogoDom.unity` and must show the playable prototype.
3. Stop Play mode.
4. Unity menu: `PogoDom > Build > 2 - Build Android APK (Development)`.
5. Expected output: `Builds/Android/PogoDom-development.apk`.

The build command automatically creates/enables the POGODOM bootstrap scene, configures product name, application identifier, portrait orientation and Android API floor. It intentionally does not configure signing or Google Play release settings yet.

## What this APK is for

This first APK is a **real-device truth gate**, not a store release. Evaluate:

- touch/swipe latency and missed directions;
- 0.5 s auto-bounce rhythm;
- portrait camera framing;
- board/pickup/player readability on a phone-sized display;
- visual glitches/magenta materials;
- frame pacing during capture/TNT/missile/celebration moments;
- whether the first 20 seconds explain themselves;
- whether the result screen makes immediate rematch desirable.

Do not tune game rules from screenshots alone. The purpose of this build is to turn the remaining Unity/runtime assumptions into observable evidence on an Android device.

## Batch equivalent

For later automation, the same build can be invoked with:

`Unity.exe -batchmode -quit -projectPath <repo> -executeMethod PogoDom.Editor.PogoDomAndroidBuild.BuildAndroidDevelopmentBatch -logFile -`

This still requires a machine with Unity `6000.3.23f1` and Android Build Support installed/licensed.
