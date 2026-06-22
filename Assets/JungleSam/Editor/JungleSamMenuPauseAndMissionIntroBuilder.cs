using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class JungleSamMenuPauseAndMissionIntroBuilder
{
    private const string AutoBuildFlagPath = "Assets/JungleSam/Editor/RunMenuPauseMissionBuild.flag";
    private const string MissionIntroPrefabPath = "Assets/JungleSam/UI/MissionIntro/Prefabs/PF_MissionIntroPanel.prefab";
    private const string PausePrefabPath = "Assets/JungleSam/UI/Pause/Prefabs/PF_PauseMenu.prefab";
    private const string MainMenuScenePath = "Assets/JungleSam/Scenes/Menu/Scene_MainMenu.unity";
    private const string GameplayScenePath = "Assets/ThirdParty/Flooded_Grounds/Scenes/Scene_A.unity";
    private const string MissionIntroSpritePath = "Assets/JungleSam/UI/WybórMisji.png";
    private const string PauseSpritePath = "Assets/JungleSam/UI/Pauza.png";
    private const string MenuFontPath = "Assets/JungleSam/UI/BlackOpsOne-Regular SDF 1.asset";
    private const string GameplaySceneName = "Scene_A";
    private const string MainMenuSceneName = "Scene_MainMenu";

    private static readonly Vector2 ReferenceResolution = new Vector2(1672f, 941f);
    private static TMP_FontAsset _menuFont;

    [MenuItem("Jungle Sam/Build Menu Pause And Mission Intro")]
    public static void BuildAll()
    {
        _menuFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(MenuFontPath);
        EnsureFolders();
        GameObject missionIntroPrefab = BuildMissionIntroPrefab();
        GameObject pausePrefab = BuildPausePrefab();

        IntegrateMissionIntroIntoMainMenu(missionIntroPrefab);
        IntegratePauseIntoGameplay(pausePrefab);
        EnsureBuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Mission Intro and Pause Menu UI built and integrated.");
    }

    [InitializeOnLoadMethod]
    private static void RunRequestedBuildOnLoad()
    {
        if (!File.Exists(AutoBuildFlagPath))
            return;

        EditorApplication.delayCall += () =>
        {
            if (!File.Exists(AutoBuildFlagPath))
                return;

            File.Delete(AutoBuildFlagPath);
            File.Delete($"{AutoBuildFlagPath}.meta");
            AssetDatabase.Refresh();
            BuildAll();
        };
    }

    private static GameObject BuildMissionIntroPrefab()
    {
        GameObject canvasObject = new GameObject("Canvas_MissionIntro", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(MissionIntroController));
        ConfigureCanvas(canvasObject.GetComponent<Canvas>(), 600);
        ConfigureCanvasScaler(canvasObject.GetComponent<CanvasScaler>());

        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.sizeDelta = ReferenceResolution;

        RectTransform root = CreateRoot("MissionIntroRoot", canvasRect);
        Image background = CreateImage("MissionIntroBackground", root, LoadSprite(MissionIntroSpritePath), Color.white);
        Stretch(background.rectTransform);
        background.raycastTarget = true;

        Button startButton = CreateTransparentButton("StartMissionButton", root, PixelRect(1265f, 790f, 363f, 74f));
        Button backButton = CreateTransparentButton("BackButton", root, PixelRect(56f, 36f, 180f, 52f));

        TextMeshProUGUI startText = CreateHiddenText("StartMissionButtonText", startButton.transform as RectTransform, "ROZPOCZNIJ MISJĘ");
        TextMeshProUGUI backText = CreateHiddenText("BackButtonText", backButton.transform as RectTransform, "WSTECZ");

        MissionIntroController controller = canvasObject.GetComponent<MissionIntroController>();
        WireMissionIntro(controller, root.gameObject, background, startButton, backButton, startText, backText, null, null);
        root.gameObject.SetActive(false);

        GameObject prefab = SavePrefab(canvasObject, MissionIntroPrefabPath);
        Object.DestroyImmediate(canvasObject);
        return prefab;
    }

    private static GameObject BuildPausePrefab()
    {
        GameObject canvasObject = new GameObject("Canvas_PauseMenu", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(PauseMenuController));
        ConfigureCanvas(canvasObject.GetComponent<Canvas>(), 700);
        ConfigureCanvasScaler(canvasObject.GetComponent<CanvasScaler>());

        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.sizeDelta = ReferenceResolution;

        RectTransform root = CreateRoot("PauseRoot", canvasRect);
        Image background = CreateImage("PauseBackground", root, LoadSprite(PauseSpritePath), Color.white);
        Stretch(background.rectTransform);
        background.raycastTarget = true;

        Button resumeButton = CreateTransparentButton("ResumeButton", root, PixelRect(211f, 424f, 496f, 63f));
        Button settingsButton = CreateTransparentButton("SettingsButton", root, PixelRect(211f, 506f, 496f, 63f));
        Button checkpointButton = CreateTransparentButton("CheckpointButton", root, PixelRect(211f, 588f, 496f, 63f));
        Button exitButton = CreateTransparentButton("ExitToMenuButton", root, PixelRect(211f, 669f, 496f, 63f));
        settingsButton.interactable = true;

        PauseMenuController controller = canvasObject.GetComponent<PauseMenuController>();
        WirePause(controller, root.gameObject, background, resumeButton, settingsButton, checkpointButton, exitButton, null, null, null);
        root.gameObject.SetActive(false);

        GameObject prefab = SavePrefab(canvasObject, PausePrefabPath);
        Object.DestroyImmediate(canvasObject);
        return prefab;
    }

    private static void IntegrateMissionIntroIntoMainMenu(GameObject missionIntroPrefab)
    {
        if (missionIntroPrefab == null || !File.Exists(MainMenuScenePath))
            return;

        Scene scene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
        RemoveExistingRootObject("Canvas_MissionIntro");

        GameObject instance = PrefabUtility.InstantiatePrefab(missionIntroPrefab, scene) as GameObject;
        MissionIntroController intro = instance.GetComponent<MissionIntroController>();
        MainMenuController menu = Object.FindFirstObjectByType<MainMenuController>(FindObjectsInactive.Include);
        GameObject mainMenuPanel = FindObjectByName("MainMenuPanel");

        WireMissionIntroFromInstance(intro, menu, mainMenuPanel);
        WireMainMenu(menu, intro);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void IntegratePauseIntoGameplay(GameObject pausePrefab)
    {
        if (pausePrefab == null || !File.Exists(GameplayScenePath))
            return;

        Scene scene = EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);
        RemoveExistingRootObject("Canvas_PauseMenu");

        GameObject instance = PrefabUtility.InstantiatePrefab(pausePrefab, scene) as GameObject;
        PauseMenuController pause = instance.GetComponent<PauseMenuController>();

        PlayerControlLock playerControlLock = Object.FindFirstObjectByType<PlayerControlLock>(FindObjectsInactive.Include);
        CheckpointManager checkpointManager = Object.FindFirstObjectByType<CheckpointManager>(FindObjectsInactive.Include);
        GameObject playerRoot = playerControlLock != null ? playerControlLock.gameObject : null;
        WirePauseFromInstance(pause, playerControlLock, playerRoot, checkpointManager);

        EnsureEventSystem();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void WireMissionIntroFromInstance(MissionIntroController intro, MainMenuController menu, GameObject mainMenuPanel)
    {
        if (intro == null)
            return;

        GameObject root = FindChild(intro.transform, "MissionIntroRoot")?.gameObject;
        Image background = FindChildComponent<Image>(intro.transform, "MissionIntroBackground");
        Button startButton = FindChildComponent<Button>(intro.transform, "StartMissionButton");
        Button backButton = FindChildComponent<Button>(intro.transform, "BackButton");
        TMP_Text startText = FindChildComponent<TMP_Text>(intro.transform, "StartMissionButtonText");
        TMP_Text backText = FindChildComponent<TMP_Text>(intro.transform, "BackButtonText");

        WireMissionIntro(intro, root, background, startButton, backButton, startText, backText, menu, mainMenuPanel);

        if (root != null)
            root.SetActive(false);
    }

    private static void WireMainMenu(MainMenuController menu, MissionIntroController intro)
    {
        if (menu == null)
            return;

        SerializedObject serialized = new SerializedObject(menu);
        serialized.FindProperty("missionIntroController").objectReferenceValue = intro;
        serialized.FindProperty("showMissionIntroOnNewGame").boolValue = true;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void WirePauseFromInstance(PauseMenuController pause, PlayerControlLock playerControlLock, GameObject playerRoot, CheckpointManager checkpointManager)
    {
        if (pause == null)
            return;

        GameObject root = FindChild(pause.transform, "PauseRoot")?.gameObject;
        Image background = FindChildComponent<Image>(pause.transform, "PauseBackground");
        Button resumeButton = FindChildComponent<Button>(pause.transform, "ResumeButton");
        Button settingsButton = FindChildComponent<Button>(pause.transform, "SettingsButton");
        Button checkpointButton = FindChildComponent<Button>(pause.transform, "CheckpointButton");
        Button exitButton = FindChildComponent<Button>(pause.transform, "ExitToMenuButton");

        WirePause(pause, root, background, resumeButton, settingsButton, checkpointButton, exitButton, playerControlLock, playerRoot, checkpointManager);

        if (root != null)
            root.SetActive(false);
    }

    private static void WireMissionIntro(
        MissionIntroController controller,
        GameObject root,
        Image background,
        Button startButton,
        Button backButton,
        TMP_Text startText,
        TMP_Text backText,
        MainMenuController mainMenuController,
        GameObject mainMenuPanel)
    {
        SerializedObject serialized = new SerializedObject(controller);
        serialized.FindProperty("root").objectReferenceValue = root;
        serialized.FindProperty("backgroundImage").objectReferenceValue = background;
        serialized.FindProperty("startMissionButton").objectReferenceValue = startButton;
        serialized.FindProperty("backButton").objectReferenceValue = backButton;
        serialized.FindProperty("startMissionButtonText").objectReferenceValue = startText;
        serialized.FindProperty("backButtonText").objectReferenceValue = backText;
        serialized.FindProperty("gameplaySceneName").stringValue = GameplaySceneName;
        serialized.FindProperty("hideOnStart").boolValue = true;
        serialized.FindProperty("mainMenuController").objectReferenceValue = mainMenuController;
        serialized.FindProperty("mainMenuPanel").objectReferenceValue = mainMenuPanel;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void WirePause(
        PauseMenuController controller,
        GameObject root,
        Image background,
        Button resumeButton,
        Button settingsButton,
        Button checkpointButton,
        Button exitButton,
        PlayerControlLock playerControlLock,
        GameObject playerRoot,
        CheckpointManager checkpointManager)
    {
        SerializedObject serialized = new SerializedObject(controller);
        serialized.FindProperty("root").objectReferenceValue = root;
        serialized.FindProperty("backgroundImage").objectReferenceValue = background;
        serialized.FindProperty("resumeButton").objectReferenceValue = resumeButton;
        serialized.FindProperty("settingsButton").objectReferenceValue = settingsButton;
        serialized.FindProperty("checkpointButton").objectReferenceValue = checkpointButton;
        serialized.FindProperty("exitToMenuButton").objectReferenceValue = exitButton;
        serialized.FindProperty("playerControlLock").objectReferenceValue = playerControlLock;
        serialized.FindProperty("playerRoot").objectReferenceValue = playerRoot;
        serialized.FindProperty("checkpointManager").objectReferenceValue = checkpointManager;
        serialized.FindProperty("mainMenuSceneName").stringValue = MainMenuSceneName;
        serialized.FindProperty("pauseOnOpen").boolValue = true;
        serialized.FindProperty("unlockCursorOnPause").boolValue = true;
        serialized.FindProperty("lockCursorOnResume").boolValue = true;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static RectTransform CreateRoot(string name, RectTransform parent)
    {
        GameObject rootObject = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
        RectTransform root = rootObject.GetComponent<RectTransform>();
        root.SetParent(parent, false);
        Stretch(root);
        return root;
    }

    private static Image CreateImage(string name, RectTransform parent, Sprite sprite, Color color)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);

        Image image = imageObject.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.type = Image.Type.Simple;
        image.preserveAspect = false;
        return image;
    }

    private static Button CreateTransparentButton(string name, RectTransform parent, Rect rect)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.SetParent(parent, false);
        SetRect(buttonRect, rect);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.01f);
        image.raycastTarget = true;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(1f, 1f, 1f, 0.01f);
        colors.highlightedColor = new Color(0.78f, 0.9f, 0.48f, 0.12f);
        colors.pressedColor = new Color(0.78f, 0.9f, 0.48f, 0.22f);
        colors.selectedColor = new Color(0.78f, 0.9f, 0.48f, 0.14f);
        colors.disabledColor = new Color(0.25f, 0.25f, 0.25f, 0.04f);
        button.colors = colors;
        return button;
    }

    private static TextMeshProUGUI CreateHiddenText(string name, RectTransform parent, string value)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        Stretch(rect);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.font = _menuFont;
        text.fontSize = 20;
        text.color = new Color(1f, 1f, 1f, 0f);
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return text;
    }

    private static void ConfigureCanvas(Canvas canvas, int sortingOrder)
    {
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
    }

    private static void ConfigureCanvasScaler(CanvasScaler scaler)
    {
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
    }

    private static Rect PixelRect(float x, float y, float width, float height)
    {
        return new Rect(x - ReferenceResolution.x * 0.5f, ReferenceResolution.y * 0.5f - y - height, width, height);
    }

    private static void SetRect(RectTransform transform, Rect rect)
    {
        transform.anchorMin = new Vector2(0.5f, 0.5f);
        transform.anchorMax = new Vector2(0.5f, 0.5f);
        transform.pivot = new Vector2(0f, 0f);
        transform.anchoredPosition = new Vector2(rect.x, rect.y);
        transform.sizeDelta = new Vector2(rect.width, rect.height);
        transform.localScale = Vector3.one;
    }

    private static void Stretch(RectTransform transform)
    {
        transform.anchorMin = Vector2.zero;
        transform.anchorMax = Vector2.one;
        transform.pivot = new Vector2(0.5f, 0.5f);
        transform.offsetMin = Vector2.zero;
        transform.offsetMax = Vector2.zero;
        transform.localScale = Vector3.one;
    }

    private static Sprite LoadSprite(string path)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

        if (sprite != null)
            return sprite;

        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
    }

    private static GameObject SavePrefab(GameObject instance, string path)
    {
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);
        AssetDatabase.ImportAsset(path);
        return prefab;
    }

    private static void RemoveExistingRootObject(string objectName)
    {
        GameObject existing = FindObjectByName(objectName);

        if (existing != null)
            Object.DestroyImmediate(existing);
    }

    private static GameObject FindObjectByName(string objectName)
    {
        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject obj in objects)
        {
            if (obj != null && obj.name == objectName && obj.scene.IsValid())
                return obj;
        }

        return null;
    }

    private static Transform FindChild(Transform parent, string childName)
    {
        if (parent == null)
            return null;

        Transform[] children = parent.GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (child != null && child.name == childName)
                return child;
        }

        return null;
    }

    private static T FindChildComponent<T>(Transform parent, string childName) where T : Component
    {
        Transform child = FindChild(parent, childName);
        return child != null ? child.GetComponent<T>() : null;
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include) != null)
            return;

        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
    }

    private static void EnsureBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
        AddSceneIfMissing(scenes, MainMenuScenePath);
        AddSceneIfMissing(scenes, GameplayScenePath);
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static void AddSceneIfMissing(List<EditorBuildSettingsScene> scenes, string scenePath)
    {
        EditorBuildSettingsScene existing = scenes.FirstOrDefault(scene => scene.path == scenePath);

        if (existing != null)
        {
            existing.enabled = true;
            return;
        }

        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets/JungleSam/Core/UI/Pause");
        EnsureFolder("Assets/JungleSam/Core/UI/MissionIntro");
        EnsureFolder("Assets/JungleSam/UI/Pause");
        EnsureFolder("Assets/JungleSam/UI/Pause/Prefabs");
        EnsureFolder("Assets/JungleSam/UI/MissionIntro");
        EnsureFolder("Assets/JungleSam/UI/MissionIntro/Prefabs");
    }

    private static void EnsureFolder(string path)
    {
        string normalized = path.Replace("\\", "/");

        if (AssetDatabase.IsValidFolder(normalized))
            return;

        string parent = Path.GetDirectoryName(normalized)?.Replace("\\", "/");
        string name = Path.GetFileName(normalized);

        if (!string.IsNullOrEmpty(parent))
            EnsureFolder(parent);

        AssetDatabase.CreateFolder(parent, name);
    }
}
