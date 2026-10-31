using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
namespace RooftopRunner
{
    public sealed class RunnerUI : MonoBehaviour
    {
        public GameController game;
        public Text distanceText, bestText, title, subtitle, primaryLabel;
        public GameObject overlay, pauseButton, jumpButton, powerGroup;
        public Image chargeFill;
        public Button primaryButton, restartButton;
        public RectTransform safeArea;
        Rect lastSafe;
        int lastWidth, lastHeight;
        void Start() { ApplySafeArea(); }
        void Update()
        {
            if (Screen.safeArea != lastSafe || Screen.width != lastWidth || Screen.height != lastHeight) ApplySafeArea();
            distanceText.text = game.Distance.ToString() + " <size=20>m</size>";
            bestText.text = "BEST\n" + Mathf.Max(game.Best, game.Distance) + " m";
            chargeFill.fillAmount = game.runner.Charge;
            chargeFill.color = Color.Lerp(new Color(1, 0.55f, 0.18f), new Color(1, 0.86f, 0.37f), game.runner.Charge);
        }
        void ApplySafeArea()
        {
            lastSafe = Screen.safeArea; lastWidth = Screen.width; lastHeight = Screen.height;
            if (Screen.width == 0 || Screen.height == 0) return;
            safeArea.anchorMin = lastSafe.position / new Vector2(Screen.width, Screen.height);
            safeArea.anchorMax = (lastSafe.position + lastSafe.size) / new Vector2(Screen.width, Screen.height);
        }
        public void Refresh()
        {
            bool running = game.State == GameController.RunState.Running;
            overlay.SetActive(!running); pauseButton.SetActive(running); jumpButton.SetActive(running); if (powerGroup) powerGroup.SetActive(running);
            primaryButton.onClick.RemoveAllListeners(); restartButton.onClick.RemoveAllListeners();
            restartButton.gameObject.SetActive(game.State == GameController.RunState.Paused);
            if (game.State == GameController.RunState.Start)
            {
                title.text = "ROOFTOP\nRUNNER";
                subtitle.text = "CHASE THE LAST LIGHT\n\nHold to charge. Release to jump.\nLonger hold, higher jump. Stay on the rooftops.";
                primaryLabel.text = "START RUN"; primaryButton.onClick.AddListener(game.StartRun);
            }
            else if (game.State == GameController.RunState.Paused)
            {
                title.text = "TAKE A\nBREATHER";
                subtitle.text = "RUN PAUSED\n\nYour next rooftop is waiting.";
                primaryLabel.text = "RESUME"; primaryButton.onClick.AddListener(game.Resume);
                restartButton.onClick.AddListener(game.StartRun);
            }
            else if (game.State == GameController.RunState.GameOver)
            {
                title.text = "ONE MORE\nROOFTOP?";
                subtitle.text = "RUN COMPLETE  /  " + game.Distance + " m\nBEST  /  " + game.Best + " m\n\nCharge before the edge. Release while grounded.";
                primaryLabel.text = "RUN AGAIN"; primaryButton.onClick.AddListener(game.StartRun);
            }
        }
    }
}
