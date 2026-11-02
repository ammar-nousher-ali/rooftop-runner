#if UNITY_EDITOR
using System.Collections;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace RooftopRunner
{
    // Opt-in batch validation only; excluded entirely from Android players.
    public sealed class SmokeChecks : MonoBehaviour
    {
        const string Flag = "RooftopRunner.Smoke";
        int oldBest;
        bool hadBest;
        public static void Run()
        {
            SessionState.SetBool(Flag, true);
            EditorSceneManager.OpenScene("Assets/RooftopRunner/Scenes/RooftopRunner.unity");
            EditorApplication.isPlaying = true;
        }
        public static void RenderPreview()
        {
            SessionState.SetBool("RooftopRunner.PreviewOnly", true); Run();
        }
        [InitializeOnLoadMethod]
        static void Register() { EditorApplication.playModeStateChanged += OnPlay; }
        static void OnPlay(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Flag, false))
            {
                SessionState.SetBool(Flag, false);
                new GameObject("Smoke checks").AddComponent<SmokeChecks>();
            }
        }
        void Check(bool condition, string message)
        {
            if (!condition) { Debug.LogError("SMOKE_FAIL: " + message); RestoreBest(); EditorApplication.Exit(1); throw new System.Exception(message); }
            Debug.Log("SMOKE_PASS: " + message);
        }
        IEnumerator Start()
        {
            hadBest = PlayerPrefs.HasKey("RooftopRunner.BestDistance"); oldBest = PlayerPrefs.GetInt("RooftopRunner.BestDistance", 0);
            yield return null;
            var game = FindFirstObjectByType<GameController>();
            Check(game && game.State == GameController.RunState.Start, "Start screen");
            Check(game.ui.home && game.ui.home.gameObject.activeSelf && !game.ui.safeArea.gameObject.activeSelf, "Home visible and gameplay HUD hidden");
            var home = game.ui.home;
            Check(!home.chaseRun.interactable, "Chase Run is Coming Soon and disabled");
            home.settings.onClick.Invoke();
            Check(home.settingsPanel.activeSelf, "Settings opens");
            float oldCharge = game.runner.chargeSeconds;
            home.chargeSetting.onClick.Invoke();
            Check(game.runner.chargeSeconds != oldCharge, "Charge setting updates gameplay");
            home.chargeSetting.onClick.Invoke(); home.chargeSetting.onClick.Invoke();
            bool oldMotion = game.followCamera.reducedMotion;
            home.motionSetting.onClick.Invoke();
            Check(game.followCamera.reducedMotion != oldMotion, "Reduced motion setting updates camera");
            home.motionSetting.onClick.Invoke();
            if (SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null)
                yield return Capture(game, "/tmp/rooftop-settings.png");
            home.closeSettings.onClick.Invoke();
            Check(!home.settingsPanel.activeSelf, "Settings closes");
            if (SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                yield return Capture(game, "/tmp/rooftop-menu.png");
            }
            game.ui.home.freeRun.onClick.Invoke(); yield return new WaitForSeconds(0.2f);
            Check(game.runner.Grounded, "Runner lands on initial roof");
            var animation = game.runner.GetComponent<RunnerAnimator>();
            Check(animation && animation.art && animation.illustration.sprite, "Animated runner artwork is assigned and rendered");
            Check(game.rooftops.transform.GetChild(0).GetComponent<RooftopDress>() && game.rooftops.transform.GetChild(0).childCount >= 2, "Painted rooftops and props instantiated");
            if (SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                yield return Capture(game, "/tmp/rooftop-game.png");
            }
            if (SessionState.GetBool("RooftopRunner.PreviewOnly", false))
            {
                SessionState.SetBool("RooftopRunner.PreviewOnly", false);
                game.StartRun(); game.runner.Freeze(); RestoreBest();
                Debug.Log("ROOFTOP_PREVIEW_OK"); EditorApplication.Exit(0); yield break;
            }
            Check(game.Distance > 0, "Automatic running and distance score");
            game.BeginCharge(); yield return new WaitForSeconds(0.1f); game.ReleaseCharge();
            yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            float shortSpeed = game.runner.GetComponent<Rigidbody2D>().linearVelocity.y;
            Check(shortSpeed > 3, "Short charge produces upward velocity");
            game.BeginCharge(); Check(!game.runner.Charging, "Airborne charge rejected");
            game.StartRun(); yield return new WaitForSeconds(0.2f);
            game.BeginCharge(); yield return new WaitForSeconds(game.runner.chargeSeconds + 0.15f);
            Check(Mathf.Approximately(game.runner.Charge, 1), "Charge clamps at maximum");
            game.ReleaseCharge(); yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            Check(game.runner.GetComponent<Rigidbody2D>().linearVelocity.y > shortSpeed + 2, "Longer hold increases jump strength");
            game.Pause(); Vector2 pausedPosition = game.runner.GetComponent<Rigidbody2D>().position;
            yield return new WaitForSecondsRealtime(0.15f);
            Check(game.State == GameController.RunState.Paused && game.runner.GetComponent<Rigidbody2D>().position == pausedPosition && Time.timeScale == 0, "Pause freezes simulation");
            game.ui.homeButton.onClick.Invoke();
            Check(game.State == GameController.RunState.Start && home.gameObject.activeSelf && Time.timeScale == 1 && !game.runner.Active, "Home from pause resets time and freezes runner");
            home.freeRun.onClick.Invoke(); game.Pause();
            game.Resume(); Check(Time.timeScale == 1 && game.State == GameController.RunState.Running, "Resume restores play");
            game.StartRun(); yield return new WaitForSeconds(0.2f);
            game.BeginCharge(); game.Pause(); Check(!game.runner.Charging, "Pause cancels held charge"); game.Resume();
            game.runner.transform.position = new Vector3(25, -10, 0);
            game.runner.GetComponent<Rigidbody2D>().position = new Vector2(25, -10);
            yield return null; yield return null;
            Check(game.State == GameController.RunState.GameOver, "Falling ends run");
            Check(PlayerPrefs.GetInt("RooftopRunner.BestDistance") >= game.Distance, "Best score persisted locally");
            game.StartRun(); yield return new WaitForSeconds(0.1f);
            Check(game.Distance < 3 && game.State == GameController.RunState.Running && game.rooftops.RoofCount < 12, "Restart resets score and route");
            // Reproduce the phone report using real physics: run off a roof into the next wall.
            game.rooftops.enabled = false;
            foreach (Transform roof in game.rooftops.transform) roof.gameObject.SetActive(false);
            var takeoff = new GameObject("Regression takeoff"); takeoff.layer = 6;
            takeoff.transform.position = new Vector3(0, -9, 0);
            takeoff.AddComponent<BoxCollider2D>().size = new Vector2(12, 18);
            var wall = new GameObject("Regression landing wall"); wall.layer = 6;
            wall.transform.position = new Vector3(14, -9, 0);
            wall.AddComponent<BoxCollider2D>().size = new Vector2(12, 18);
            game.runner.ResetRunner(new Vector2(5, 0.7f)); game.runner.Active = true;
            float failDeadline = Time.time + 1.5f;
            while (game.State == GameController.RunState.Running && Time.time < failDeadline) yield return new WaitForFixedUpdate();
            Debug.Log("WALL_REGRESSION: state=" + game.State + " position=" + game.runner.GetComponent<Rigidbody2D>().position);
            Check(game.State == GameController.RunState.GameOver && game.ui.overlay.activeSelf && game.ui.title.text == "GAME OVER", "Missed gap hitting a building side shows GAME OVER promptly");
            if (SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null)
                yield return Capture(game, "/tmp/rooftop-fall-gameover.png");
            takeoff.SetActive(false); wall.SetActive(false); Destroy(takeoff); Destroy(wall);
            game.rooftops.enabled = true;
            game.ui.primaryButton.onClick.Invoke(); yield return new WaitForSeconds(0.2f);
            Check(game.State == GameController.RunState.Running && game.runner.Grounded && !game.ui.overlay.activeSelf, "TRY AGAIN after a wall miss resets the runner and hides GAME OVER");
            // Audit actual generated roof geometry over a long route, including recycling.
            game.runner.Active = false;
            game.runner.GetComponent<Rigidbody2D>().simulated = false;
            for (int step = 0; step < 100; step++)
            {
                game.runner.transform.position = new Vector3(step * 25, 1, 0);
                yield return null; yield return null;
                float previousEdge = float.NaN, previousTop = 0;
                foreach (Transform roof in game.rooftops.transform)
                {
                    float left = roof.position.x - roof.localScale.x / 2;
                    float top = roof.position.y + roof.localScale.y / 2;
                    if (!float.IsNaN(previousEdge))
                    {
                        float reach = RooftopGenerator.Reach(game.runner.speed, game.runner.maximumJump, game.runner.gravity, top - previousTop);
                        if (left - previousEdge > reach - 2.09f) Check(false, "Generated gap exceeds safe reach");
                    }
                    previousEdge = roof.position.x + roof.localScale.x / 2; previousTop = top;
                }
                if (game.rooftops.RoofCount > 15) Check(false, "Roof recycling bound exceeded");
            }
            Check(true, "2.5 km of generated roofs satisfy reach margin and bounded object count");
            game.StartRun();
            Time.timeScale = 5;
            float deadline = Time.time + 75;
            int jumps = 0; bool actionCaptured = false;
            while (game.Distance < 300 && Time.time < deadline && game.State == GameController.RunState.Running)
            {
                yield return new WaitForFixedUpdate();
                if (!actionCaptured && game.Distance > 15 && !game.runner.Grounded && SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null)
                {
                    yield return Capture(game, "/tmp/rooftop-sunset-action.png"); actionCaptured = true;
                }
                if (!game.runner.Grounded) continue;
                float x = game.runner.GetComponent<Rigidbody2D>().position.x;
                foreach (Transform roof in game.rooftops.transform)
                {
                    float left = roof.position.x - roof.localScale.x / 2;
                    float right = roof.position.x + roof.localScale.x / 2;
                    if (x < left || x > right) continue;
                    float remaining = right - x;
                    if (!game.runner.Charging && remaining <= game.runner.speed * (game.runner.chargeSeconds + 0.22f) + 1)
                        game.BeginCharge();
                    if (game.runner.Charging && remaining <= 1.1f) { game.ReleaseCharge(); jumps++; }
                    break;
                }
            }
            Check(game.Distance >= 300 && jumps >= 10, "Physics-driven runner traverses 300 m with at least ten charged jumps");
            game.StartRun(); game.runner.Freeze(); RestoreBest();
            Debug.Log("ROOFTOP_SMOKE_OK"); EditorApplication.Exit(0);
        }
        IEnumerator Capture(GameController game, string path)
        {
            var camera = game.followCamera.GetComponent<Camera>();
            var canvas = game.ui.GetComponent<Canvas>();
            var target = new RenderTexture(1280, 720, 24);
            camera.targetTexture = target;
            int oldOrder = canvas.sortingOrder;
            canvas.sortingOrder = 100;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera; canvas.planeDistance = 1;
            yield return null;
            Canvas.ForceUpdateCanvases();
            FindFirstObjectByType<CityBackdrop>().SendMessage("LateUpdate");
            camera.Render();
            var previous = RenderTexture.active; RenderTexture.active = target;
            var texture = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); texture.Apply();
            try { System.IO.File.WriteAllBytes(path, texture.EncodeToPNG()); }
            catch (System.IO.IOException error) { Debug.LogWarning("Preview write skipped: " + error.Message); }
            RenderTexture.active = previous; camera.targetTexture = null;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = oldOrder;
            Destroy(texture); target.Release(); Destroy(target);
        }
        void RestoreBest()
        {
            if (hadBest) PlayerPrefs.SetInt("RooftopRunner.BestDistance", oldBest); else PlayerPrefs.DeleteKey("RooftopRunner.BestDistance");
            PlayerPrefs.Save();
        }
    }
}
#endif
