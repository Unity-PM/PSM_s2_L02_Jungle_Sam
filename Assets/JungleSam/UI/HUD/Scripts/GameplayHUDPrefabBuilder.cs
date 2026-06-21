using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
public class GameplayHUDPrefabBuilder : MonoBehaviour
{
    private const string ArtRoot = "Assets/JungleSam/UI/Art/JungleSam_HUD_UI_Assets/Cleaned/";

    private static readonly Color PanelColor = new Color(0.005f, 0.008f, 0.006f, 0.82f);
    private static readonly Color BorderColor = new Color(0.42f, 0.52f, 0.25f, 0.38f);
    private static readonly Color MainGreen = new Color(0.55f, 0.65f, 0.32f, 1f);
    private static readonly Color TextWhite = new Color(0.86f, 0.88f, 0.78f, 1f);
    private static readonly Color TextDim = new Color(0.62f, 0.66f, 0.55f, 1f);
    private static readonly Color PanelDarkBackgroundColor = new Color32(4, 7, 5, 190);
    private static readonly Color PanelFrameColor = new Color32(154, 174, 84, 170);

    [Header("Sprites")]
    [SerializeField] private Sprite panelBackgroundSprite;
    [SerializeField] private Sprite panelFrameSprite;
    [SerializeField] private Sprite buttonDarkSprite;
    [SerializeField] private Sprite buttonSelectedSprite;
    [SerializeField] private Sprite crosshairSprite;
    [SerializeField] private Sprite scanlinesSprite;
    [SerializeField] private Sprite barBackgroundSprite;
    [SerializeField] private Sprite healthFillSprite;
    [SerializeField] private Sprite armorFillSprite;
    [SerializeField] private Sprite barFrameSprite;
    [SerializeField] private Sprite combatHpIconSprite;
    [SerializeField] private Sprite combatArmorIconSprite;
    [SerializeField] private Sprite combatAmmoIconSprite;
    [SerializeField] private Sprite combatObjectiveIconSprite;
    [SerializeField] private Sprite combatInteractionIconSprite;
    [SerializeField] private Sprite combatCheckpointIconSprite;

    [Header("Automation")]
    [SerializeField] private bool allowDestructiveRebuild = false;

    [ContextMenu("Rebuild HUD")]
    public void RebuildHUD()
    {
        if (!allowDestructiveRebuild)
        {
            Debug.LogWarning("Rebuild HUD is blocked. Enable Allow Destructive Rebuild only when you intentionally want to delete and recreate HUD children.");
            return;
        }

        AutoLoadSprites();

        Transform existing = transform.Find("SafeArea");

        if (existing != null)
            DestroyGeneratedObject(existing.gameObject);

        Transform overlay = transform.Find("HUD_Overlay");

        if (overlay != null)
            DestroyGeneratedObject(overlay.gameObject);

        BuildHUD();
    }

    [ContextMenu("Apply HUD Art Sprites And Rebuild")]
    public void ApplyArtSpritesAndRebuild()
    {
        if (!allowDestructiveRebuild)
        {
            Debug.LogWarning("Apply HUD Art Sprites And Rebuild is blocked. Enable Allow Destructive Rebuild only for intentional full HUD regeneration.");
            return;
        }

        panelBackgroundSprite = LoadSpriteOrCreateFallback("UI_Panel_Background.png", new Vector4(80f, 80f, 80f, 80f));
        panelFrameSprite = LoadSpriteOrCreateFallback("UI_Panel_Frame.png", new Vector4(70f, 70f, 70f, 70f));
        buttonDarkSprite = LoadSpriteOrCreateFallback("UI_Button_Dark.png", new Vector4(90f, 40f, 90f, 40f));
        buttonSelectedSprite = LoadSpriteOrCreateFallback("UI_Button_Selected.png", new Vector4(90f, 40f, 90f, 40f));
        crosshairSprite = LoadSpriteOrCreateFallback("UI_Crosshair.png", Vector4.zero);
        scanlinesSprite = LoadSpriteOrCreateFallback("UI_Overlay_Scanlines.png", Vector4.zero);
        LoadSlicedHUDSprites(true);
        RebuildHUD();
    }

