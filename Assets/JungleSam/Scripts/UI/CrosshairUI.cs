using UnityEngine;
using UnityEngine.UI;

public class CrosshairUI : MonoBehaviour
{
    private const string PrefColorR = "Crosshair_Color_R";
    private const string PrefColorG = "Crosshair_Color_G";
    private const string PrefColorB = "Crosshair_Color_B";
    private const string PrefColorA = "Crosshair_Color_A";
    private const string PrefScale = "Crosshair_OverallScale";

    [Header("Style")]
    [SerializeField] private Color crosshairColor = Color.white;
    [SerializeField, Min(0.1f)] private float overallScale = 1f;
    [SerializeField, Min(1f)] private float lineLength = 18f;
    [SerializeField, Min(1f)] private float lineThickness = 3f;
    [SerializeField, Min(0f)] private float gap = 8f;
    [SerializeField, Min(1f)] private float centerDotSize = 4f;
    [SerializeField] private bool showCenterDot = true;

    [Header("UI Elements")]
    [SerializeField] private Image top;
    [SerializeField] private Image bottom;
    [SerializeField] private Image left;
    [SerializeField] private Image right;
    [SerializeField] private Image centerDot;

    public Color CrosshairColor => crosshairColor;
    public float OverallScale => overallScale;
    public float LineLength => lineLength;
    public float LineThickness => lineThickness;
    public float Gap => gap;
    public float CenterDotSize => centerDotSize;
    public bool ShowCenterDot => showCenterDot;

    private void Awake()
    {
        LoadSavedSettings();
        EnsureElements();
        ApplyStyle();
    }

    private void OnValidate()
    {
        overallScale = Mathf.Max(0.1f, overallScale);
        lineLength = Mathf.Max(1f, lineLength);
        lineThickness = Mathf.Max(1f, lineThickness);
        gap = Mathf.Max(0f, gap);
        centerDotSize = Mathf.Max(1f, centerDotSize);

        ApplyStyle();
    }

    public void ApplyStyle()
    {
        ApplyImage(top, new Vector2(lineThickness, lineLength), new Vector2(0f, gap + lineLength * 0.5f), true);
        ApplyImage(bottom, new Vector2(lineThickness, lineLength), new Vector2(0f, -(gap + lineLength * 0.5f)), true);
        ApplyImage(left, new Vector2(lineLength, lineThickness), new Vector2(-(gap + lineLength * 0.5f), 0f), true);
        ApplyImage(right, new Vector2(lineLength, lineThickness), new Vector2(gap + lineLength * 0.5f, 0f), true);
        ApplyImage(centerDot, new Vector2(centerDotSize, centerDotSize), Vector2.zero, showCenterDot);

        transform.localScale = Vector3.one * overallScale;
    }

    public void SetCrosshairColor(Color color)
    {
        crosshairColor = color;
        SaveColor();
        ApplyStyle();
    }

    public void SetCrosshairScale(float scale)
    {
        overallScale = Mathf.Max(0.1f, scale);
        SaveScale();
        ApplyStyle();
    }

    public void SetLineLength(float length)
    {
        lineLength = Mathf.Max(1f, length);
        ApplyStyle();
    }

    public void SetLineThickness(float thickness)
    {
        lineThickness = Mathf.Max(1f, thickness);
        ApplyStyle();
    }

    public void SetGap(float value)
    {
        gap = Mathf.Max(0f, value);
        ApplyStyle();
    }

    public void SetCenterDotSize(float size)
    {
        centerDotSize = Mathf.Max(1f, size);
        ApplyStyle();
    }

    public void SetShowCenterDot(bool isVisible)
    {
        showCenterDot = isVisible;
        ApplyStyle();
    }

    public void SetScaleFromSlider(float value)
    {
        SetCrosshairScale(value);
    }

    public void SetRed(float value)
    {
        SetCrosshairColor(new Color(Mathf.Clamp01(value), crosshairColor.g, crosshairColor.b, crosshairColor.a));
    }

    public void SetGreen(float value)
    {
        SetCrosshairColor(new Color(crosshairColor.r, Mathf.Clamp01(value), crosshairColor.b, crosshairColor.a));
    }

    public void SetBlue(float value)
    {
        SetCrosshairColor(new Color(crosshairColor.r, crosshairColor.g, Mathf.Clamp01(value), crosshairColor.a));
    }

    public void SetAlpha(float value)
    {
        SetCrosshairColor(new Color(crosshairColor.r, crosshairColor.g, crosshairColor.b, Mathf.Clamp01(value)));
    }

    private void EnsureElements()
    {
        top = EnsureImage(top, "Top");
        bottom = EnsureImage(bottom, "Bottom");
        left = EnsureImage(left, "Left");
        right = EnsureImage(right, "Right");
        centerDot = EnsureImage(centerDot, "CenterDot");
    }

    private Image EnsureImage(Image image, string objectName)
    {
        if (image != null)
            return image;

        Transform existingChild = transform.Find(objectName);
        if (existingChild != null && existingChild.TryGetComponent(out Image existingImage))
            return existingImage;

        GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(transform, false);

        Image newImage = imageObject.GetComponent<Image>();
        newImage.raycastTarget = false;
        return newImage;
    }

    private void ApplyImage(Image image, Vector2 size, Vector2 position, bool isVisible)
    {
        if (image == null)
            return;

        RectTransform rectTransform = image.rectTransform;
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;

        image.color = crosshairColor;
        image.raycastTarget = false;
        image.gameObject.SetActive(isVisible);
    }

    private void LoadSavedSettings()
    {
        if (PlayerPrefs.HasKey(PrefColorR))
        {
            crosshairColor = new Color(
                PlayerPrefs.GetFloat(PrefColorR, crosshairColor.r),
                PlayerPrefs.GetFloat(PrefColorG, crosshairColor.g),
                PlayerPrefs.GetFloat(PrefColorB, crosshairColor.b),
                PlayerPrefs.GetFloat(PrefColorA, crosshairColor.a));
        }

        overallScale = PlayerPrefs.GetFloat(PrefScale, overallScale);
    }

    private void SaveColor()
    {
        PlayerPrefs.SetFloat(PrefColorR, crosshairColor.r);
        PlayerPrefs.SetFloat(PrefColorG, crosshairColor.g);
        PlayerPrefs.SetFloat(PrefColorB, crosshairColor.b);
        PlayerPrefs.SetFloat(PrefColorA, crosshairColor.a);
        PlayerPrefs.Save();
    }

    private void SaveScale()
    {
        PlayerPrefs.SetFloat(PrefScale, overallScale);
        PlayerPrefs.Save();
    }
}
