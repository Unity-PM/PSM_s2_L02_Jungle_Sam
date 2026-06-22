using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class StoryItemPopupUI : MonoBehaviour
{
    [Header("Popup")]
    [SerializeField] private GameObject popupRoot;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button continueButton;

    [Header("Input Lock")]
    [FormerlySerializedAs("playerControlLock")]
    [SerializeField] private PlayerControlLock optionalPlayerControlLock;
    [SerializeField] private bool lockPlayerWhileOpen = true;
    [SerializeField] private bool manageCursorWhileOpen = true;
    [SerializeField] private float keyboardInputGraceTime = 0.2f;

    private Action _onClosed;
    private bool _isOpen;
    private bool _unlockPlayerOnClose;
    private bool _previousCursorVisible;
    private CursorLockMode _previousCursorLockMode;
    private GameObject _dimBackgroundRoot;
    private int _openedFrame;
    private float _openedUnscaledTime;

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

    private void Update()
    {
        if (!_isOpen)
            return;

        if (Time.frameCount == _openedFrame || Time.unscaledTime - _openedUnscaledTime < keyboardInputGraceTime)
            return;

        Keyboard keyboard = Keyboard.current;

        if (keyboard != null &&
            (keyboard.enterKey.wasPressedThisFrame ||
             keyboard.numpadEnterKey.wasPressedThisFrame ||
             keyboard.spaceKey.wasPressedThisFrame ||
             keyboard.eKey.wasPressedThisFrame ||
             keyboard.escapeKey.wasPressedThisFrame))
        {
            Continue();
        }
    }

    [ContextMenu("Auto Wire")]
    public void AutoWire()
    {
        if (popupRoot == null)
        {
            Transform popupRootTransform = transform.Find("PopupRoot");
            popupRoot = popupRootTransform != null ? popupRootTransform.gameObject : gameObject;
        }

        Transform dimBackgroundTransform = transform.Find("DimBackground");
        _dimBackgroundRoot = dimBackgroundTransform != null ? dimBackgroundTransform.gameObject : null;

        canvasGroup ??= GetComponent<CanvasGroup>();
        continueButton ??= GetComponentInChildren<Button>(true);

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (optionalPlayerControlLock == null)
            optionalPlayerControlLock = FindFirstObjectByType<PlayerControlLock>(FindObjectsInactive.Include);
    }

    public void Show(StoryPopupData data, Action onClosed)
    {
        Show(onClosed);
    }

    public void Show(string title, string subtitle, string body, Sprite image, Action onClosed)
    {
        Show(onClosed);
    }

    public void Show(string title, string subtitle, string body, string buttonLabel, Action onClosed)
    {
        Show(onClosed);
    }

    public void Show(Action onClosed)
    {
        AutoWire();

        if (popupRoot == null)
        {
            Debug.LogWarning($"Story popup '{name}' has no Popup Root. Continuing without popup.", this);
            onClosed?.Invoke();
            return;
        }

        _onClosed = onClosed;
        _unlockPlayerOnClose = lockPlayerWhileOpen && optionalPlayerControlLock != null;

        gameObject.SetActive(true);
        FixOverlayCanvasRect();

        if (_dimBackgroundRoot != null)
            _dimBackgroundRoot.SetActive(true);

        popupRoot.SetActive(true);

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        if (_unlockPlayerOnClose)
            optionalPlayerControlLock.SetLocked(true);

        if (manageCursorWhileOpen)
        {
            _previousCursorVisible = Cursor.visible;
            _previousCursorLockMode = Cursor.lockState;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        _openedFrame = Time.frameCount;
        _openedUnscaledTime = Time.unscaledTime;
        _isOpen = true;
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

        if (_dimBackgroundRoot != null)
            _dimBackgroundRoot.SetActive(false);

        if (unlockPlayer && _unlockPlayerOnClose && optionalPlayerControlLock != null)
            optionalPlayerControlLock.SetLocked(false);

        if (unlockPlayer && manageCursorWhileOpen && _isOpen)
        {
            Cursor.visible = _previousCursorVisible;
            Cursor.lockState = _previousCursorLockMode;
        }

        _onClosed = null;
        _unlockPlayerOnClose = false;
        _isOpen = false;
    }

    public void Continue()
    {
        Action callback = _onClosed;
        Hide();
        callback?.Invoke();
    }

    private void FixOverlayCanvasRect()
    {
        Canvas canvas = GetComponent<Canvas>();

        if (canvas == null || canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            return;

        canvas.overrideSorting = true;

        if (canvas.sortingOrder < 200)
            canvas.sortingOrder = 200;

        if (transform is RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.localPosition = Vector3.zero;
            rectTransform.localScale = Vector3.one;
        }
    }
}
