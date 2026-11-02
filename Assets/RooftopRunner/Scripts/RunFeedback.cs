using UnityEngine;

namespace RooftopRunner
{
    // Gameplay effects only: no music or ambient track.
    public sealed class RunFeedback : MonoBehaviour
    {
        public GameController game;
        public AudioClip running, jumpGrunt;
        public AudioClip landing, charge;
        [Range(0, 1)] public float effectsVolume = .7f;
        [Range(0, 1)] public float chargeVolume = .22f;
        [Range(0, 1)] public float runningVolume = .45f;
        [Range(0, 1)] public float jumpVolume = .7f;
        [Range(20, 150)] public int failureVibrationMs = 70;
        public bool SoundEnabled => PlayerPrefs.GetInt("RooftopRunner.Sound", 1) == 1;
        public bool HapticsEnabled => PlayerPrefs.GetInt("RooftopRunner.Haptics", 1) == 1;
        public bool ChargePlaying => tone && tone.isPlaying;
        public float ChargePitch => tone ? tone.pitch : 0;
        public int FootstepCount { get; private set; }
        public int LandingCount { get; private set; }
        public int FailureCount { get; private set; }
        public int JumpCount { get; private set; }
        public bool RunningPlaying => runningSource && runningSource.isPlaying;
        AudioSource effects, tone, runningSource;
        float airborne;
        bool touchedRoof;
        void Awake()
        {
            effects = gameObject.AddComponent<AudioSource>();
            tone = gameObject.AddComponent<AudioSource>();
            runningSource = gameObject.AddComponent<AudioSource>();
            foreach (var source in new[] { effects, tone, runningSource })
            { source.playOnAwake = false; source.spatialBlend = 0; source.dopplerLevel = 0; }
            tone.loop = true; tone.clip = charge;
            runningSource.loop = true; runningSource.clip = running;
        }
        void OnEnable() { game.runner.Jumped += OnJump; }
        void OnJump()
        {
            if (game.State != GameController.RunState.Running || !SoundEnabled) return;
            runningSource.Pause(); tone.Stop(); tone.volume = 0;
            effects.PlayOneShot(jumpGrunt, jumpVolume); JumpCount++;
        }
        void LateUpdate()
        {
            if (game.State != GameController.RunState.Running || !game.runner.Active)
            { StopAudio(); return; }
            var runner = game.runner;
            if (SoundEnabled && runner.Charging)
            {
                tone.pitch = Mathf.Lerp(.8f, 2.2f, runner.Charge);
                tone.volume = Mathf.MoveTowards(tone.volume, chargeVolume, Time.unscaledDeltaTime * 4);
                if (!tone.isPlaying) tone.Play();
            }
            else { tone.Stop(); tone.volume = 0; }
            if (!runner.Grounded) { runningSource.Pause(); airborne += Time.deltaTime; return; }
            if (SoundEnabled)
            {
                runningSource.volume = runningVolume;
                if (!runningSource.isPlaying)
                {
                    runningSource.UnPause();
                    if (!runningSource.isPlaying) runningSource.Play();
                    FootstepCount++;
                }
            }
            else runningSource.Stop();
            if (touchedRoof && airborne > .10f)
            {
                LandingCount++;
                if (SoundEnabled) effects.PlayOneShot(landing, effectsVolume);
            }
            touchedRoof = true; airborne = 0;
        }
        public void ResetRun()
        { StopAudio(); touchedRoof = false; airborne = 0; }
        public void StopAudio()
        { if (runningSource) runningSource.Stop(); if (effects) effects.Stop(); if (tone) { tone.Stop(); tone.volume = 0; } }
        public void SetSound(bool enabled)
        { PlayerPrefs.SetInt("RooftopRunner.Sound", enabled ? 1 : 0); PlayerPrefs.Save(); if (!enabled) StopAudio(); }
        public void SetHaptics(bool enabled)
        { PlayerPrefs.SetInt("RooftopRunner.Haptics", enabled ? 1 : 0); PlayerPrefs.Save(); }
        public void Fail()
        {
            StopAudio(); FailureCount++;
            if (!HapticsEnabled) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var activity = unity.GetStatic<AndroidJavaObject>("currentActivity");
                using var vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                if (vibrator == null || !vibrator.Call<bool>("hasVibrator")) return;
                using var vibrationEffect = new AndroidJavaClass("android.os.VibrationEffect");
                using var effect = vibrationEffect.CallStatic<AndroidJavaObject>("createOneShot", (long)failureVibrationMs, -1);
                vibrator.Call("vibrate", effect);
            }
            catch (AndroidJavaException error) { Debug.LogWarning("Haptics unavailable: " + error.Message); }
#endif
        }
        void OnDisable() { game.runner.Jumped -= OnJump; StopAudio(); }
        void OnApplicationPause(bool paused) { if (paused) StopAudio(); }
        void OnApplicationFocus(bool focused) { if (!focused) StopAudio(); }
    }
}
