# Validation — 31 October 2026

Validated with the installed **Unity 6000.6.7f1** on this Mac, using a temporary project copy so the open working project was not taken over.

## Executed and passed

- Unity imported and compiled the runtime and Editor scripts successfully.
- The scene builder saved `RooftopRunner.unity`, both prefabs, sprites, wired UI references, build scene list and Android player settings.
- Play Mode smoke checks verified the start state and menu visibility, grounded detection, automatic running and distance scoring.
- A short charge produced upward velocity; a longer charge produced greater upward velocity; charge clamped at its limit; airborne charging was rejected.
- Pause froze Rigidbody2D position, resume restored simulation, and pause cancelled an active charge.
- Moving below the fall threshold triggered game over. Best distance was written to local preferences. Restart reset the score and rebuilt the route.
- Generated roof geometry was audited across **2.5 km**, checking each gap against the configured ballistic reach with a 2.1-unit margin. The active roof count stayed bounded.
- A physics-driven test runner travelled **300 metres** with **at least ten charged jumps**, using normal movement, collisions and jump methods.
- Menu and gameplay were rendered at **1280 × 720** and visually inspected. The offscreen capture uses a camera canvas; the shipped game uses a screen-overlay canvas.

Evidence: `Validation/smoke-results.txt`, `Validation/menu.png`, and `Validation/gameplay.png`.

The first pause assertion compared the interpolated display transform, which can settle by a fraction of a frame after pausing. It was corrected to check the authoritative physics position. The final smoke run passed. The temporary-project runs also logged an internal Unity SearchDatabase indexing exception during Editor startup; its stack trace is in Unity Editor code. It did not prevent the scene generation or gameplay checks from completing. No game-code exceptions were reported in the passing runs.

## Repeat the automated checks

With the project closed in other Unity instances, run the command below in Terminal. It enters Play Mode and exits Unity with code 0 on success or 1 on an assertion failure. Do not add `-quit`, because the checks must run across frames. The test restores the previous saved best score after successful completion. Use a project copy for automated checks.

```sh
"/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -nographics \
  -projectPath "/absolute/path/to/project-copy" \
  -executeMethod RooftopRunner.SmokeChecks.Run \
  -logFile "/tmp/rooftop-smoke.log"
```

The test runner is wrapped in `UNITY_EDITOR` and is not included in an Android player. Tests invoke gameplay methods; they do not emulate physical finger or keyboard events.

## Remaining manual checks

- Open the supplied scene and play with Space and mouse. Check short, medium and full holds, late release at the edge, and attempted midair jumps.
- Install Android Build Support, SDK/NDK and OpenJDK via Unity Hub, then build the APK using the README instructions. **No Android APK has been produced or installed yet.**
- On an ARM64 Android device: test touch hold/release, dragging off the jump button, extra fingers, pause while charging, background/resume, screen lock/unlock, and restarting after falling.
- Check HUD and controls at 16:9 and wider phone ratios, including a notched device. Safe-area adaptation is implemented but has not been tested on a physical phone.
- Confirm the best score survives fully closing and relaunching the installed app. Preference writing was automated; a device relaunch was not.
- Play several minutes on the target device to assess responsiveness, jump timing and frame rate. No hardware performance or battery profiling has been done.

## Sunset art milestone verification

Rebuilt and tested the art update using Unity 6000.6.7f1. The final run exited successfully with `ROOFTOP_SMOKE_OK`. All earlier gameplay checks passed again, including the 300 m physics traversal and 2.5 km geometry audit. Additional checks verified the runner animation has a valid sprite and that dressed rooftops instantiate.

Inspected rendered start-menu, gameplay and airborne-jump previews at 1280 × 720. Fixed the scene builder's asset-loading order, corrected sprite sheet regions, set UI shapes to single-sprite import, and refreshed background sizing during offscreen capture. A previous capture attempt reported a disk-full write error; the final captures and complete test run succeeded. The internal Unity SearchDatabase startup exception remains unrelated to game code.

Evidence: `Validation/sunset-smoke-results.txt`, `Validation/sunset-menu.png`, `Validation/sunset-gameplay.png`, `Validation/sunset-jump.png`. The older images remain as the initial placeholder MVP record.

The artwork is an MVP art pass. Character animation uses four run frames plus four state poses; it has not received production animation cleanup. The skyline is reused for the distant and near layers. Audio is not included. Android module installation, an APK build and physical-device input/performance testing are still pending, as described above.

## Android APK with explicit game-over message

Built `Builds/Android/RooftopRunner-GameOver.apk` successfully with Unity 6000.6.7f1, IL2CPP and ARM64. Build report: zero errors, one warning. Android's apksigner verified the APK signature. Manifest inspection confirmed `com.rooftoprunner.game`, minimum API 26 and target API 36. The actual APK is approximately 42 MiB; Unity's total build-output byte count includes additional output files and is not the APK file size.

The existing fall-to-game-over transition now displays GAME OVER, a fall message, distance, best score and TRY AGAIN. The earlier APK remains unchanged. This APK has not been installed or played on a physical Android device during this update. Earlier notes about missing Android modules and no APK are historical and superseded by this build.

## Launcher icon update — 1 November 2026

