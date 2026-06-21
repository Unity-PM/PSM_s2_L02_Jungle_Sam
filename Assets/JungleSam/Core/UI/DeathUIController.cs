using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class DeathUIController : MonoBehaviour
{
    [Header("Death Panel")]
    [FormerlySerializedAs("deathPanel")]
    [SerializeField] private GameObject root;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button respawnButton;
    [SerializeField] private Button exitButton;

    [Header("Input")]
    [SerializeField] private bool manageCursorWhileOpen = true;

    private Action _respawnCallback;
    private bool _isOpen;
    private bool _previousCursorVisible;
    private CursorLockMode _previousCursorLockMode;

    public bool CanUseRespawnButton
    {
        get
        {
            AutoWire();
            return respawnButton != null;
        }
    }

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
        if (root == null)
        {
            Transform deathRootTransform = transform.Find("DeathRoot");
            root = deathRootTransform != null ? deathRootTransform.gameObject : gameObject;
        }

        canvasGroup ??= GetComponent<CanvasGroup>();
        respawnButton ??= FindButton("RespawnButton");
        exitButton ??= FindButton("ExitButton");

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Show()
    {
        AutoWire();

        if (root != null)
            root.SetActive(true);

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        if (manageCursorWhileOpen)
        {
            _previousCursorVisible = Cursor.visible;
            _previousCursorLockMode = Cursor.lockState;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        _isOpen = true;
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

        if (manageCursorWhileOpen && _isOpen)
        {
            Cursor.visible = _previousCursorVisible;
            Cursor.lockState = _previousCursorLockMode;
        }

        _isOpen = false;
    }

    public void SetRespawnCallback(Action callback)
    {
        _respawnCallback = callback;
    }

    private void HandleRespawnClicked()
    {
        _respawnCallback?.Invoke();
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
