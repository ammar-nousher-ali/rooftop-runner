using UnityEngine;
using UnityEngine.InputSystem;
namespace RooftopRunner
{
    public sealed class GameController : MonoBehaviour
    {
        public enum RunState { Start, Running, Paused, GameOver }
        public Runner runner;
        public RooftopGenerator rooftops;
        public FollowCamera followCamera;
        public RunnerUI ui;
        public RunFeedback feedback;
        public CityAtmosphere atmosphere;
        public float fallHeight = -7;
        public RunState State { get; private set; }
        public int Distance { get; private set; }
        public int Best { get; private set; }
        const string BestKey = "RooftopRunner.BestDistance";
        void Start()
        {
            Application.targetFrameRate = 60;
            Best = PlayerPrefs.GetInt(BestKey, 0);
            Prepare(); State = RunState.Start; ui.Refresh();
        }
        void Prepare()
        {
            if (feedback) feedback.ResetRun();
            if (atmosphere) atmosphere.ResetSunset();
            Time.timeScale = 1;
            Distance = 0; rooftops.ResetRoute();
            runner.ResetRunner(new Vector2(0, 0.8f)); runner.Freeze(); followCamera.Snap();
        }
        public void StartRun()
        {
            Prepare(); runner.ResetRunner(new Vector2(0, 0.8f)); runner.Active = true;
            State = RunState.Running; ui.Refresh();
        }
        public void ReturnHome()
        {
            SaveBest(); Prepare(); State = RunState.Start; ui.Refresh();
        }
        void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.escapeKey.wasPressedThisFrame || keyboard.pKey.wasPressedThisFrame))
            {
                if (State == RunState.Running) Pause(); else if (State == RunState.Paused) Resume();
                else if (State == RunState.Start && ui.home) ui.home.CloseSettingsIfOpen();
            }
            if (State != RunState.Running) return;
            if (keyboard != null)
            {
                if (keyboard.spaceKey.wasPressedThisFrame) BeginCharge();
                if (keyboard.spaceKey.wasReleasedThisFrame) ReleaseCharge();
            }
            Distance = Mathf.Max(Distance, Mathf.FloorToInt(runner.transform.position.x));
            if (runner.MissedRoof || runner.transform.position.y < fallHeight) EndRun();
        }
        public void BeginCharge() { if (State == RunState.Running) runner.BeginCharge(); }
        public void ReleaseCharge() { if (State == RunState.Running) runner.ReleaseCharge(); }
        public void CancelCharge() { if (runner) runner.CancelCharge(); }
        public void Pause()
        {
            if (State != RunState.Running) return;
            CancelCharge(); if (feedback) feedback.StopAudio(); SaveBest(); State = RunState.Paused; Time.timeScale = 0; ui.Refresh();
        }
        public void Resume()
        {
            if (State != RunState.Paused) return;
            CancelCharge(); State = RunState.Running; Time.timeScale = 1; ui.Refresh();
        }
        void EndRun()
        {
            SaveBest(); runner.Freeze(); State = RunState.GameOver; ui.Refresh();
            if (feedback) feedback.Fail();
        }
        void SaveBest()
        {
            if (Distance <= Best) return;
            Best = Distance; PlayerPrefs.SetInt(BestKey, Best); PlayerPrefs.Save();
        }
        void OnApplicationPause(bool paused) { if (paused) Pause(); }
        void OnApplicationFocus(bool focused) { if (!focused) Pause(); }
        void OnApplicationQuit() { SaveBest(); }
        void OnDestroy() { Time.timeScale = 1; }
    }
}
