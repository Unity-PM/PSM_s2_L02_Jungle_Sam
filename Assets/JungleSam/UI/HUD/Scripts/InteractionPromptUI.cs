using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class InteractionPromptUI : MonoBehaviour
{
    [SerializeField] private GameObject promptRoot;
    [SerializeField] private TMP_Text keyText;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private CanvasGroup canvasGroup;

    private void Awake()
    {
        AutoWire();
        Hide();
    }

    private void Start()
    {
        AutoWire();
        Hide();
    }

    [ContextMenu("Auto Wire")]
    public void AutoWire()
    {
        promptRoot ??= gameObject;
        keyText ??= transform.Find("KeyText")?.GetComponent<TMP_Text>();
        promptText ??= transform.Find("PromptText")?.GetComponent<TMP_Text>();
        canvasGroup ??= GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Show(string key, string text)
    {
        AutoWire();

        if (promptRoot != null && !promptRoot.activeSelf)
            promptRoot.SetActive(true);

        if (keyText != null)
            keyText.text = NormalizeKey(key);

        if (promptText != null)
            promptText.text = NormalizePrompt(text);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void Show(string fullText)
    {
        ParseFullPrompt(fullText, out string key, out string text);
        Show(key, text);
    }

    public void Hide()
    {
        AutoWire();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private static void ParseFullPrompt(string fullText, out string key, out string text)
    {
        key = "E";
        text = fullText;

        if (string.IsNullOrWhiteSpace(fullText))
        {
            text = "INTERAKCJA";
            return;
        }

        string trimmed = fullText.Trim();

        if (trimmed.StartsWith("[") && trimmed.Contains("]"))
        {
            int endIndex = trimmed.IndexOf(']');
            key = trimmed.Substring(1, endIndex - 1);
            text = trimmed.Substring(endIndex + 1).TrimStart(' ', '-');
            return;
        }

        int separatorIndex = trimmed.IndexOf('-');

        if (separatorIndex > 0 && separatorIndex <= 4)
        {
            key = trimmed.Substring(0, separatorIndex).Trim();
            text = trimmed.Substring(separatorIndex + 1).Trim();
        }
    }

    private static string NormalizeKey(string key)
    {
        return string.IsNullOrWhiteSpace(key) ? "E" : key.Trim().Trim('[', ']').ToUpperInvariant();
    }

    private static string NormalizePrompt(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "INTERAKCJA";

        string normalized = text.Trim();

        if (normalized.StartsWith("-"))
            normalized = normalized.Substring(1).Trim();

        return normalized.ToUpperInvariant();
    }
}
