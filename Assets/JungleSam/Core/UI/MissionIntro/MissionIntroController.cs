using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class MissionIntroController : MonoBehaviour
{
    [Header("Mission Intro")]
    [SerializeField] private GameObject root;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Button startMissionButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_Text startMissionButtonText;
    [SerializeField] private TMP_Text backButtonText;

    [Header("Scene")]
    [SerializeField] private string gameplaySceneName = "Scene_A";
    [SerializeField] private bool hideOnStart = true;

    [Header("Optional Menu References")]
    [SerializeField] private MainMenuController mainMenuController;
    [SerializeField] private GameObject mainMenuPanel;

    private void Reset()
    {
        AutoWire();
    }

    private void Awake()
    {
        AutoWire();
    }

    private void Start()
    {
        if (hideOnStart)
            Hide();
    }

    private void OnEnable()
    {
        AutoWire();
        AddListener(startMissionButton, StartMission);
        AddListener(backButton, BackToMainMenu);
    }

    private void OnDisable()
    {
        RemoveListener(startMissionButton, StartMission);
        RemoveListener(backButton, BackToMainMenu);
    }

    [ContextMenu("Auto Wire")]
    public void AutoWire()
    {
        root ??= FindChildGameObject("MissionIntroRoot");
        backgroundImage ??= FindChildComponent<Image>("MissionIntroBackground");
        startMissionButton ??= FindChildComponent<Button>("StartMissionButton");
        backButton ??= FindChildComponent<Button>("BackButton");
        startMissionButtonText ??= FindChildComponent<TMP_Text>("StartMissionButtonText");
        backButtonText ??= FindChildComponent<TMP_Text>("BackButtonText");

        if (mainMenuController == null)
            mainMenuController = FindFirstObjectByType<MainMenuController>(FindObjectsInactive.Include);

        if (mainMenuPanel == null && mainMenuController != null)
            mainMenuPanel = FindNamedObject("MainMenuPanel");
    }

    public void Show(string targetGameplaySceneName)
    {
        AutoWire();

        if (!string.IsNullOrWhiteSpace(targetGameplaySceneName))
            gameplaySceneName = targetGameplaySceneName.Trim();

        if (root != null)
            root.SetActive(true);
        else
            gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (root != null)
            root.SetActive(false);
        else
            gameObject.SetActive(false);
    }

    public void StartMission()
    {
        if (string.IsNullOrWhiteSpace(gameplaySceneName))
        {
            Debug.LogWarning("MissionIntroController cannot start mission because gameplaySceneName is empty.", this);
            return;
        }

        SceneManager.LoadScene(gameplaySceneName);
    }

    public void BackToMainMenu()
    {
        SaveLoadContext.Clear();
        Hide();

        if (mainMenuController != null)
        {
            mainMenuController.ShowMainMenu();
            return;
        }

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
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

    private static GameObject FindNamedObject(string objectName)
    {
        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();

        foreach (Transform transform in transforms)
        {
            if (transform != null && transform.name == objectName && transform.gameObject.scene.IsValid())
                return transform.gameObject;
        }

        return null;
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
