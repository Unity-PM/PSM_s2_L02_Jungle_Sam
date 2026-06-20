#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class StoryAndDeathUIBuilder
{
    private const string PopupDataFolder = "Assets/JungleSam/UI/Popups/Data";
    private const string PopupPrefabFolder = "Assets/JungleSam/UI/Popups/Prefabs";
    private const string DeathPrefabFolder = "Assets/JungleSam/UI/Death/Prefabs";
    private const string ArtFolder = "Assets/JungleSam/UI/Art/JungleSam_HUD_UI_Assets/Cleaned";

    [MenuItem("Jungle Sam/UI/Build Story And Death UI")]
    public static void BuildAll()
    {
        EnsureFolders();
        ConfigureTextureImportSettings();
        CreateStoryPopupDataAssets();
        CreateStoryPopupPrefab();
        CreateDeathPanelPrefab();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Jungle Sam story popup and death UI assets rebuilt.");
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets/JungleSam/UI", "Popups");
        EnsureFolder("Assets/JungleSam/UI/Popups", "Data");
        EnsureFolder("Assets/JungleSam/UI/Popups", "Prefabs");
        EnsureFolder("Assets/JungleSam/UI", "Death");
        EnsureFolder("Assets/JungleSam/UI/Death", "Prefabs");
        EnsureFolder("Assets/JungleSam/UI", "Pauza");
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = $"{parent}/{child}";

        if (!AssetDatabase.IsValidFolder(path))
            AssetDatabase.CreateFolder(parent, child);
    }

    private static void ConfigureTextureImportSettings()
    {
        string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { ArtFolder });

        foreach (string guid in textureGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer == null)
                continue;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = 4096;
            importer.SaveAndReimport();
        }
    }

    private static void CreateStoryPopupDataAssets()
    {
        CreateOrUpdatePopupData(
            $"{PopupDataFolder}/POPUP_GromRadio.asset",
            "POPUP_GromRadio",
            "DOWÓD / ŁUP FABULARNY",
            "RADIO GROM DIVISION",
            "Kanał awaryjny CH-07",
            "Odebrano urwany sygnał awaryjny. Jednostka Grom Division zgłasza utratę kontaktu z personelem Black Orchid. W pobliżu nabrzeża wykryto ruch zainfekowanych. Przygotuj się na kontakt.",
            null);

        CreateOrUpdatePopupData(
            $"{PopupDataFolder}/POPUP_ChurchMilitaryDocument.asset",
            "POPUP_ChurchMilitaryDocument",
            "DOWÓD / ŁUP FABULARNY",
            "DOKUMENT GROM DIVISION",
            "Raport terenowy // Sektor kościoła",
            "Zabezpieczono fragment raportu Grom Division.\n\nBlack Orchid przeniosła część materiału biologicznego do zabudowań obok kościoła. Oddział zabezpieczający zgłaszał wzrost agresji zainfekowanych w rejonie cmentarza.\n\nOstatni wpis:\n„Nie otwierać domu bez wsparcia. Sygnał nad strefą wpływa na mutanty.”",
            null);
    }

    private static void CreateOrUpdatePopupData(string path, string popupId, string category, string title, string subtitle, string body, Sprite image)
    {
        StoryPopupData data = AssetDatabase.LoadAssetAtPath<StoryPopupData>(path);

        if (data == null)
        {
            data = ScriptableObject.CreateInstance<StoryPopupData>();
            AssetDatabase.CreateAsset(data, path);
        }

        SerializedObject serialized = new SerializedObject(data);
        serialized.FindProperty("popupId").stringValue = popupId;
        serialized.FindProperty("itemImage").objectReferenceValue = image;
        serialized.FindProperty("categoryLabel").stringValue = category;
        serialized.FindProperty("title").stringValue = title;
        serialized.FindProperty("subtitle").stringValue = subtitle;
        serialized.FindProperty("body").stringValue = body;
        serialized.FindProperty("continueButtonText").stringValue = "KONTYNUUJ";
        serialized.FindProperty("lockPlayerWhileOpen").boolValue = true;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(data);
    }

    private static void CreateStoryPopupPrefab()
    {
        GameObject canvasObject = CreateCanvasRoot("Canvas_StoryPopup", 200);
        CanvasGroup canvasGroup = canvasObject.AddComponent<CanvasGroup>();
        StoryItemPopupUI popupUI = canvasObject.AddComponent<StoryItemPopupUI>();

        GameObject dimBackground = CreateImage("DimBackground", canvasObject.transform, null, new Color32(0, 0, 0, 180), true);
        Stretch(dimBackground.GetComponent<RectTransform>());

        GameObject popupRoot = CreateUIObject("PopupRoot", canvasObject.transform);
        RectTransform popupRect = popupRoot.GetComponent<RectTransform>();
        Center(popupRect, new Vector2(1300f, 720f));

        Sprite panelBackgroundSprite = LoadSprite("UI_Panel_Background.png");
        Sprite panelFrameSprite = LoadSprite("UI_Panel_Frame.png");
        Sprite buttonDarkSprite = LoadSprite("UI_Button_Dark.png");
        Sprite buttonSelectedSprite = LoadSprite("UI_Button_Selected.png");

        GameObject panelBackground = CreateImage("PanelBackground", popupRoot.transform, panelBackgroundSprite, new Color32(4, 7, 5, 220), false);
        Stretch(panelBackground.GetComponent<RectTransform>());

        GameObject panelFrame = CreateImage("PanelFrame", popupRoot.transform, panelFrameSprite != null ? panelFrameSprite : panelBackgroundSprite, new Color32(154, 174, 84, 210), false);
        Stretch(panelFrame.GetComponent<RectTransform>());

        GameObject itemImage = CreateImage("ItemImage", popupRoot.transform, null, new Color32(255, 255, 255, 255), false);
        RectTransform itemRect = itemImage.GetComponent<RectTransform>();
        Anchor(itemRect, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(275f, 35f), new Vector2(430f, 540f));
        itemImage.GetComponent<Image>().enabled = false;

        TMP_Text category = CreateText("CategoryLabelText", popupRoot.transform, "DOWÓD / ŁUP FABULARNY", 24, TextAlignmentOptions.Left, new Color32(154, 174, 84, 255));
        Anchor(category.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(545f, -105f), new Vector2(650f, 42f));

        TMP_Text title = CreateText("TitleText", popupRoot.transform, "RADIO GROM DIVISION", 46, TextAlignmentOptions.Left, new Color32(216, 230, 172, 255));
        Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(545f, -165f), new Vector2(650f, 58f));

        TMP_Text subtitle = CreateText("SubtitleText", popupRoot.transform, "Kanał awaryjny CH-07", 26, TextAlignmentOptions.Left, new Color32(190, 199, 160, 255));
        Anchor(subtitle.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(545f, -220f), new Vector2(650f, 42f));

        TMP_Text body = CreateText("BodyText", popupRoot.transform, string.Empty, 25, TextAlignmentOptions.TopLeft, new Color32(224, 228, 206, 255));
        body.enableWordWrapping = true;
        Anchor(body.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(545f, -420f), new Vector2(650f, 310f));

        GameObject continueButtonObject = CreateButton("ContinueButton", popupRoot.transform, buttonDarkSprite, buttonSelectedSprite);
        Anchor(continueButtonObject.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-235f, 95f), new Vector2(310f, 74f));

        TMP_Text buttonText = CreateText("ContinueButtonText", continueButtonObject.transform, "KONTYNUUJ", 25, TextAlignmentOptions.Center, new Color32(216, 230, 172, 255));
        Stretch(buttonText.rectTransform);

        SetObject(popupUI, "popupRoot", popupRoot);
        SetObject(popupUI, "canvasGroup", canvasGroup);
        SetObject(popupUI, "dimBackground", dimBackground.GetComponent<Image>());
        SetObject(popupUI, "panelBackground", panelBackground.GetComponent<Image>());
        SetObject(popupUI, "panelFrame", panelFrame.GetComponent<Image>());
        SetObject(popupUI, "itemImage", itemImage.GetComponent<Image>());
        SetObject(popupUI, "categoryLabelText", category);
        SetObject(popupUI, "titleText", title);
        SetObject(popupUI, "subtitleText", subtitle);
        SetObject(popupUI, "bodyText", body);
        SetObject(popupUI, "continueButton", continueButtonObject.GetComponent<Button>());
        SetObject(popupUI, "continueButtonText", buttonText);

        popupRoot.SetActive(false);
        SavePrefab(canvasObject, $"{PopupPrefabFolder}/PF_StoryPopup.prefab");
    }

    private static void CreateDeathPanelPrefab()
    {
        GameObject canvasObject = CreateCanvasRoot("Canvas_DeathPanel", 250);
        CanvasGroup canvasGroup = canvasObject.AddComponent<CanvasGroup>();
        DeathUIController deathUI = canvasObject.AddComponent<DeathUIController>();

        GameObject dimBackground = CreateImage("DeathDimBackground", canvasObject.transform, null, new Color32(0, 0, 0, 205), true);
        Stretch(dimBackground.GetComponent<RectTransform>());

        GameObject deathRoot = CreateUIObject("DeathRoot", canvasObject.transform);
        RectTransform deathRect = deathRoot.GetComponent<RectTransform>();
        Center(deathRect, new Vector2(920f, 520f));

        Sprite panelBackgroundSprite = LoadSprite("UI_Panel_Background.png");
        Sprite panelFrameSprite = LoadSprite("UI_Panel_Frame.png");
        Sprite buttonDarkSprite = LoadSprite("UI_Button_Dark.png");
        Sprite buttonSelectedSprite = LoadSprite("UI_Button_Selected.png");

        GameObject panelBackground = CreateImage("PanelBackground", deathRoot.transform, panelBackgroundSprite, new Color32(8, 4, 3, 230), false);
        Stretch(panelBackground.GetComponent<RectTransform>());

        GameObject panelFrame = CreateImage("PanelFrame", deathRoot.transform, panelFrameSprite != null ? panelFrameSprite : panelBackgroundSprite, new Color32(210, 86, 54, 215), false);
        Stretch(panelFrame.GetComponent<RectTransform>());

        TMP_Text title = CreateText("TitleText", deathRoot.transform, "ELIMINACJA", 56, TextAlignmentOptions.Center, new Color32(255, 156, 112, 255));
        Anchor(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(760f, 70f));

        TMP_Text subtitle = CreateText("SubtitleText", deathRoot.transform, "Utracono łączność bojową", 28, TextAlignmentOptions.Center, new Color32(230, 202, 172, 255));
        Anchor(subtitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -170f), new Vector2(760f, 45f));

        TMP_Text body = CreateText("BodyText", deathRoot.transform, "Ostatni checkpoint zostanie przywrócony. Utrzymaj pozycję i kontynuuj operację.", 25, TextAlignmentOptions.Center, new Color32(226, 220, 204, 255));
        body.enableWordWrapping = true;
        Anchor(body.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -270f), new Vector2(700f, 110f));

        GameObject respawnButtonObject = CreateButton("RespawnButton", deathRoot.transform, buttonDarkSprite, buttonSelectedSprite);
        Anchor(respawnButtonObject.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 135f), new Vector2(430f, 76f));
        TMP_Text respawnText = CreateText("RespawnButtonText", respawnButtonObject.transform, "POWRÓT DO CHECKPOINTU", 23, TextAlignmentOptions.Center, new Color32(255, 210, 174, 255));
        Stretch(respawnText.rectTransform);

        GameObject exitButtonObject = CreateButton("ExitButton", deathRoot.transform, buttonDarkSprite, buttonSelectedSprite);
        Anchor(exitButtonObject.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 55f), new Vector2(310f, 62f));
        TMP_Text exitText = CreateText("ExitButtonText", exitButtonObject.transform, "WYJDŹ DO MENU", 21, TextAlignmentOptions.Center, new Color32(220, 186, 160, 255));
        Stretch(exitText.rectTransform);

        SetObject(deathUI, "root", deathRoot);
        SetObject(deathUI, "canvasGroup", canvasGroup);
        SetObject(deathUI, "titleText", title);
        SetObject(deathUI, "subtitleText", subtitle);
        SetObject(deathUI, "bodyText", body);
        SetObject(deathUI, "respawnButton", respawnButtonObject.GetComponent<Button>());
        SetObject(deathUI, "exitButton", exitButtonObject.GetComponent<Button>());
        SetObject(deathUI, "respawnButtonText", respawnText);
        SetObject(deathUI, "exitButtonText", exitText);

        deathRoot.SetActive(false);
        SavePrefab(canvasObject, $"{DeathPrefabFolder}/PF_DeathPanel.prefab");
    }

    private static GameObject CreateCanvasRoot(string name, int sortingOrder)
    {
        GameObject canvasObject = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        return canvasObject;
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        return obj;
    }

    private static GameObject CreateImage(string name, Transform parent, Sprite sprite, Color color, bool raycastTarget)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(parent, false);
        Image image = obj.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = raycastTarget;

        if (sprite != null)
            image.type = Image.Type.Sliced;

        return obj;
    }

    private static TMP_Text CreateText(string name, Transform parent, string text, float fontSize, TextAlignmentOptions alignment, Color color)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        obj.transform.SetParent(parent, false);
        TMP_Text tmp = obj.GetComponent<TMP_Text>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = color;
        tmp.raycastTarget = false;
        tmp.enableAutoSizing = false;
        return tmp;
    }

    private static GameObject CreateButton(string name, Transform parent, Sprite normalSprite, Sprite selectedSprite)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        obj.transform.SetParent(parent, false);
        Image image = obj.GetComponent<Image>();
        image.sprite = normalSprite;
        image.color = new Color32(24, 32, 19, 230);
        image.type = normalSprite != null ? Image.Type.Sliced : Image.Type.Simple;
        image.raycastTarget = true;

        Button button = obj.GetComponent<Button>();
        SpriteState state = button.spriteState;
        state.highlightedSprite = selectedSprite;
        state.pressedSprite = selectedSprite;
        state.selectedSprite = selectedSprite;
        button.spriteState = state;
        return obj;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    private static void Center(RectTransform rect, Vector2 size)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;
    }

    private static void Anchor(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
    }

    private static Sprite LoadSprite(string fileName)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtFolder}/{fileName}");
    }

    private static void SetObject(Object target, string propertyName, Object value)
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);

        if (property != null)
            property.objectReferenceValue = value;

        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SavePrefab(GameObject root, string path)
    {
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
    }
}
#endif
