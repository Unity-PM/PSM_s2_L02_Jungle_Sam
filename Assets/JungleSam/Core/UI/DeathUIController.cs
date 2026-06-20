using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class DeathUIController : MonoBehaviour
{
    [Header("Root")]
    [FormerlySerializedAs("deathPanel")]
    [SerializeField] private GameObject root;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Text")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private TMP_Text respawnButtonText;
    [SerializeField] private TMP_Text exitButtonText;

    [Header("Buttons")]
    [SerializeField] private Button respawnButton;
    [SerializeField] private Button exitButton;

    [Header("Defaults")]
    [SerializeField] private string defaultTitle = "ELIMINACJA";
    [SerializeField] private string defaultSubtitle = "Utracono łączność bojową";
    [TextArea(2, 5)]
    [SerializeField] private string defaultBody = "Ostatni checkpoint zostanie przywrócony. Utrzymaj pozycję i kontynuuj operację.";
    [SerializeField] private string defaultRespawnButtonText = "POWRÓT DO CHECKPOINTU";
    [SerializeField] private string defaultExitButtonText = "WYJDŹ DO MENU";

    [Header("Legacy Fallback")]
    [SerializeField] private string respawningText = "Respawning...";
    [SerializeField] private TMP_Text tmpText;
    [SerializeField] private Text legacyText;

    private Action _respawnCallback;

    private void Reset()
    {
        AutoWire();
    }

    private void Awake()
    {
        AutoWire();
        Hide();
    }

    private void OnEnable()
    {
        AutoWire();

        if (respawnButton != null)
        {
            respawnButton.onClick.RemoveListener(HandleRespawnClicked);
            respawnButton.onClick.AddListener(HandleRespawnClicked);
        }
    }

    private void OnDisable()
    {
        if (respawnButton != null)
            respawnButton.onClick.RemoveListener(HandleRespawnClicked);
    }

    [ContextMenu("Auto Wire")]
    public void AutoWire()
    {
        root ??= gameObject;
        canvasGroup ??= GetComponent<CanvasGroup>();
        titleText ??= FindText("TitleText");
        subtitleText ??= FindText("SubtitleText");
        bodyText ??= FindText("BodyText");
        respawnButtonText ??= FindText("RespawnButtonText");
        exitButtonText ??= FindText("ExitButtonText");
        respawnButton ??= FindButton("RespawnButton");
        exitButton ??= FindButton("ExitButton");

        if (tmpText == null)
            tmpText = GetComponentInChildren<TMP_Text>(true);

        if (legacyText == null)
            legacyText = GetComponentInChildren<Text>(true);

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Show()
    {
        AutoWire();

        if (titleText != null)
            titleText.text = defaultTitle;

        if (subtitleText != null)
            subtitleText.text = defaultSubtitle;

        if (bodyText != null)
            bodyText.text = defaultBody;

        if (respawnButtonText != null)
            respawnButtonText.text = defaultRespawnButtonText;

        if (exitButtonText != null)
            exitButtonText.text = defaultExitButtonText;

        if (root != null)
            root.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        if (titleText == null && subtitleText == null && bodyText == null)
            SetText(respawningText);
    }

    public void Hide()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (root != null)
            root.SetActive(false);
    }

    public void SetRespawnCallback(Action callback)
    {
        _respawnCallback = callback;
    }

    private void HandleRespawnClicked()
    {
        _respawnCallback?.Invoke();
    }

    private void SetText(string value)
    {
        if (tmpText != null)
            tmpText.text = value;

        if (legacyText != null)
            legacyText.text = value;
    }

    private TMP_Text FindText(string childName)
    {
        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);

        foreach (TMP_Text text in texts)
        {
            if (text != null && text.name == childName)
                return text;
        }

        return null;
    }

    private Button FindButton(string childName)
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);

        foreach (Button button in buttons)
        {
            if (button != null && button.name == childName)
                return button;
        }

        return null;
    }
}
