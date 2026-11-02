using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
namespace RooftopRunner
{
    public sealed class RunnerUI : MonoBehaviour
    {
        public GameController game;
        public HomeScreen home;
        public Button homeButton;
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
        // Keep all menu controls inside the safe area instead of extending below
        // a fixed-height card (which clipped on wide landscape phones).
        public void LayoutMenu()
        {
            var content = primaryButton.transform.parent as RectTransform;
            Place(content, .08f, .06f, .92f, .94f);
            var eyebrow = content.Find("Eyebrow") as RectTransform;
            if (eyebrow) Place(eyebrow, 0, .91f, 1, 1);
            Place(title.rectTransform, 0, .56f, 1, .91f);
            Place(subtitle.rectTransform, 0, .31f, 1, .56f);
            Place((RectTransform)primaryButton.transform, .32f, .20f, .68f, .30f);
            Place((RectTransform)restartButton.transform, .35f, .10f, .65f, .18f);
            if (homeButton) Place((RectTransform)homeButton.transform, .40f,
                game.State == GameController.RunState.Paused ? 0 : .10f,
                .60f, game.State == GameController.RunState.Paused ? .08f : .18f);
            title.resizeTextForBestFit = true; title.resizeTextMinSize = 24; title.resizeTextMaxSize = 74;
            subtitle.resizeTextForBestFit = true; subtitle.resizeTextMinSize = 14; subtitle.resizeTextMaxSize = 23;
        }
        static void Place(RectTransform rect, float x, float y, float right, float top)
        {
            rect.anchorMin = new Vector2(x, y); rect.anchorMax = new Vector2(right, top);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
        public void Refresh()
        {
            LayoutMenu();
            bool running = game.State == GameController.RunState.Running;
            bool atHome = game.State == GameController.RunState.Start && home;
            if (home) home.gameObject.SetActive(atHome);
            safeArea.gameObject.SetActive(!atHome);
            if (homeButton) { homeButton.gameObject.SetActive(!atHome && !running); homeButton.onClick.RemoveAllListeners(); homeButton.onClick.AddListener(game.ReturnHome); }
            overlay.SetActive(!running && !atHome); pauseButton.SetActive(running); jumpButton.SetActive(running); if (powerGroup) powerGroup.SetActive(running);
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
                title.text = "GAME OVER";
                subtitle.text = "You fell between the rooftops!\n\nDISTANCE  /  " + game.Distance + " m\nBEST  /  " + game.Best + " m";
                primaryLabel.text = "TRY AGAIN"; primaryButton.onClick.AddListener(game.StartRun);
            }
        }
    }
}
