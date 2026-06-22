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

public static class JungleSamMainMenuSceneBuilder
{
    private const string ScenePath = "Assets/JungleSam/Scenes/Menu/Scene_MainMenu.unity";
    private const string MenuFolderPath = "Assets/JungleSam/Scenes/Menu";
    private const string LoginSpritePath = "Assets/JungleSam/UI/Logowanie.png";
    private const string MenuFontPath = "Assets/JungleSam/UI/BlackOpsOne-Regular SDF 1.asset";
    private const string GameplaySceneName = "Scene_A";
    private const string AutoBuildFlagPath = "Assets/JungleSam/Editor/RunMainMenuSceneBuild.flag";

    private static readonly Vector2 ReferenceResolution = new Vector2(1672f, 941f);
    private static TMP_FontAsset _menuFont;
    private static TMP_FontAsset _fallbackFont;

    [MenuItem("Jungle Sam/Build Main Menu Scene")]
    public static void BuildScene()
    {
        EnsureFolders();

        _menuFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(MenuFontPath);
        _fallbackFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Scene_MainMenu";

        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.006f, 0.01f, 0.012f, 1f);
        camera.orthographic = true;
        cameraObject.tag = "MainCamera";

        GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        InputSystemUIInputModule inputModule = eventSystemObject.GetComponent<InputSystemUIInputModule>();
        inputModule.deselectOnBackgroundClick = true;

        GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = ReferenceResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.sizeDelta = ReferenceResolution;

        Sprite loginSprite = LoadSprite(LoginSpritePath);
        Image background = CreateImage("Background_Logowanie", canvasRect, loginSprite, new Color(1f, 1f, 1f, 1f));
        Stretch(background.rectTransform);
        background.raycastTarget = false;

        GameObject controllerObject = new GameObject("MainMenuController");
        controllerObject.transform.SetParent(canvasRect, false);
        MainMenuController controller = controllerObject.AddComponent<MainMenuController>();

        RectTransform loginPanel = CreatePanel("LoginPanel", canvasRect);
        RectTransform mainMenuPanel = CreatePanel("MainMenuPanel", canvasRect);

        TMP_InputField usernameInput = CreateInput("UsernameInput", loginPanel, PixelRect(1253f, 467f, 321f, 48f), false);
        TMP_InputField passwordInput = CreateInput("PasswordInput", loginPanel, PixelRect(1253f, 569f, 321f, 48f), true);
        Button loginButton = CreateTransparentButton("LoginButton", loginPanel, PixelRect(1253f, 643f, 321f, 55f));
        Button registerButton = CreateTransparentButton("RegisterButton", loginPanel, PixelRect(1417f, 718f, 104f, 24f));

        TextMeshProUGUI loginErrorText = CreateText(
            "LoginErrorText",
            canvasRect,
            PixelRect(1253f, 746f, 321f, 34f),
            string.Empty,
            17,
            new Color32(238, 101, 81, 255),
            TextAlignmentOptions.Center);
        loginErrorText.raycastTarget = false;

        CreateRightLoggedInPanel(mainMenuPanel);
        TextMeshProUGUI playerNameText = CreateText(
            "PlayerNameText",
            mainMenuPanel,
            PixelRect(1253f, 430f, 321f, 42f),
            "OPERATOR: -",
            24,
            new Color32(194, 214, 133, 255),
            TextAlignmentOptions.Center);
        playerNameText.raycastTarget = false;

        Button newGameButton = CreateTransparentButton("NewGameButton", mainMenuPanel, PixelRect(96f, 455f, 256f, 64f));
        Button continueButton = CreateTransparentButton("ContinueButton", mainMenuPanel, PixelRect(96f, 534f, 256f, 64f));
        Button settingsButton = CreateTransparentButton("SettingsButton_VisualOnly", mainMenuPanel, PixelRect(96f, 611f, 256f, 64f));
        Button exitButton = CreateTransparentButton("ExitButton", mainMenuPanel, PixelRect(96f, 688f, 256f, 64f));
        settingsButton.interactable = false;

        Button logoutButton = CreateTextButton("LogoutButton", mainMenuPanel, PixelRect(1342f, 646f, 140f, 42f), "WYLOGUJ", 17);

        WireController(
            controller,
            loginPanel.gameObject,
            mainMenuPanel.gameObject,
            usernameInput,
            passwordInput,
            loginButton,
            registerButton,
            loginErrorText,
            newGameButton,
            continueButton,
            exitButton,
            logoutButton,
            playerNameText);

