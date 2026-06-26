using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    [SerializeField] private string mainMenuSceneName = "Scene_MainMenu";

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

        if (exitButton != null)
        {
            exitButton.onClick.RemoveListener(ExitToMainMenu);
            exitButton.onClick.AddListener(ExitToMainMenu);
        }
    }

    private void OnDisable()
    {
        if (respawnButton != null)
            respawnButton.onClick.RemoveListener(HandleRespawnClicked);

        if (exitButton != null)
            exitButton.onClick.RemoveListener(ExitToMainMenu);

        GameplayAudioSilencer.Unmute(this);
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

        GameplayAudioSilencer.Mute(this);
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

        GameplayAudioSilencer.Unmute(this);
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

    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;
        SaveLoadContext.Clear();
        GameplayAudioSilencer.Unmute(this);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            Debug.LogWarning("DeathUIController cannot return to main menu because mainMenuSceneName is empty.", this);
            return;
        }

        SceneManager.LoadScene(mainMenuSceneName);
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

public static class GameplayAudioSilencer
{
    private static readonly HashSet<object> Owners = new HashSet<object>();
    private static bool _previousListenerPause;
    private static float _previousListenerVolume = 1f;

    public static void Mute(object owner)
    {
        if (owner == null)
            return;

        if (Owners.Count == 0)
        {
            _previousListenerPause = AudioListener.pause;
            _previousListenerVolume = AudioListener.volume;
        }

        Owners.Add(owner);
        ApplyMutedState();
    }

    public static void Unmute(object owner)
    {
        if (owner == null)
            return;

        Owners.Remove(owner);

        if (Owners.Count == 0)
            RestorePreviousState();
        else
            ApplyMutedState();
    }

    private static void ApplyMutedState()
    {
        AudioListener.pause = true;
        AudioListener.volume = 0f;
    }

    private static void RestorePreviousState()
    {
        AudioListener.pause = _previousListenerPause;
        AudioListener.volume = _previousListenerVolume;
    }
}
