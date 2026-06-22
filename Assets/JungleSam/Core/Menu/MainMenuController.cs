using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
public class MainMenuController : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string gameplaySceneName = "Scene_A";

    [Header("Panels")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private MissionIntroController missionIntroController;
    [SerializeField] private bool showMissionIntroOnNewGame = true;

    [Header("Login UI")]
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button registerButton;
    [SerializeField] private TextMeshProUGUI loginErrorText;

    [Header("Menu UI")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button logoutButton;
    [SerializeField] private TextMeshProUGUI playerNameText;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs;

    private IAuthService _authService;
    private ISaveGameService _saveService;

    private void Reset()
    {
        AutoWire();
    }

    private void Awake()
    {
        AutoWire();
        _authService = new LocalJsonAuthService(enableDebugLogs);
        _saveService = new LocalJsonSaveGameService(enableDebugLogs);
    }

    private void Start()
    {
        if (AuthSession.IsLoggedIn)
            ShowMainMenu();
        else
            ShowLogin();
    }

    private void OnEnable()
    {
        AutoWire();

        AddListener(loginButton, Login);
        AddListener(registerButton, Register);
        AddListener(newGameButton, NewGame);
        AddListener(continueButton, ContinueGame);
        AddListener(exitButton, ExitGame);
        AddListener(logoutButton, Logout);
    }

    private void OnDisable()
    {
        RemoveListener(loginButton, Login);
        RemoveListener(registerButton, Register);
        RemoveListener(newGameButton, NewGame);
        RemoveListener(continueButton, ContinueGame);
        RemoveListener(exitButton, ExitGame);
        RemoveListener(logoutButton, Logout);
    }

    [ContextMenu("Auto Wire")]
    public void AutoWire()
    {
        loginPanel ??= FindChildGameObject("LoginPanel");
        mainMenuPanel ??= FindChildGameObject("MainMenuPanel");
        usernameInput ??= FindChildComponent<TMP_InputField>("UsernameInput");
        passwordInput ??= FindChildComponent<TMP_InputField>("PasswordInput");
        loginButton ??= FindChildComponent<Button>("LoginButton");
        registerButton ??= FindChildComponent<Button>("RegisterButton");
        newGameButton ??= FindChildComponent<Button>("NewGameButton");
        continueButton ??= FindChildComponent<Button>("ContinueButton");
        exitButton ??= FindChildComponent<Button>("ExitButton");
        logoutButton ??= FindChildComponent<Button>("LogoutButton");
        loginErrorText ??= FindChildComponent<TextMeshProUGUI>("LoginErrorText");
        playerNameText ??= FindChildComponent<TextMeshProUGUI>("PlayerNameText");

        if (missionIntroController == null)
            missionIntroController = FindFirstObjectByType<MissionIntroController>(FindObjectsInactive.Include);
    }

    public void Login()
    {
        AuthResult result = _authService.Login(GetUsername(), GetPassword());
        HandleAuthResult(result);
    }

    public void Register()
    {
        AuthResult result = _authService.Register(GetUsername(), GetPassword());
        HandleAuthResult(result);
    }

    public void NewGame()
    {
        if (!RequireLoggedIn())
            return;

        AuthUserData user = AuthSession.CurrentUser;
        _saveService.DeleteSave(user.userId);

        SaveGameData newSave = _saveService.CreateNewSave(user.userId, gameplaySceneName);
        _saveService.SaveGame(newSave);

        if (showMissionIntroOnNewGame && missionIntroController != null)
        {
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(false);

            missionIntroController.Show(gameplaySceneName);
            return;
        }

        LoadGameplayScene(newSave.sceneName);
    }

    public void ContinueGame()
    {
        if (!RequireLoggedIn())
            return;

        AuthUserData user = AuthSession.CurrentUser;

        if (!_saveService.HasSave(user.userId))
        {
            ShowError("Brak zapisu gry");
            RefreshMenuState();
            return;
        }

        SaveGameData save = _saveService.LoadSave(user.userId);

        if (save == null)
        {
            ShowError("Nie udało się odczytać zapisu gry.");
            RefreshMenuState();
            return;
        }

        LoadGameplayScene(string.IsNullOrWhiteSpace(save.sceneName) ? gameplaySceneName : save.sceneName);
    }

    public void ExitGame()
    {
        Log("Exit game requested.");

#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void Logout()
    {
        AuthSession.Clear();
        ShowLogin();
    }

    private void HandleAuthResult(AuthResult result)
    {
        if (result != null && result.success)
        {
            ShowMainMenu();
            return;
        }

        ShowError(result != null ? result.message : "Operacja logowania nie powiodła się.");
    }

    private void ShowLogin()
    {
        if (loginPanel != null)
            loginPanel.SetActive(true);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        SetErrorText(string.Empty);
    }

    public void ShowMainMenu()
    {
        if (loginPanel != null)
            loginPanel.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        SetErrorText(string.Empty);
        RefreshMenuState();
    }

    private void RefreshMenuState()
    {
        AuthUserData user = AuthSession.CurrentUser;

        if (playerNameText != null)
            playerNameText.text = user != null ? $"OPERATOR: {user.username}" : "OPERATOR: -";

        if (continueButton != null)
            continueButton.interactable = user != null && _saveService.HasSave(user.userId);
    }

    private bool RequireLoggedIn()
    {
        if (AuthSession.IsLoggedIn)
            return true;

        ShowError("Musisz się zalogować.");
        ShowLogin();
        return false;
    }

    private void LoadGameplayScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            ShowError("Nie ustawiono sceny gameplay.");
            return;
        }

        Log($"Loading scene '{sceneName}'.");
        SceneManager.LoadScene(sceneName);
    }

    private string GetUsername()
    {
        return usernameInput != null ? usernameInput.text : string.Empty;
    }

    private string GetPassword()
    {
        return passwordInput != null ? passwordInput.text : string.Empty;
    }

    private void ShowError(string message)
    {
        SetErrorText(message);
        Log(message);
    }

    private void SetErrorText(string message)
    {
        if (loginErrorText != null)
            loginErrorText.text = message ?? string.Empty;
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

    private void Log(string message)
    {
        if (enableDebugLogs)
            Debug.Log($"[MainMenuController] {message}", this);
    }
}