Built `Builds/Android/RooftopRunner-NewIcon.apk` successfully (zero errors). Verified the APK signature with Android apksigner. Inspected the exported icon image and confirmed six density-specific background/foreground pairs plus the adaptive application icon in the packaged resource table. Project settings changes were limited to the default and Android icon assignments. The icon has not been checked on a physical phone launcher.

## Missed-landing regression fix — 1 November 2026

Reproduced the reported stuck-runner problem with two colliding buildings separated by a gap. Before the fix, the runner remained in Running at (7.69, 0.07) after 1.5 seconds, held against the landing wall by friction. The earlier test that moved the runner directly below the fall threshold did not exercise this failure mode.

The runner now uses zero-friction collision material and records a missed landing when a frontal wall contact occurs with its feet below the roof surface. The controller ends the run on that contact or on the existing fall threshold. Reset clears the missed-landing flag.

Verified the real-physics wall collision reaches GAME OVER, the overlay is visible, TRY AGAIN restarts and hides it, and a 300 m run with at least ten charged jumps still succeeds. The existing 2.5 km generation audit and pause/score tests passed. Rendered and inspected the game-over screen. Evidence: `Validation/fall-fix-results.txt` and `Validation/fall-gameover.png`. Device testing remains to be done.

## Home screen — 1 November 2026

Unity 6000.6.7f1 Play Mode checks passed in a separate project copy (`ROOFTOP_SMOKE_OK`). Verified home visibility and hidden gameplay HUD, disabled Chase Run, opening/closing settings, charge timing and reduced-motion changes, Free Run launch, and returning home from pause with time restored and runner frozen. The existing charge, pause/resume, score persistence, restart, real-physics missed-gap GAME OVER, 2.5 km roof geometry and 300 m charged-jump traversal checks passed again.

Inspected 1280 × 720 Unity renders of the home screen and settings. Evidence: `Validation/home-screen.png`, `Validation/home-settings.png`, `Validation/home-screen-results.txt`. Controls use safe-area anchors, but physical Android touch, screen cutouts, unusual aspect ratios and performance still require device checks. Recommended manual flow: launch → Settings → change/reopen settings → Free Run → pause → Home → Free Run → miss a roof → GAME OVER → Home → Free Run. Confirm settings and best distance survive closing/reopening the app. Chase Run must remain disabled.

Built `Builds/Android/RooftopRunner-v0.2.0-Home.apk` successfully: version 0.2.0 / versionCode 3, ARM64, minimum API 26, target API 36, approximately 44 MiB. Unity reported zero errors and one warning. Android apksigner verified its v2 signature; aapt confirmed package and version metadata. This APK has not been installed on a physical phone during this update.

## Sound and haptics — 2 November 2026

Unity Play Mode suite passed (`Validation/audio-results.txt`): effect clip assignments, persisted sound/vibration switches, footsteps while grounded, charge playback and increasing pitch, stopping on release, one failure-feedback dispatch, and landing feedback across at least ten real jumps. Existing home, pause, charge, fall/wall game-over, restart, 2.5 km geometry and 300 m traversal checks also passed. Inspected the expanded Settings render (`Validation/audio-settings.png`).

These checks establish playback state and event timing, not subjective sound quality. Manual Android checks remain: listen on speaker/headphones, confirm no footsteps in flight, charge stops on release/pause, one short vibration on a missed roof, switches work independently and survive app restart, and returning from background stays paused. Vibration requires device hardware and may be affected by system preferences. No music was added.

APK build permission was declined, so Android compilation, manifest merging and physical-device vibration are unverified for this update. Previously built APKs are unchanged.

## Home button clipping fix — 2 November 2026

Replaced the fixed 580-unit menu card and negative Home-button offset with proportional anchors inside the safe area. Pause has three separated buttons; game over moves Home directly below Try Again. Text scales within its allocated space. Checked 1600 × 720 pause/game-over renders and asserted all four Home-button corners fall inside the safe area in both states. Full gameplay and sound regression suite passed. Evidence: `Validation/pause-wide-fixed.png`, `Validation/gameover-wide-fixed.png`, `Validation/menu-layout-results.txt`. Changes are in the project scene and runtime layout; no new APK was built in this update. Physical phone verification remains.

## User-supplied running and grunt — 3 November 2026

Imported the two supplied MP3s and assigned them in the saved scene. Unity suite passed, including actual takeoff playing exactly one grunt, running playback pausing in the air, and rejected airborne input not repeating the grunt. Existing charge, landing, settings, wide menu bounds, fall, restart and route tests passed. Evidence: Validation/user-audio-results.txt. Phone listening and subjective loop/volume balance still require checking. No new APK was built.

## Atmosphere transition — 3 November 2026

Unity suite passed with new checks for sunset reset, halfway dusk, full night at 800 m, pause holding the transition, and Home reset. Existing sound, jump, menu bounds, missed-gap game over, restart, 2.5 km generation and 300 m traversal checks passed. Inspected rendered dusk and night screenshots: cloud detail, painted glowing windows, player, UI and roof edges remain visible. Evidence: Validation/atmosphere-results.txt, atmosphere-dusk.png and atmosphere-night.png. Screenshots use frozen player positions to inspect distant route sections; they are not landing tests.

No new APK built. Android shader compilation, device appearance at low brightness, performance, and a full physical-phone night run remain manual checks. The shader is referenced through saved materials in SunsetArt so builds include it.
