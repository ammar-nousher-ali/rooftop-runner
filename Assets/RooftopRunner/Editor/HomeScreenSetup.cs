using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using RooftopRunner;

public static class HomeScreenSetup
{
    const string Scene = "Assets/RooftopRunner/Scenes/RooftopRunner.unity";
    const string Art = "Assets/RooftopRunner/Art/Home/";
    static readonly Color Cream = new Color(1f, .94f, .77f);
    [MenuItem("Rooftop Runner/Install Home Screen")]
    public static void Install()
    {
        EditorSceneManager.OpenScene(Scene);
        AddToScene(Object.FindFirstObjectByType<RunnerUI>());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        PlayerSettings.bundleVersion = "0.2.1";
        PlayerSettings.Android.bundleVersionCode = 4;
        AssetDatabase.SaveAssets();
        Debug.Log("ROOFTOP_HOME_INSTALLED");
    }
    public static void InstallAndTest() { Install(); SmokeChecks.Run(); }
    public static void AddToScene(RunnerUI ui)
    {
        FeedbackSetup.AddToScene(ui.game);
        if (ui.home) Object.DestroyImmediate(ui.home.gameObject);
        if (ui.homeButton) Object.DestroyImmediate(ui.homeButton.gameObject);
        var root = Rect("Home Screen", ui.transform, 0, 0, 1, 1);
        var bg = Rect("Sunset city", root, 0, 0, 1, 1).gameObject.AddComponent<Image>();
        bg.sprite = Import("HomeBackground.png");
        var fit = bg.gameObject.AddComponent<AspectRatioFitter>();
        fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        fit.aspectRatio = bg.sprite.rect.width / bg.sprite.rect.height;
        var safe = Rect("Home Safe Area", root, 0, 0, 1, 1);
        var home = root.gameObject.AddComponent<HomeScreen>();
        ui.home = home; home.game = ui.game; home.safeContent = safe;
        var menu = Rect("Menu", safe, .615f, .10f, .955f, .88f);
        var logo = Rect("Logo", menu, -.04f, .51f, 1.04f, 1).gameObject.AddComponent<Image>();
        logo.sprite = Import("HomeLogo.png"); logo.preserveAspect = true; logo.raycastTarget = false;
        home.bestLabel = Label("Best", menu, "PERSONAL BEST   /   0 m", 19, 0, .43f, 1, .50f);
        home.freeRun = Button("Free Run", menu, "FREE RUN   >", new Color(1,.61f,.12f), 0,.23f,1,.41f, 38);
        home.freeRun.GetComponentInChildren<Text>().color = new Color(.19f,.09f,.05f);
        home.chaseRun = Button("Chase Run", menu, "CHASE RUN", new Color(.08f,.35f,.38f), 0,.04f,1,.20f, 29);
        var chaseText = home.chaseRun.GetComponentInChildren<Text>();
        chaseText.rectTransform.anchorMin = new Vector2(0,.28f);
        Label("Coming soon", home.chaseRun.transform, "COMING SOON", 15, 0,.05f,1,.34f);
        home.chaseRun.interactable = false;
        Label("Instructions", menu, "HOLD TO CHARGE  /  RELEASE TO JUMP", 14, -.1f,-.055f,1.1f,.01f);
        home.settings = Button("Settings", safe, "SETTINGS", new Color(.13f,.10f,.22f,.9f), .85f,.90f,.97f,.97f, 16);
        var modal = Rect("Settings Overlay", safe, 0,0,1,1);
        modal.gameObject.AddComponent<Image>().color = new Color(.03f,.02f,.08f,.88f);
        home.settingsPanel = modal.gameObject;
        var panel = Rect("Settings Card", modal, .23f,.10f,.77f,.90f);
        panel.gameObject.AddComponent<Image>().color = new Color(.12f,.09f,.20f);
        Label("Title", panel, "SETTINGS", 36, .05f,.84f,.95f,.98f);
        home.chargeSetting = Button("Charge setting", panel, "", new Color(.25f,.18f,.27f), .08f,.66f,.92f,.80f, 21);
        home.chargeLabel = home.chargeSetting.GetComponentInChildren<Text>();
        home.motionSetting = Button("Motion setting", panel, "", new Color(.25f,.18f,.27f), .08f,.50f,.92f,.64f, 21);
        home.motionLabel = home.motionSetting.GetComponentInChildren<Text>();
        home.soundSetting = Button("Sound effects", panel, "", new Color(.25f,.18f,.27f), .08f,.34f,.92f,.48f, 21);
        home.soundLabel = home.soundSetting.GetComponentInChildren<Text>();
        home.hapticsSetting = Button("Failure vibration", panel, "", new Color(.25f,.18f,.27f), .08f,.18f,.92f,.32f, 21);
        home.hapticsLabel = home.hapticsSetting.GetComponentInChildren<Text>();
        Label("Hint", panel, "Tap a setting to change it. Saved on this device.", 16, .05f,.12f,.95f,.18f);
        home.closeSettings = Button("Back", panel, "BACK", new Color(.88f,.48f,.12f), .32f,.01f,.68f,.11f, 23);
        modal.gameObject.SetActive(false);
        var content = ui.primaryButton.transform.parent;
        ui.homeButton = Button("Home", content, "HOME", new Color(.16f,.18f,.27f), .5f,0,.5f,0, 20);
        var hr = (RectTransform)ui.homeButton.transform;
        hr.offsetMin = new Vector2(-100,-48); hr.offsetMax = new Vector2(100,-4);
        ui.homeButton.gameObject.SetActive(false);
        ui.LayoutMenu();
    }
    static Sprite Import(string name)
    {
        string path = Art + name;
        AssetDatabase.ImportAsset(path);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true; importer.mipmapEnabled = false;
        importer.maxTextureSize = 2048; importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
    static RectTransform Rect(string name, Transform parent, float x, float y, float x2, float y2)
    {
        var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        r.SetParent(parent,false); r.anchorMin = new Vector2(x,y); r.anchorMax = new Vector2(x2,y2);
        r.offsetMin = r.offsetMax = Vector2.zero; return r;
    }
    static Text Label(string name, Transform parent, string value, int size, float x, float y, float x2, float y2)
    {
        var t = Rect(name,parent,x,y,x2,y2).gameObject.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); t.text=value;
        t.fontSize=size; t.fontStyle=FontStyle.Bold; t.alignment=TextAnchor.MiddleCenter;
        t.color=Cream; t.raycastTarget=false; t.resizeTextForBestFit=true;
        t.resizeTextMinSize=10; t.resizeTextMaxSize=size; return t;
    }
    static Button Button(string name, Transform parent, string text, Color color, float x, float y, float x2, float y2, int size)
    {
        var r=Rect(name,parent,x,y,x2,y2);
        var image=r.gameObject.AddComponent<Image>(); image.color=color;
        var border=r.gameObject.AddComponent<Outline>(); border.effectColor=new Color(1,.8f,.4f,.65f); border.effectDistance=new Vector2(2,-2);
        var button=r.gameObject.AddComponent<Button>(); button.targetGraphic=image;
        var colors=button.colors; colors.disabledColor=new Color(.65f,.7f,.7f,1); button.colors=colors;
        button.navigation=new Navigation { mode=Navigation.Mode.None };
        Label("Label",r,text,size,.03f,.05f,.97f,.95f); return button;
    }
}
