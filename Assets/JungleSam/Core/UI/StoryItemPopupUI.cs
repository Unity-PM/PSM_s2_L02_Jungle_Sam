using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class StoryItemPopupUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject popupRoot;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Images")]
    [SerializeField] private Image dimBackground;
    [SerializeField] private Image panelBackground;
    [SerializeField] private Image panelFrame;
    [SerializeField] private Image itemImage;

    [Header("Text")]
    [SerializeField] private TMP_Text categoryLabelText;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private TMP_Text bodyText;
    [FormerlySerializedAs("buttonText")]
    [SerializeField] private TMP_Text continueButtonText;

    [Header("Controls")]
    [SerializeField] private Button continueButton;

    [Header("Optional Control Lock")]
    [FormerlySerializedAs("playerControlLock")]
    [SerializeField] private PlayerControlLock optionalPlayerControlLock;
    [SerializeField] private bool lockPlayerWhileOpen = true;

    private Action _onClosed;
    private bool _unlockPlayerOnClose;

    private void Awake()
    {
        AutoWire();
        Hide(false);
    }

    private void OnEnable()
    {
        AutoWire();

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(Continue);
            continueButton.onClick.AddListener(Continue);
        }
    }

    private void OnDisable()
    {
        if (continueButton != null)
            continueButton.onClick.RemoveListener(Continue);
    }

    [ContextMenu("Auto Wire")]
    public void AutoWire()
    {
        popupRoot ??= gameObject;
        canvasGroup ??= GetComponent<CanvasGroup>();
        dimBackground ??= FindImage("DimBackground");
        panelBackground ??= FindImage("PanelBackground");
        panelFrame ??= FindImage("PanelFrame");
        itemImage ??= FindImage("ItemImage");
        categoryLabelText ??= FindText("CategoryLabelText");
        titleText ??= FindText("TitleText");
        subtitleText ??= FindText("SubtitleText");
        bodyText ??= FindText("BodyText");
        continueButtonText ??= FindText("ContinueButtonText");
        continueButtonText ??= FindText("ButtonText");
        continueButton ??= GetComponentInChildren<Button>(true);

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (optionalPlayerControlLock == null)
            optionalPlayerControlLock = FindFirstObjectByType<PlayerControlLock>();
    }

    public void Show(StoryPopupData data, Action onClosed)
    {
        if (data == null)
        {
            Show("DOKUMENT", string.Empty, string.Empty, (Sprite)null, onClosed);
            return;
        }

        ShowInternal(
            data.CategoryLabel,
            data.Title,
            data.Subtitle,
            data.Body,
            data.ItemImage,
            data.ContinueButtonText,
            data.LockPlayerWhileOpen,
            onClosed);
    }

    public void Show(string title, string subtitle, string body, Sprite image, Action onClosed)
    {
        ShowInternal(string.Empty, title, subtitle, body, image, "KONTYNUUJ", lockPlayerWhileOpen, onClosed);
    }

    public void Show(string title, string subtitle, string body, string buttonLabel, Action onClosed)
    {
        ShowInternal(string.Empty, title, subtitle, body, null, buttonLabel, lockPlayerWhileOpen, onClosed);
    }

    private void ShowInternal(string categoryLabel, string title, string subtitle, string body, Sprite image, string buttonLabel, bool lockPlayer, Action onClosed)
    {
        AutoWire();
        _onClosed = onClosed;
        _unlockPlayerOnClose = lockPlayer && optionalPlayerControlLock != null;

        if (categoryLabelText != null)
        {
            bool hasCategory = !string.IsNullOrWhiteSpace(categoryLabel);
            categoryLabelText.gameObject.SetActive(hasCategory);
            categoryLabelText.text = hasCategory ? categoryLabel : string.Empty;
        }

        if (titleText != null)
            titleText.text = string.IsNullOrWhiteSpace(title) ? "DOKUMENT" : title;

        if (subtitleText != null)
            subtitleText.text = string.IsNullOrWhiteSpace(subtitle) ? string.Empty : subtitle;

        if (bodyText != null)
            bodyText.text = string.IsNullOrWhiteSpace(body) ? string.Empty : body;

        if (continueButtonText != null)
            continueButtonText.text = string.IsNullOrWhiteSpace(buttonLabel) ? "KONTYNUUJ" : buttonLabel;

        if (itemImage != null)
        {
            itemImage.sprite = image;
            itemImage.enabled = image != null;
        }

        if (popupRoot != null)
            popupRoot.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        if (_unlockPlayerOnClose)
            optionalPlayerControlLock.SetLocked(true);
    }

    public void Hide()
    {
        Hide(true);
    }

    private void Hide(bool unlockPlayer)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (popupRoot != null)
            popupRoot.SetActive(false);

        if (unlockPlayer && _unlockPlayerOnClose && optionalPlayerControlLock != null)
            optionalPlayerControlLock.SetLocked(false);

        _unlockPlayerOnClose = false;
    }

    public void Continue()
    {
        Action callback = _onClosed;
        _onClosed = null;

        Hide();
        callback?.Invoke();
    }

    private TMP_Text FindText(string childName)
    {
        Transform child = transform.Find(childName);

        if (child != null)
            return child.GetComponent<TMP_Text>();

        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);

        foreach (TMP_Text text in texts)
        {
            if (text != null && text.name == childName)
                return text;
        }

        return null;
    }

    private Image FindImage(string childName)
    {
        Transform child = transform.Find(childName);

        if (child != null)
            return child.GetComponent<Image>();

        Image[] images = GetComponentsInChildren<Image>(true);

        foreach (Image image in images)
        {
            if (image != null && image.name == childName)
                return image;
        }

        return null;
    }
}