    private void EnsureCanvasComponents()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
            canvas = gameObject.AddComponent<Canvas>();

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler == null)
            scaler = gameObject.AddComponent<CanvasScaler>();

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        if (GetComponent<GraphicRaycaster>() == null)
            gameObject.AddComponent<GraphicRaycaster>();
    }

    private void BuildHUD()
    {
        AutoLoadSprites();

        RectTransform overlay = CreateRect("HUD_Overlay", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Image scanlines = CreateStretchImage("UI_Overlay_Scanlines", overlay, scanlinesSprite, new Color(1f, 1f, 1f, 0.11f));
        scanlines.raycastTarget = false;

        RectTransform safeArea = CreateRect("SafeArea", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        CreateTopLeftStatus(safeArea);
        CreateTopRightBranding(safeArea);
        CreateObjectivePanel(safeArea);
        CreateStatsPanel(safeArea);
        CreateAmmoPanel(safeArea);
        CreateCrosshair(safeArea);
        CreateInteractionPrompt(safeArea);
        CreateNotification(safeArea);
        EnsureMainPanelLayers();

        GameplayHUDController controller = GetComponent<GameplayHUDController>();
        if (controller != null)
        {
            controller.AutoWireFromChildren();
        }
    }

    private void CreateTopLeftStatus(RectTransform parent)
    {
        RectTransform panel = CreatePanel("TopLeft_StatusPanel", parent, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -22f), new Vector2(300f, 44f), 0.5f);
        CreateText("StatusText", panel, "GROMNET // SECURE CHANNEL\nSTATUS: ONLINE", 16, TextDim, TextAlignmentOptions.Left, new Vector2(18f, -6f), new Vector2(245f, 36f));
        Image dot = CreateImage("OnlineDot", panel, MainGreen, new Vector2(270f, -22f), new Vector2(9f, 9f));
        dot.type = Image.Type.Simple;
    }

    private void CreateTopRightBranding(RectTransform parent)
    {
        RectTransform panel = CreatePanel("TopRight_BlackOrchidPanel", parent, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-34f, -26f), new Vector2(310f, 104f), 0.42f);
        CreateText("FactionTitleText", panel, "BLACK ORCHID", 23, TextWhite, TextAlignmentOptions.Right, new Vector2(-20f, -9f), new Vector2(255f, 27f));
        CreateText("DivisionText", panel, "BIOTIC DIVISION", 15, TextDim, TextAlignmentOptions.Right, new Vector2(-20f, -36f), new Vector2(255f, 20f));
        CreateText("ProjectText", panel, "PROJECT: ECLIPSE", 15, TextDim, TextAlignmentOptions.Right, new Vector2(-20f, -58f), new Vector2(255f, 20f));
        CreateText("ClearanceText", panel, "CLEARANCE: OMEGA", 15, TextDim, TextAlignmentOptions.Right, new Vector2(-20f, -80f), new Vector2(255f, 20f));
    }

    private void CreateObjectivePanel(RectTransform parent)
    {
        RectTransform panel = CreatePanel("ObjectivePanel", parent, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(42f, -96f), new Vector2(520f, 190f), 0.72f);
        CreateText("ObjectiveHeaderText", panel, "CEL MISJI", 34, MainGreen, TextAlignmentOptions.Left, new Vector2(92f, -28f), new Vector2(340f, 42f));
        CreateImage("SeparatorLine", panel, BorderColor, new Vector2(278f, -78f), new Vector2(390f, 2f));
        CreateText("MainObjectiveText", panel, "Znajdź źródło sygnału", 24, TextWhite, TextAlignmentOptions.Left, new Vector2(90f, -94f), new Vector2(380f, 34f));
        CreateText("SecondaryObjectiveText", panel, "Przedostań się przez nabrzeże", 18, TextDim, TextAlignmentOptions.Left, new Vector2(90f, -134f), new Vector2(380f, 28f));
        CreateIcon("ObjectiveIcon", panel, combatObjectiveIconSprite, "□", 34, MainGreen, new Vector2(34f, -34f), new Vector2(46f, 46f));
    }

    private void CreateStatsPanel(RectTransform parent)
    {
        RectTransform panel = CreatePanel("PlayerStatsPanel", parent, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(42f, 42f), new Vector2(430f, 158f), 0.7f);
        CreateText("HealthText", panel, "HP 100", 23, MainGreen, TextAlignmentOptions.Left, new Vector2(70f, -24f), new Vector2(160f, 30f));
        CreateBar("HealthBar", panel, new Vector2(88f, -68f), new Vector2(270f, 16f), 1f, healthFillSprite);
        CreateText("ArmorText", panel, "PANCERZ 0", 20, TextDim, TextAlignmentOptions.Left, new Vector2(70f, -94f), new Vector2(180f, 28f));
        CreateBar("ArmorBar", panel, new Vector2(88f, -130f), new Vector2(270f, 14f), 0f, armorFillSprite);
        CreateIcon("HealthIcon", panel, combatHpIconSprite, "+", 36, MainGreen, new Vector2(26f, -25f), new Vector2(38f, 38f));
        CreateIcon("ArmorIcon", panel, combatArmorIconSprite, "□", 27, TextDim, new Vector2(28f, -96f), new Vector2(36f, 36f));
    }

    private void CreateAmmoPanel(RectTransform parent)
    {
        RectTransform panel = CreatePanel("AmmoPanel", parent, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-42f, 42f), new Vector2(390f, 150f), 0.72f);
        CreateText("WeaponNameText", panel, "AK-47", 17, MainGreen, TextAlignmentOptions.Left, new Vector2(32f, -16f), new Vector2(220f, 24f));
        CreateText("AmmoText", panel, "30 / 120", 44, TextWhite, TextAlignmentOptions.Left, new Vector2(32f, -48f), new Vector2(230f, 54f));
        CreateText("ReserveAmmoText", panel, "RESERVE 120", 13, TextDim, TextAlignmentOptions.Left, new Vector2(260f, -66f), new Vector2(100f, 24f));
        CreateText("AmmoTypeText", panel, "7.62x39mm", 14, MainGreen, TextAlignmentOptions.Right, new Vector2(-30f, -112f), new Vector2(160f, 22f));
        CreateIcon("AmmoIcon", panel, combatAmmoIconSprite, "|||", 24, MainGreen, new Vector2(310f, -20f), new Vector2(52f, 42f));
    }

    private void CreateCrosshair(RectTransform parent)
    {
        RectTransform root = CreateRect("CenterCrosshair", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(56f, 56f));
        Image crosshair = root.gameObject.AddComponent<Image>();
        crosshair.sprite = crosshairSprite;
        crosshair.color = new Color(0.86f, 0.9f, 0.78f, 0.78f);
        crosshair.raycastTarget = false;

        if (crosshairSprite == null)
        {
            CreateImage("Line_Top", root, TextWhite, new Vector2(0f, 22f), new Vector2(2f, 22f));
            CreateImage("Line_Bottom", root, TextWhite, new Vector2(0f, -22f), new Vector2(2f, 22f));
            CreateImage("Line_Left", root, TextWhite, new Vector2(-22f, 0f), new Vector2(22f, 2f));
            CreateImage("Line_Right", root, TextWhite, new Vector2(22f, 0f), new Vector2(22f, 2f));
        }
    }

    private void CreateInteractionPrompt(RectTransform parent)
    {
        RectTransform panel = CreateButtonPanel("InteractionPromptPanel", parent, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 128f), new Vector2(400f, 64f), buttonDarkSprite, 0.82f);
        panel.gameObject.AddComponent<CanvasGroup>();
        panel.gameObject.AddComponent<InteractionPromptUI>();
        CreatePanel("KeyBox", panel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -12f), new Vector2(48f, 40f), 0.55f);
        CreateText("KeyText", panel, "E", 23, MainGreen, TextAlignmentOptions.Center, new Vector2(28f, -12f), new Vector2(48f, 38f));
        CreateText("PromptText", panel, "PODNIEŚ RADIO", 21, TextWhite, TextAlignmentOptions.Left, new Vector2(94f, -16f), new Vector2(250f, 34f));
        CreateIcon("InteractionIcon", panel, combatInteractionIconSprite, "", 16, MainGreen, new Vector2(350f, -13f), new Vector2(34f, 34f));
        InteractionPromptUI promptUI = panel.GetComponent<InteractionPromptUI>();
        if (promptUI != null)
            promptUI.Hide();
    }

    private void CreateNotification(RectTransform parent)
    {
        RectTransform panel = CreateButtonPanel("NotificationPanel", parent, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-54f, 110f), new Vector2(360f, 76f), buttonSelectedSprite, 0.78f);
        panel.gameObject.AddComponent<HUDNotificationUI>();
        panel.gameObject.AddComponent<CanvasGroup>();
        CreateIcon("NotificationIcon", panel, combatCheckpointIconSprite, "✓", 30, MainGreen, new Vector2(20f, -12f), new Vector2(44f, 42f));
        CreateText("NotificationText", panel, "CEL ZAKTUALIZOWANY", 22, MainGreen, TextAlignmentOptions.Left, new Vector2(74f, -16f), new Vector2(230f, 34f));
        HUDNotificationUI notificationUI = panel.GetComponent<HUDNotificationUI>();
        if (notificationUI != null)
            notificationUI.Hide();
    }

    [ContextMenu("Fix HUD Panel Background Layers")]
    public void FixPanelBackgroundLayers()
    {
        AutoLoadSprites();
        EnsureMainPanelLayers();
    }

    private void EnsureMainPanelLayers()
    {
        string[] panelPaths =
        {
            "SafeArea/ObjectivePanel",
            "SafeArea/PlayerStatsPanel",
            "SafeArea/AmmoPanel",
            "SafeArea/InteractionPromptPanel",
            "SafeArea/NotificationPanel",
            "SafeArea/TopLeft_StatusPanel",
            "SafeArea/TopRight_BlackOrchidPanel"
        };

        foreach (string panelPath in panelPaths)
        {
            Transform panel = transform.Find(panelPath);

            if (panel is RectTransform rectTransform)
                EnsurePanelLayerImages(rectTransform);
        }
    }

    private RectTransform CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size, float alpha = 0.78f)
    {
        RectTransform rect = CreateRect(name, parent, anchorMin, anchorMax, anchoredPosition, size);
        EnsurePanelLayerImages(rect);
        return rect;
    }

    private RectTransform CreateButtonPanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size, Sprite sprite, float alpha = 0.86f)
    {
        RectTransform rect = CreateRect(name, parent, anchorMin, anchorMax, anchoredPosition, size);
        EnsurePanelLayerImages(rect);
        return rect;
    }

    private void EnsurePanelLayerImages(RectTransform panel)
    {
        if (panel == null)
            return;

        Image rootImage = panel.GetComponent<Image>();

        if (rootImage != null)
        {
            rootImage.sprite = null;
            rootImage.color = Color.clear;
            rootImage.raycastTarget = false;
        }

        RectTransform background = GetOrCreateStretchLayer(panel, "PanelDarkBackground");
        Image backgroundImage = background.GetComponent<Image>();
        backgroundImage.sprite = null;
        backgroundImage.color = PanelDarkBackgroundColor;
        backgroundImage.raycastTarget = false;
        background.SetSiblingIndex(0);

        RectTransform frame = GetOrCreateStretchLayer(panel, "PanelFrame");
        Image frameImage = frame.GetComponent<Image>();
        frameImage.sprite = panelFrameSprite != null ? panelFrameSprite : panelBackgroundSprite;
        frameImage.type = frameImage.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
        frameImage.color = PanelFrameColor;
        frameImage.raycastTarget = false;
        frame.SetSiblingIndex(1);
    }

    private RectTransform GetOrCreateStretchLayer(RectTransform parent, string layerName)
    {
        Transform existing = parent.Find(layerName);

        if (existing == null && layerName == "PanelFrame")
        {
            existing = parent.Find("Frame");

            if (existing != null)
                existing.name = layerName;
        }

        RectTransform rect;

        if (existing is RectTransform existingRect)
        {
            rect = existingRect;
        }
        else
        {
            GameObject obj = new GameObject(layerName, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(parent, false);
            rect = obj.GetComponent<RectTransform>();
        }

        if (rect.GetComponent<Image>() == null)
            rect.gameObject.AddComponent<Image>();

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return rect;
    }

    private void CreateBar(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, float fill, Sprite fillSprite)
    {
        RectTransform root = CreateRect(name, parent, new Vector2(0f, 1f), new Vector2(0f, 1f), anchoredPosition, size);
        Image background = CreateImage("Background", root, new Color(0f, 0f, 0f, 0.62f), Vector2.zero, size);
        background.sprite = barBackgroundSprite;
        background.type = barBackgroundSprite != null ? Image.Type.Sliced : Image.Type.Simple;

        Image fillImage = CreateImage("Fill", root, new Color(MainGreen.r, MainGreen.g, MainGreen.b, 0.88f), Vector2.zero, size);
        fillImage.sprite = fillSprite;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = 0;
        fillImage.fillAmount = fill;

        if (barFrameSprite != null)
        {
            Image frame = CreateStretchImage("Frame", root, barFrameSprite, new Color(MainGreen.r, MainGreen.g, MainGreen.b, 0.7f), new Vector2(-4f, -4f), new Vector2(4f, 4f));
            frame.type = Image.Type.Sliced;
        }
    }

    private void CreateIcon(string name, Transform parent, Sprite sprite, string fallbackText, int fallbackFontSize, Color color, Vector2 anchoredPosition, Vector2 size)
    {
        if (sprite == null)
        {
            if (!string.IsNullOrEmpty(fallbackText))
                CreateText(name, parent, fallbackText, fallbackFontSize, color, TextAlignmentOptions.Center, anchoredPosition, size);

            return;
        }

        RectTransform rect = CreateRect(name, parent, new Vector2(0f, 1f), new Vector2(0f, 1f), anchoredPosition, size);
        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.preserveAspect = true;
        image.raycastTarget = false;
    }

    private TMP_Text CreateText(string name, Transform parent, string text, int fontSize, Color color, TextAlignmentOptions alignment, Vector2 anchoredPosition, Vector2 size)
    {
        RectTransform rect = CreateRect(name, parent, new Vector2(0f, 1f), new Vector2(0f, 1f), anchoredPosition, size);
        TextMeshProUGUI tmp = rect.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.raycastTarget = false;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        return tmp;
    }

    private Image CreateImage(string name, Transform parent, Color color, Vector2 anchoredPosition, Vector2 size)
    {
        RectTransform rect = CreateRect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), anchoredPosition, size);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private Image CreateStretchImage(string name, Transform parent, Sprite sprite, Color color)
    {
        return CreateStretchImage(name, parent, sprite, color, Vector2.zero, Vector2.zero);
    }

    private Image CreateStretchImage(string name, Transform parent, Sprite sprite, Color color, Vector2 offsetMin, Vector2 offsetMax)
    {
        RectTransform rect = CreateRect(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(anchorMin.x == anchorMax.x ? anchorMin.x : 0.5f, anchorMin.y == anchorMax.y ? anchorMin.y : 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        return rect;
    }

    private static void DestroyGeneratedObject(Object obj)
    {
        if (obj == null)
            return;

        if (Application.isPlaying)
            Destroy(obj);
        else
            DestroyImmediate(obj);
    }

    private void AutoLoadSprites()
    {
#if UNITY_EDITOR
        panelBackgroundSprite ??= LoadSpriteOrCreateFallback("UI_Panel_Background.png", new Vector4(80f, 80f, 80f, 80f));
        panelFrameSprite ??= LoadSpriteOrCreateFallback("UI_Panel_Frame.png", new Vector4(70f, 70f, 70f, 70f));
        buttonDarkSprite ??= LoadSpriteOrCreateFallback("UI_Button_Dark.png", new Vector4(90f, 40f, 90f, 40f));
        buttonSelectedSprite ??= LoadSpriteOrCreateFallback("UI_Button_Selected.png", new Vector4(90f, 40f, 90f, 40f));
        crosshairSprite ??= LoadSpriteOrCreateFallback("UI_Crosshair.png", Vector4.zero);
        scanlinesSprite ??= LoadSpriteOrCreateFallback("UI_Overlay_Scanlines.png", Vector4.zero);
        LoadSlicedHUDSprites(false);
#endif
    }

#if UNITY_EDITOR
    private void LoadSlicedHUDSprites(bool forceAssign)
    {
        Sprite[] bars = LoadSlicedSprites("UI_Bars_Sheet.png");
        AssignSprite(ref barBackgroundSprite, GetSpriteAt(bars, 0), forceAssign);
        AssignSprite(ref healthFillSprite, GetSpriteAt(bars, 1), forceAssign);
        AssignSprite(ref armorFillSprite, GetSpriteAt(bars, 2), forceAssign);
        AssignSprite(ref barFrameSprite, GetSpriteAt(bars, 3), forceAssign);

        Sprite[] combatIcons = LoadSlicedSprites("UI_Icons_Combat_Sheet.png");
        AssignSprite(ref combatHpIconSprite, GetSpriteAt(combatIcons, 0), forceAssign);
        AssignSprite(ref combatArmorIconSprite, GetSpriteAt(combatIcons, 1), forceAssign);
        AssignSprite(ref combatAmmoIconSprite, GetSpriteAt(combatIcons, 2), forceAssign);
        AssignSprite(ref combatObjectiveIconSprite, GetSpriteAt(combatIcons, 3), forceAssign);
        AssignSprite(ref combatInteractionIconSprite, GetSpriteAt(combatIcons, 4), forceAssign);
        AssignSprite(ref combatCheckpointIconSprite, GetSpriteAt(combatIcons, 5), forceAssign);

        if (forceAssign)
            Debug.Log($"HUD sprite loader: bars={bars.Length}, combatIcons={combatIcons.Length}");
    }

    private static void AssignSprite(ref Sprite target, Sprite source, bool forceAssign)
    {
        if (source == null)
            return;

        if (forceAssign || target == null)
            target = source;
    }

    private static Sprite[] LoadSlicedSprites(string fileName)
    {
        Object[] objects = AssetDatabase.LoadAllAssetsAtPath(ArtRoot + fileName);
        List<Sprite> sprites = new List<Sprite>();

        foreach (Object obj in objects)
        {
            if (obj is Sprite sprite)
                sprites.Add(sprite);
        }

        sprites.Sort((left, right) => GetTrailingIndex(left.name).CompareTo(GetTrailingIndex(right.name)));
        return sprites.ToArray();
    }

    private static Sprite GetSpriteAt(Sprite[] sprites, int index)
    {
        if (sprites == null || index < 0 || index >= sprites.Length)
            return null;

        return sprites[index];
    }

    private static int GetTrailingIndex(string name)
    {
        int underscoreIndex = name.LastIndexOf('_');

        if (underscoreIndex < 0 || underscoreIndex >= name.Length - 1)
            return int.MaxValue;

        string indexText = name.Substring(underscoreIndex + 1);
        return int.TryParse(indexText, out int index) ? index : int.MaxValue;
    }

    private static Sprite LoadSpriteOrCreateFallback(string fileName, Vector4 border)
    {
        string path = ArtRoot + fileName;
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

        if (sprite != null)
            return sprite;

        Object[] objects = AssetDatabase.LoadAllAssetsAtPath(path);

        foreach (Object obj in objects)
        {
            if (obj is Sprite subSprite)
                return subSprite;
        }

        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

        if (texture == null)
        {
            Debug.LogWarning($"HUD art sprite not found: {path}");
            return null;
        }

        return Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            border
        );
    }
#endif
}