        loginPanel.gameObject.SetActive(true);
        mainMenuPanel.gameObject.SetActive(false);

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings(ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Scene_MainMenu built at {ScenePath}");
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
            BuildScene();
        };
    }

    private static void WireController(
        MainMenuController controller,
        GameObject loginPanel,
        GameObject mainMenuPanel,
        TMP_InputField usernameInput,
        TMP_InputField passwordInput,
        Button loginButton,
        Button registerButton,
        TextMeshProUGUI loginErrorText,
        Button newGameButton,
        Button continueButton,
        Button exitButton,
        Button logoutButton,
        TextMeshProUGUI playerNameText)
    {
        SerializedObject serializedController = new SerializedObject(controller);
        serializedController.FindProperty("gameplaySceneName").stringValue = GameplaySceneName;
        serializedController.FindProperty("loginPanel").objectReferenceValue = loginPanel;
        serializedController.FindProperty("mainMenuPanel").objectReferenceValue = mainMenuPanel;
        serializedController.FindProperty("usernameInput").objectReferenceValue = usernameInput;
        serializedController.FindProperty("passwordInput").objectReferenceValue = passwordInput;
        serializedController.FindProperty("loginButton").objectReferenceValue = loginButton;
        serializedController.FindProperty("registerButton").objectReferenceValue = registerButton;
        serializedController.FindProperty("loginErrorText").objectReferenceValue = loginErrorText;
        serializedController.FindProperty("newGameButton").objectReferenceValue = newGameButton;
        serializedController.FindProperty("continueButton").objectReferenceValue = continueButton;
        serializedController.FindProperty("exitButton").objectReferenceValue = exitButton;
        serializedController.FindProperty("logoutButton").objectReferenceValue = logoutButton;
        serializedController.FindProperty("playerNameText").objectReferenceValue = playerNameText;
        serializedController.ApplyModifiedPropertiesWithoutUndo();
    }

    private static RectTransform CreatePanel(string name, RectTransform parent)
    {
        GameObject panelObject = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
        RectTransform rect = panelObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        Stretch(rect);
        return rect;
    }

    private static void CreateRightLoggedInPanel(RectTransform parent)
    {
        Image cover = CreateImage("LoggedInPanelCover", parent, null, new Color(0.02f, 0.025f, 0.02f, 0.86f));
        SetRect(cover.rectTransform, PixelRect(1217f, 344f, 404f, 432f));
        cover.raycastTarget = false;

        Image line = CreateImage("LoggedInPanelLine", parent, null, new Color(0.62f, 0.69f, 0.37f, 0.75f));
        SetRect(line.rectTransform, PixelRect(1253f, 506f, 321f, 2f));
        line.raycastTarget = false;

        CreateText(
            "LoggedInHeader",
            parent,
            PixelRect(1253f, 376f, 321f, 32f),
            "DOSTĘP PRZYZNANY",
            25,
            new Color32(194, 214, 133, 255),
            TextAlignmentOptions.Center).raycastTarget = false;

        CreateText(
            "LoggedInHint",
            parent,
            PixelRect(1265f, 530f, 296f, 70f),
            "WYBIERZ OPERACJĘ Z PANELU PO LEWEJ",
            16,
            new Color32(160, 166, 150, 255),
            TextAlignmentOptions.Center).raycastTarget = false;
    }

    private static TMP_InputField CreateInput(string name, RectTransform parent, Rect rect, bool password)
    {
        GameObject inputObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
        RectTransform inputRect = inputObject.GetComponent<RectTransform>();
        inputRect.SetParent(parent, false);
        SetRect(inputRect, rect);

        Image background = inputObject.GetComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.02f);
        background.raycastTarget = true;

        TextMeshProUGUI text = CreateText("Text", inputRect, new Rect(14f, 0f, rect.width - 28f, rect.height), string.Empty, 18, new Color32(215, 220, 205, 255), TextAlignmentOptions.MidlineLeft);
        text.raycastTarget = false;
        text.font = ResolveFont(false);

        TextMeshProUGUI placeholder = CreateText("Placeholder", inputRect, new Rect(14f, 0f, rect.width - 28f, rect.height), string.Empty, 18, new Color32(130, 136, 130, 0), TextAlignmentOptions.MidlineLeft);
        placeholder.raycastTarget = false;
        placeholder.font = ResolveFont(false);

        TMP_InputField input = inputObject.GetComponent<TMP_InputField>();
        input.textViewport = inputRect;
        input.textComponent = text;
        input.placeholder = placeholder;
        input.targetGraphic = background;
        input.caretColor = new Color32(194, 214, 133, 255);
        input.selectionColor = new Color32(111, 137, 69, 95);
        input.characterLimit = 32;
        input.contentType = password ? TMP_InputField.ContentType.Password : TMP_InputField.ContentType.Standard;
        input.lineType = TMP_InputField.LineType.SingleLine;

        return input;
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
        colors.highlightedColor = new Color(0.78f, 0.9f, 0.48f, 0.10f);
        colors.pressedColor = new Color(0.78f, 0.9f, 0.48f, 0.18f);
        colors.selectedColor = new Color(0.78f, 0.9f, 0.48f, 0.12f);
        colors.disabledColor = new Color(0.2f, 0.2f, 0.2f, 0.03f);
        button.colors = colors;

        return button;
    }

    private static Button CreateTextButton(string name, RectTransform parent, Rect rect, string label, int fontSize)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.SetParent(parent, false);
        SetRect(buttonRect, rect);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.28f, 0.35f, 0.14f, 0.65f);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;

        CreateText("Label", buttonRect, new Rect(0f, 0f, rect.width, rect.height), label, fontSize, new Color32(220, 228, 186, 255), TextAlignmentOptions.Center).raycastTarget = false;
        return button;
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

    private static TextMeshProUGUI CreateText(string name, RectTransform parent, Rect rect, string value, int size, Color color, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.SetParent(parent, false);
        SetRect(textRect, rect);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.font = ResolveFont(true);
        text.fontSize = size;
        text.color = color;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.margin = Vector4.zero;
        return text;
    }

    private static TMP_FontAsset ResolveFont(bool preferMenuFont)
    {
        if (preferMenuFont && _menuFont != null)
            return _menuFont;

        return _fallbackFont != null ? _fallbackFont : _menuFont;
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

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/JungleSam/Scenes"))
            AssetDatabase.CreateFolder("Assets/JungleSam", "Scenes");

        if (!AssetDatabase.IsValidFolder(MenuFolderPath))
            AssetDatabase.CreateFolder("Assets/JungleSam/Scenes", "Menu");
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes
            .Where(scene => scene.path != scenePath)
            .ToList();

        scenes.Insert(0, new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
