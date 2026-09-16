# Pool Pocket Shot

3D US 8-ball / 9-ball for Android. Unity Personal (0 €). No paid assets.

Working title — we can change the final name later.

## Play in the editor

1. Install **Unity Hub** (free). A copy is also installed via `Tools/Install-UnityHub.ps1`.
2. In Hub, create a **Personal** account. Do not choose Pro.
3. Install **Unity 6 LTS (6000.0)** with modules:
   - Android Build Support
   - OpenJDK
   - Android SDK & NDK Tools
4. Hub → Open → this project folder.
5. Open `Assets/Scenes/Main.unity` and press Play.

If Unity asks for money for the editor or Android module, stop and we switch to Godot.

## Controls

- Drag on the table: aim
- Power slider: shot strength
- White pad: english (spin)
- **SHOOT**
- Two-finger pinch: zoom / pan
- After a scratch: drag the cue ball, then **PLACE CUE**

## Android APK (sideload, 0 €)

1. USB debugging on the phone, or copy the APK.
2. In Unity: **Pool Pocket Shot → Build Android APK**
3. Output: `Builds/PoolPocketShot.apk`

## Modes

- 8-ball pass-and-play
- 8-ball vs CPU (easy / hard)
- 9-ball pass-and-play
- 9-ball vs CPU

Online multiplayer is intentionally not included.
