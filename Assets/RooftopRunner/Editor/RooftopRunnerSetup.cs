using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using RooftopRunner;

public static class RooftopRunnerSetup
{
    const string Root = "Assets/RooftopRunner/";
    const string ScenePath = Root + "Scenes/RooftopRunner.unity";
    static Font font;
    static Color ink = new Color(0.08f, 0.12f, 0.20f);
    static Color cream = new Color(1, 0.93f, 0.8f);
    static Color mint = new Color(0.35f, 0.9f, 0.84f);

    [MenuItem("Rooftop Runner/Create or Rebuild MVP Scene")]
    public static void Create()
    {
        Directory.CreateDirectory(Root + "Art"); Directory.CreateDirectory(Root + "Prefabs"); Directory.CreateDirectory(Root + "Scenes");
        var square = MakeSprite("Square", false); var disc = MakeSprite("Disc", true);
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var art = SunsetArtSetup.Import();
        if (!art) throw new System.InvalidOperationException("Sunset art could not be loaded.");
        var tags = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        tags.FindProperty("layers").GetArrayElementAtIndex(6).stringValue = "Rooftop"; tags.ApplyModifiedProperties();

        var roof = new GameObject("Rooftop"); roof.layer = 6;
        var roofRenderer = roof.AddComponent<SpriteRenderer>(); roofRenderer.sprite = square; roofRenderer.color = ink;
        roof.AddComponent<BoxCollider2D>();
        Visual("Roof edge", roof.transform, square, new Color(1, 0.64f, 0.23f), new Vector3(0, 0.499f, 0), new Vector3(1, 0.003f, 1), 4);
        if (art) roof.AddComponent<RooftopDress>().art = art;
        var roofPrefab = PrefabUtility.SaveAsPrefabAsset(roof, Root + "Prefabs/Rooftop.prefab"); Object.DestroyImmediate(roof);

        var player = new GameObject("Runner");
        var body = player.AddComponent<Rigidbody2D>(); body.constraints = RigidbodyConstraints2D.FreezeRotation;
        var box = player.AddComponent<BoxCollider2D>(); box.size = new Vector2(0.6f, 1.3f);
        var runner = player.AddComponent<Runner>();
        if (art)
        {
            var visual = new GameObject("Runner illustration").AddComponent<SpriteRenderer>();
            visual.transform.SetParent(player.transform, false); visual.transform.localPosition = new Vector3(0, -0.65f, 0); visual.sortingOrder = 10;
            var animation = player.AddComponent<RunnerAnimator>(); animation.art = art; animation.illustration = visual;
        }
        else Visual("Placeholder", player.transform, square, mint, Vector3.zero, new Vector3(0.6f, 1.3f, 1), 10);
        var runnerPrefab = PrefabUtility.SaveAsPrefabAsset(player, Root + "Prefabs/Runner.prefab"); Object.DestroyImmediate(player);
        runner = ((GameObject)PrefabUtility.InstantiatePrefab(runnerPrefab)).GetComponent<Runner>(); runner.transform.position = new Vector3(0, 0.8f);

        var cam = new GameObject("Main Camera").AddComponent<Camera>(); cam.tag = "MainCamera"; cam.orthographic = true; cam.orthographicSize = 6.4f;
        cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.77f, 0.43f, 0.43f); cam.nearClipPlane = 0.1f; cam.farClipPlane = 100;
        cam.gameObject.AddComponent<AudioListener>();
        var follow = cam.gameObject.AddComponent<FollowCamera>(); follow.target = runner.transform; follow.Snap();
        var city = new GameObject("Sunset City").AddComponent<CityBackdrop>(); city.view = cam; city.square = square; city.disc = disc; city.art = art;
        var generator = new GameObject("Rooftops").AddComponent<RooftopGenerator>(); generator.rooftopPrefab = roofPrefab; generator.runner = runner;
        var game = new GameObject("Game").AddComponent<GameController>(); game.runner = runner; game.rooftops = generator; game.followCamera = follow;
        var events = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        var canvasGo = new GameObject("Game UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1280, 720); scaler.matchWidthOrHeight = 0.5f;
        var ui = canvasGo.AddComponent<RunnerUI>(); ui.game = game; game.ui = ui;
        ui.safeArea = Rect("Safe Area", canvasGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var brand = Label("Game title", ui.safeArea, "ROOFTOP RUNNER", 29, TextAnchor.MiddleLeft, new Vector2(0, 1), new Vector2(0, 1), new Vector2(36, -65), new Vector2(430, -22)); brand.fontStyle = FontStyle.BoldAndItalic;
        Panel("Title underline", ui.safeArea, new Vector2(0, 1), new Vector2(0, 1), new Vector2(36, -72), new Vector2(330, -67), new Color(1, 0.58f, 0.18f));
        Label("Distance label", ui.safeArea, "DISTANCE", 17, TextAnchor.MiddleLeft, new Vector2(0, 1), new Vector2(0, 1), new Vector2(38, -106), new Vector2(240, -78));
        ui.distanceText = Label("Distance", ui.safeArea, "0 m", 36, TextAnchor.MiddleLeft, new Vector2(0, 1), new Vector2(0, 1), new Vector2(36, -151), new Vector2(340, -103));
        ui.bestText = Label("Best", ui.safeArea, "BEST\n0 m", 24, TextAnchor.UpperCenter, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(-140, -107), new Vector2(140, -28)); ui.bestText.color = new Color(1, 0.82f, 0.43f);
        var pause = Button("Pause", ui.safeArea, "II", new Vector2(1, 1), new Vector2(-98, -93), new Vector2(-30, -25), new Color(0.16f, 0.12f, 0.2f, 0.8f), cream);
        pause.GetComponent<Image>().sprite = disc;
        UnityEditor.Events.UnityEventTools.AddPersistentListener(pause.onClick, game.Pause); ui.pauseButton = pause.gameObject;
        var jump = Panel("Jump", ui.safeArea, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-198, 88), new Vector2(-38, 248), cream);
        jump.GetComponent<Image>().sprite = disc;
        var jumpCenter = Panel("Jump center", jump, Vector2.zero, Vector2.one, new Vector2(4, 4), new Vector2(-4, -4), new Color(0.18f, 0.16f, 0.21f, 0.92f)); jumpCenter.GetComponent<Image>().sprite = disc;
        jump.gameObject.AddComponent<JumpButton>().game = game; ui.jumpButton = jump.gameObject;
        Label("Jump arrow", jump, "↑", 110, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0, 4), new Vector2(0, 12));
        Label("Jump label", jump, "HOLD TO CHARGE\n<size=16>RELEASE TO JUMP</size>", 20, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(1, 0), new Vector2(-35, -66), new Vector2(35, -8));
        var power = Rect("Jump power", ui.safeArea, Vector2.zero, Vector2.zero, new Vector2(36, 35), new Vector2(380, 105)); ui.powerGroup = power.gameObject;
        Label("Charge caption", power, "JUMP POWER", 19, TextAnchor.UpperLeft, Vector2.zero, Vector2.one, new Vector2(3, 28), Vector2.zero);
        var track = Panel("Charge track", power, Vector2.zero, new Vector2(1, 0), Vector2.zero, new Vector2(0, 25), cream);
        var inner = Panel("Charge inner", track, Vector2.zero, Vector2.one, new Vector2(3, 3), new Vector2(-3, -3), new Color(0.16f, 0.13f, 0.2f, 0.95f));
        ui.chargeFill = Panel("Charge fill", inner, Vector2.zero, Vector2.one, new Vector2(2, 2), new Vector2(-2, -2), new Color(1, 0.65f, 0.23f)).GetComponent<Image>();
        ui.chargeFill.sprite = square; ui.chargeFill.type = Image.Type.Filled; ui.chargeFill.fillMethod = Image.FillMethod.Horizontal; ui.chargeFill.fillAmount = 0;
        var overlay = Panel("Menu Overlay", ui.safeArea, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.055f, 0.08f, 0.15f, 0.79f)); ui.overlay = overlay.gameObject;
        var content = Rect("Menu Content", overlay, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-440, -290), new Vector2(440, 290));
        Label("Eyebrow", content, "AN ENDLESS SUNSET", 18, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0, 525), new Vector2(0, -25));
        ui.title = Label("Title", content, "ROOFTOP\nRUNNER", 74, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0, 315), new Vector2(0, -65)); ui.title.fontStyle = FontStyle.Bold;
        ui.subtitle = Label("Subtitle", content, "", 23, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0, 155), new Vector2(0, -265));
        ui.primaryButton = Button("Primary", content, "START RUN", new Vector2(0.5f, 0), new Vector2(-180, 70), new Vector2(180, 135), new Color(1, 0.67f, 0.28f), ink);
        ui.primaryLabel = ui.primaryButton.GetComponentInChildren<Text>();
        ui.restartButton = Button("Restart", content, "RESTART RUN", new Vector2(0.5f, 0), new Vector2(-150, 7), new Vector2(150, 57), new Color(0.16f, 0.21f, 0.3f), cream);
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        PlayerSettings.companyName = "RooftopRunner"; PlayerSettings.productName = "Rooftop Runner";
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.defaultScreenWidth = 1280; PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, "com.rooftoprunner.game");
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        EditorUserBuildSettings.buildAppBundle = false;
        AssetDatabase.SaveAssets();
        Debug.Log("ROOFTOP_SETUP_OK: Scene, prefabs, UI and Android settings generated.");
    }
    static Sprite MakeSprite(string name, bool circle)
    {
        string path = Root + "Art/" + name + ".png";
        int size = circle ? 256 : 64;
        var texture = new Texture2D(size, size);
        for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
        {
            float alpha = circle ? Mathf.Clamp01(size * 0.5f - 1 - Vector2.Distance(new Vector2(x, y), Vector2.one * (size - 1) * 0.5f)) : 1;
            texture.SetPixel(x, y, new Color(1, 1, 1, alpha));
        }
        texture.Apply(); File.WriteAllBytes(path, texture.EncodeToPNG()); Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path); importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single; importer.spritePixelsPerUnit = size; importer.filterMode = FilterMode.Bilinear; importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
    static Transform Visual(string name, Transform parent, Sprite sprite, Color color, Vector3 position, Vector3 scale, int order)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.localPosition = position; go.transform.localScale = scale;
        var renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.color = color; renderer.sortingOrder = order; return go.transform;
    }
    static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max, Vector2 low, Vector2 high)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent, false); rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = low; rect.offsetMax = high; return rect;
    }
    static RectTransform Panel(string name, Transform parent, Vector2 min, Vector2 max, Vector2 low, Vector2 high, Color color)
    {
        var rect = Rect(name, parent, min, max, low, high); rect.gameObject.AddComponent<Image>().color = color; return rect;
    }
    static Text Label(string name, Transform parent, string text, int size, TextAnchor alignment, Vector2 min, Vector2 max, Vector2 low, Vector2 high)
    {
        var rect = Rect(name, parent, min, max, low, high); var label = rect.gameObject.AddComponent<Text>(); label.font = font; label.text = text; label.fontSize = size; label.color = cream; label.alignment = alignment; label.raycastTarget = false; return label;
    }
    static Button Button(string name, Transform parent, string text, Vector2 anchor, Vector2 low, Vector2 high, Color bg, Color fg)
    {
        var rect = Panel(name, parent, anchor, anchor, low, high, bg); var button = rect.gameObject.AddComponent<Button>();
        var label = Label("Label", rect, text, 23, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero); label.color = fg; return button;
    }
    public static void ValidateArt() { Create(); RooftopRunner.SmokeChecks.Run(); }

    [MenuItem("Rooftop Runner/Build Android APK")]
    public static void BuildAndroid()
    {
        if (!File.Exists(ScenePath)) throw new System.InvalidOperationException("Create the MVP scene first.");
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
            throw new System.InvalidOperationException("Android Build Support is missing for Unity " + Application.unityVersion + ". Install it through Unity Hub, including SDK/NDK and OpenJDK, then restart this editor.");
        Directory.CreateDirectory("Builds/Android"); EditorUserBuildSettings.buildAppBundle = false;
        EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { ScenePath }, locationPathName = "Builds/Android/RooftopRunner.apk", target = BuildTarget.Android, options = BuildOptions.None });
        if (report.summary.result != BuildResult.Succeeded) throw new System.Exception("APK build failed: " + report.summary.result);
        File.WriteAllText("Builds/Android/build-summary.txt", "Rooftop Runner Android APK\nUnity: " + Application.unityVersion + "\nResult: " + report.summary.result + "\nBytes: " + report.summary.totalSize + "\nErrors: " + report.summary.totalErrors + "\nWarnings: " + report.summary.totalWarnings + "\nElapsed: " + report.summary.totalTime + "\n");
        Debug.Log("ROOFTOP_APK_OK: " + Path.GetFullPath("Builds/Android/RooftopRunner.apk"));
    }
}
