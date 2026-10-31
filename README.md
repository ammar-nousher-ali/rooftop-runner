# Rooftop Runner — playable Unity MVP

A single-scene, landscape 2D Android runner built with Unity **6000.6.7f1**, C#, Unity UI and the installed Input System. Uses an original AI-assisted sunset city art set and an eight-pose animated runner; no accounts, services, ads or purchases.

## Play in the Editor

1. Open this folder in Unity Hub with **6000.6.7f1** (Apple Silicon editor on an M1 Mac).
2. Allow the import and script compilation to finish.
3. Open `Assets/RooftopRunner/Scenes/RooftopRunner.unity`. Set the Game view to **1280 × 720** or another landscape aspect ratio.
4. Press Play, then **START RUN**.
5. Hold **Space**, or hold the on-screen jump button with the mouse; release to jump. On Android, use the same button with a finger. Longer holds produce stronger jumps, capped at 0.65 seconds. Charging and jumping are only accepted while grounded. Begin charging before the roof edge and release before running off it.
6. Use **PAUSE**, **P**, or **Escape** to pause/resume. Switching away from the application also pauses the game; resume explicitly on return. Pause cancels a held charge. Dragging outside the jump button cancels that touch charge.

The character runs automatically. Distance is measured from the starting position. Falling below the configurable fall height ends the run. Restart returns to the same seeded route. The best distance is stored using `PlayerPrefs` on game over, pause and application exit.

## Project structure and tuning

- `Assets/RooftopRunner/Scenes/RooftopRunner.unity`: ready-to-play scene, camera, city, game controller, generator, UI and input event system.
- `Assets/RooftopRunner/Prefabs/Runner.prefab`: collider, Rigidbody2D, animated teal-jacket character and movement settings. Select its RunnerAnimator component to adjust animation speed and visual size.
- `Assets/RooftopRunner/Prefabs/Rooftop.prefab`: solid roof collider, rim trim and RooftopDress component, which builds three facade styles and eight types of visual-only props.
- `Assets/RooftopRunner/Scripts/`: runtime gameplay; `SmokeChecks.cs` is Editor-only validation and is excluded from Android players.
- `Assets/RooftopRunner/Editor/RooftopRunnerSetup.cs`: reproducible scene/prefab generation and APK build command.
- `Assets/RooftopRunner/Art/`: original PNG art sheets, a SunsetArt asset with sprite regions, and generated UI shapes. See ART_DIRECTION.md for the prompts and asset details.

Select **Runner** in the scene to adjust speed, minimum/maximum jump velocity, charge time, gravity and ground layer. Select **Rooftops** to adjust width, gaps, height limits, generation distance and route seed. Select **Main Camera** to adjust view size, follow offset and damping. Select **Game** to change the fall threshold. Keep the fall threshold below the lowest roof.

The generator calculates the descending flight distance to each next roof using the configured full jump. It limits upward steps and reserves 2.1 world units for takeoff/landing margin. Roof widths allow time to recharge and contain a full jump. Requested gaps are clamped when they exceed the reachable distance. Default settings are the tested configuration; substantial physics or camera changes should be playtested again. Old roofs are removed behind the player to bound the number of objects.

The HUD adapts to the device safe area. UI objects and references are saved in the scene and editable in the Inspector. The skyline and roofs are created at runtime.

**Rooftop Runner → Create or Rebuild MVP Scene** recreates the supplied scene/prefabs and Android settings. It overwrites those generated assets, so save any custom versions under other names before using it. Rebuilding is not necessary to play the supplied scene. The original template SampleScene is retained but excluded from the build.

## Android APK on an M1 MacBook

1. In **Unity Hub → Installs → 6000.6.7f1 → Add modules**, install **Android Build Support**, including **Android SDK & NDK Tools** and **OpenJDK**. Use the versions bundled with this exact Unity editor rather than a separate Homebrew Java/NDK installation.
2. In Unity's **Settings/Preferences → External Tools → Android**, use the tools installed with Unity.
3. Open **File → Build Profiles**, select/add **Android**, and **Switch Platform**. Let Unity finish reimporting.
4. Check Player Settings: package ID `com.rooftoprunner.game`, landscape-left orientation, **IL2CPP**, **ARM64**, minimum API **26**, and automatic/highest-installed target API. The setup has already saved these settings. Change the package ID if you intend to distribute under your own identity.
5. Ensure `Assets/RooftopRunner/Scenes/RooftopRunner.unity` is the enabled build scene. Keep **Build App Bundle (Google Play)** off for an APK.
6. Choose **Rooftop Runner → Build Android APK**. Output: `Builds/Android/RooftopRunner.apk`. Alternatively use **Build** in Build Profiles and choose an APK path.
7. For **Build and Run**, connect an ARM64 Android phone by USB, enable Developer Options and USB debugging, and accept the device's authorization prompt. Select it as the run device. A local test build uses Unity's default debug signing; no custom keystore is required for testing.

This Mac's Unity installation was inspected and has Mac/WebGL modules but **no Android Build Support**. An APK has not been built. Android SDK downloads, module installation and device authorization remain manual steps.

Unity's official dependency instructions: https://docs.unity.com/en-us/engine/6000.5/manual/platform-specific/android/getting-started/sdksetup/install-dependencies

## Validation and manual checks

See `TESTING.md` for executed checks and remaining device checks.

The MVP has painterly sunset scenery, three facade styles, eight decorative props, a four-frame run cycle and charge/jump/fall/landing poses. The skyline texture is reused in two parallax layers. Audio remains outside this art milestone; no audio is included. Long runs use normal Unity world coordinates; floating-origin support and production profiling are outside this MVP.

## Sunset art milestone

The updated scene is the same `RooftopRunner.unity`; open it after Unity finishes importing. The HUD now follows the concept layout: title and distance at upper left, best at upper center, pause at upper right, jump power at lower left and a large jump control at lower right.

`Assets/RooftopRunner/Art/Sunset/SunsetArt.asset` holds the sky, city texture and individual sprite regions from the sheets. `RunnerAnimator` selects poses using grounded state, charge and vertical velocity. `RooftopDress` adds non-colliding props and facade modules, while `CityBackdrop` handles the three scenery layers.

Source sheets are retained. `runner-clean.png` is the character sheet used in-game; `runner.png` is the first source version. Texture CPU readability is disabled after import to avoid retaining extra runtime copies. Image generation prompts and production limitations are documented in `ART_DIRECTION.md`.
