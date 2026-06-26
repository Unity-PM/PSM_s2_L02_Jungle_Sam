using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class PauseMenuController : MonoBehaviour
{
    [Header("Pause UI")]
    [SerializeField] private GameObject root;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button checkpointButton;
    [SerializeField] private Button exitToMenuButton;

    [Header("Gameplay References")]
    [SerializeField] private PlayerControlLock playerControlLock;
    [SerializeField] private GameObject playerRoot;
    [SerializeField] private CheckpointManager checkpointManager;

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "Scene_MainMenu";

    [Header("Behaviour")]
    [SerializeField] private bool pauseOnOpen = true;
    [SerializeField] private bool unlockCursorOnPause = true;
    [SerializeField] private bool lockCursorOnResume = true;
    [SerializeField] private InputActionReference pauseAction;
    [SerializeField] private string fallbackPauseActionMapName = "Pause";
    [SerializeField] private string fallbackPauseActionName = "Pause";

    private bool _isPaused;
    private InputAction _boundPauseAction;
    private bool _boundPauseActionWasEnabled;
    private float _previousTimeScale = 1f;
    private int _lastPauseToggleFrame = -1;

    public static bool IsPaused { get; private set; }

    private void Reset()
    {
        AutoWire();
    }

    private void Awake()
    {
        AutoWire();
        HideRootOnly();
    }

    private void OnEnable()
    {
        AutoWire();

        AddListener(resumeButton, Resume);
        AddListener(settingsButton, SettingsNotImplemented);
        AddListener(checkpointButton, ReturnToCheckpoint);
        AddListener(exitToMenuButton, ExitToMenu);

        _boundPauseAction = ResolvePauseAction();

        if (_boundPauseAction != null)
        {
            _boundPauseActionWasEnabled = _boundPauseAction.enabled;
            _boundPauseAction.performed += HandlePausePerformed;

            if (!_boundPauseAction.enabled)
                _boundPauseAction.Enable();
        }
    }

    private void OnDisable()
    {
        RemoveListener(resumeButton, Resume);
        RemoveListener(settingsButton, SettingsNotImplemented);
        RemoveListener(checkpointButton, ReturnToCheckpoint);
        RemoveListener(exitToMenuButton, ExitToMenu);

        if (_boundPauseAction != null)
        {
            _boundPauseAction.performed -= HandlePausePerformed;

            if (!_boundPauseActionWasEnabled && _boundPauseAction.enabled)
                _boundPauseAction.Disable();

            _boundPauseAction = null;
        }

        GameplayAudioSilencer.Unmute(this);
    }

    private void OnDestroy()
    {
        if (_isPaused && pauseOnOpen)
            RestoreTimeScale();

        GameplayAudioSilencer.Unmute(this);

        if (_isPaused)
            IsPaused = false;
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            TogglePauseFromInput();
    }

    [ContextMenu("Auto Wire")]
    public void AutoWire()
    {
        root ??= FindChildGameObject("PauseRoot");
        backgroundImage ??= FindChildComponent<Image>("PauseBackground");
        resumeButton ??= FindChildComponent<Button>("ResumeButton");
        settingsButton ??= FindChildComponent<Button>("SettingsButton");
        checkpointButton ??= FindChildComponent<Button>("CheckpointButton");
        exitToMenuButton ??= FindChildComponent<Button>("ExitToMenuButton");

        if (playerControlLock == null)
            playerControlLock = FindFirstObjectByType<PlayerControlLock>(FindObjectsInactive.Include);

        if (playerRoot == null && playerControlLock != null)
            playerRoot = playerControlLock.gameObject;

        if (playerRoot == null)
        {
            PlayerDeathHandler deathHandler = FindFirstObjectByType<PlayerDeathHandler>(FindObjectsInactive.Include);

            if (deathHandler != null)
                playerRoot = deathHandler.gameObject;
        }

        if (checkpointManager == null)
            checkpointManager = CheckpointManager.Instance != null
                ? CheckpointManager.Instance
                : FindFirstObjectByType<CheckpointManager>(FindObjectsInactive.Include);
    }

    public void TogglePause()
    {
        if (_isPaused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        AutoWire();

        if (_isPaused || IsBlockingGameplayUiVisible())
            return;

        IsPaused = true;

        if (root != null)
            root.SetActive(true);
        else
            gameObject.SetActive(true);

        if (pauseOnOpen)
            PauseTimeScale();

        GameplayAudioSilencer.Mute(this);

        if (unlockCursorOnPause)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        if (playerControlLock != null)
            playerControlLock.SetLocked(true);

        _isPaused = true;
    }

    public void Resume()
    {
        if (!_isPaused)
            return;

        if (IsDeathUiVisible())
        {
            Debug.LogWarning("PauseMenuController ignored Resume because Death UI is visible.", this);
            return;
        }

        HideRootOnly();

        if (pauseOnOpen)
            RestoreTimeScale();

        GameplayAudioSilencer.Unmute(this);

        if (playerControlLock != null)
            playerControlLock.SetLocked(false);

        if (lockCursorOnResume)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        _isPaused = false;
        IsPaused = false;
    }

    public void ReturnToCheckpoint()
    {
        AutoWire();

        if (IsDeathUiVisible())
        {
            Debug.LogWarning("PauseMenuController ignored ReturnToCheckpoint because Death UI is visible.", this);
            return;
        }

        HideRootOnly();

        if (pauseOnOpen)
            RestoreTimeScale();

        GameplayAudioSilencer.Unmute(this);

        if (playerControlLock != null)
            playerControlLock.SetLocked(false);

        _isPaused = false;
        IsPaused = false;

        if (checkpointManager != null && playerRoot != null)
        {
            checkpointManager.RespawnPlayer(playerRoot);
        }
        else
        {
            Debug.LogWarning("ReturnToCheckpoint needs CheckpointManager and Player Root references. Assign them in PauseMenuController if auto-wire cannot find them.", this);
        }

        if (lockCursorOnResume)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void ExitToMenu()
    {
        GameplaySaveSystem.SaveCurrentProgress("exit to menu");

        if (pauseOnOpen)
            RestoreTimeScale();

        GameplayAudioSilencer.Unmute(this);

        if (playerControlLock != null)
            playerControlLock.SetLocked(false);

        _isPaused = false;
        IsPaused = false;

        if (string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            Debug.LogWarning("PauseMenuController cannot exit to menu because mainMenuSceneName is empty.", this);
            return;
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void PauseTimeScale()
    {
        _previousTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
        Time.timeScale = 0f;
    }

    private void RestoreTimeScale()
    {
        Time.timeScale = _previousTimeScale > 0f ? _previousTimeScale : 1f;
    }

    private void HandlePausePerformed(InputAction.CallbackContext context)
    {
        TogglePauseFromInput();
    }

    private void TogglePauseFromInput()
    {
        if (_lastPauseToggleFrame == Time.frameCount)
            return;

        _lastPauseToggleFrame = Time.frameCount;
        TogglePause();
    }

    private InputAction ResolvePauseAction()
    {
        if (pauseAction != null && pauseAction.action != null)
            return pauseAction.action;

        InputActionAsset projectActions = InputSystem.actions;

        if (projectActions == null)
            return null;

        InputActionMap actionMap = !string.IsNullOrWhiteSpace(fallbackPauseActionMapName)
            ? projectActions.FindActionMap(fallbackPauseActionMapName, false)
            : null;

        if (actionMap != null && !string.IsNullOrWhiteSpace(fallbackPauseActionName))
            return actionMap.FindAction(fallbackPauseActionName, false);

        return !string.IsNullOrWhiteSpace(fallbackPauseActionName)
            ? projectActions.FindAction(fallbackPauseActionName, false)
            : null;
    }

    private void SettingsNotImplemented()
    {
        Debug.Log("Settings not implemented yet.", this);
    }

    private void HideRootOnly()
    {
        if (root != null)
            root.SetActive(false);
        else
            gameObject.SetActive(false);
    }

    private bool IsBlockingGameplayUiVisible()
    {
        return IsDeathUiVisible() || IsStoryPopupVisible();
    }

    private static bool IsDeathUiVisible()
    {
        DeathUIController deathUI = FindFirstObjectByType<DeathUIController>(FindObjectsInactive.Include);
        return IsUiComponentVisible(deathUI);
    }

    private static bool IsStoryPopupVisible()
    {
        StoryItemPopupUI storyPopup = FindFirstObjectByType<StoryItemPopupUI>(FindObjectsInactive.Include);
        return IsUiComponentVisible(storyPopup);
    }

    private static bool IsUiComponentVisible(Component component)
    {
        if (component == null || !component.gameObject.activeInHierarchy)
            return false;

        CanvasGroup canvasGroup = component.GetComponent<CanvasGroup>();

        if (canvasGroup != null)
            return canvasGroup.alpha > 0.01f && canvasGroup.blocksRaycasts;

        return true;
    }

    private GameObject FindChildGameObject(string childName)
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (child != null && child.name == childName)
                return child.gameObject;
        }

        return null;
    }

    private T FindChildComponent<T>(string childName) where T : Component
    {
        GameObject childObject = FindChildGameObject(childName);
        return childObject != null ? childObject.GetComponent<T>() : null;
    }

    private static void AddListener(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null)
            return;

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    private static void RemoveListener(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null)
            button.onClick.RemoveListener(action);
    }
}
