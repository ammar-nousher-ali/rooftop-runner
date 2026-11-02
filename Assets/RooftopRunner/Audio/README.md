# Sound effects

Original procedurally synthesized mono PCM WAV effects, 44.1 kHz, created for this project. No music or third-party recordings. Three dry shoe-impact variations, one deeper landing impact and a seamless 220 Hz charge oscillator. RunFeedback raises its playback pitch with charge strength. Effects are decompressed on load for prompt playback.

Adjust effectsVolume, chargeVolume, stepInterval and failureVibrationMs on the GameController object's RunFeedback component. Sound effects and failure vibration have separate saved settings; both default on. Vibration is Android-only, requests 70 ms using Android VibrationEffect, and checks for vibrator hardware. Phones and system preferences may suppress or alter the sensation.

Android reference: https://developer.android.com/reference/android/os/VibrationEffect#createOneShot(long,int)

## User-selected running and jump audio — 3 November 2026

Running.mp3 and JumpGrunt.mp3 are unchanged copies of the user's supplied running.mp3 and male jump grunt.mp3. Running loops only while grounded and resumes after airborne pauses. The grunt plays once from Runner's successful physical takeoff event, not on button presses or failed airborne attempts. These replace the synthesized footsteps; the older WAVs remain unused. Inspector runningVolume and jumpVolume independently control the new clips. Existing charge/landing effects and failure vibration remain, with no music.
