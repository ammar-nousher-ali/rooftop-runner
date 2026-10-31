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
