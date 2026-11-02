using UnityEngine;
using UnityEngine.UI;

namespace RooftopRunner
{
    public sealed class HomeScreen : MonoBehaviour
    {
        public GameController game;
        public RectTransform safeContent;
        public GameObject settingsPanel;
        public Text bestLabel, chargeLabel, motionLabel;
        public Button freeRun, chaseRun, settings, closeSettings, chargeSetting, motionSetting;
        readonly float[] chargeTimes = { 0.45f, 0.65f, 0.9f };
        readonly string[] chargeNames = { "QUICK", "NORMAL", "RELAXED" };
        int chargeMode;
        bool reducedMotion;
        Rect lastSafe;
        Vector2 lastSize;
        void Awake()
        {
            chargeMode = Mathf.Clamp(PlayerPrefs.GetInt("RooftopRunner.ChargeMode", 1), 0, 2);
            reducedMotion = PlayerPrefs.GetInt("RooftopRunner.ReducedMotion", 0) == 1;
            freeRun.onClick.AddListener(game.StartRun);
            chaseRun.interactable = false;
            settings.onClick.AddListener(() => settingsPanel.SetActive(true));
            closeSettings.onClick.AddListener(CloseSettings);
            chargeSetting.onClick.AddListener(CycleCharge);
            motionSetting.onClick.AddListener(ToggleMotion);
            ApplySettings();
        }
        void OnEnable()
        {
            settingsPanel.SetActive(false); ApplySafeArea();
        }
        void Update()
        {
            if (Screen.safeArea != lastSafe || lastSize != new Vector2(Screen.width, Screen.height)) ApplySafeArea();
            bestLabel.text = "PERSONAL BEST   /   " + game.Best + " m";
        }
        void ApplySafeArea()
        {
            lastSafe = Screen.safeArea; lastSize = new Vector2(Screen.width, Screen.height);
            if (Screen.width == 0 || Screen.height == 0) return;
            safeContent.anchorMin = lastSafe.position / lastSize;
            safeContent.anchorMax = (lastSafe.position + lastSafe.size) / lastSize;
        }
        public bool CloseSettingsIfOpen()
        {
            if (!settingsPanel.activeSelf) return false;
            CloseSettings(); return true;
        }
        public void CloseSettings() { settingsPanel.SetActive(false); }
        public void CycleCharge() { chargeMode = (chargeMode + 1) % 3; SaveSettings(); }
        public void ToggleMotion() { reducedMotion = !reducedMotion; SaveSettings(); }
        void SaveSettings()
        {
            ApplySettings();
            PlayerPrefs.SetInt("RooftopRunner.ChargeMode", chargeMode);
            PlayerPrefs.SetInt("RooftopRunner.ReducedMotion", reducedMotion ? 1 : 0);
            PlayerPrefs.Save();
        }
        void ApplySettings()
        {
            game.runner.chargeSeconds = chargeTimes[chargeMode];
            game.followCamera.reducedMotion = reducedMotion;
            chargeLabel.text = "JUMP CHARGE     " + chargeNames[chargeMode] + "   >";
            motionLabel.text = "CAMERA MOTION     " + (reducedMotion ? "REDUCED" : "STANDARD") + "   >";
        }
    }
}
