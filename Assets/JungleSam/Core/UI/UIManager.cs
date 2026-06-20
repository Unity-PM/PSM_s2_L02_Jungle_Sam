using System;
using UnityEngine;

[DisallowMultipleComponent]
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Scene UI")]
    [SerializeField] private GameplayHUDController gameplayHUD;
    [SerializeField] private StoryItemPopupUI storyPopupUI;
    [SerializeField] private DeathUIController deathUI;

    public GameplayHUDController GameplayHUD => gameplayHUD;
    public StoryItemPopupUI StoryPopupUI => storyPopupUI;
    public DeathUIController DeathUI => deathUI;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"Duplicate UIManager found on '{name}'. Keeping existing scene instance.");
            return;
        }

        Instance = this;
        ResolveReferences();
    }

    private void OnEnable()
    {
        if (Instance == null)
            Instance = this;

        ResolveReferences();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    [ContextMenu("Resolve References")]
    public void ResolveReferences()
    {
        if (gameplayHUD == null)
            gameplayHUD = FindFirstObjectByType<GameplayHUDController>(FindObjectsInactive.Include);

        if (storyPopupUI == null)
            storyPopupUI = FindFirstObjectByType<StoryItemPopupUI>(FindObjectsInactive.Include);

        if (deathUI == null)
            deathUI = FindFirstObjectByType<DeathUIController>(FindObjectsInactive.Include);
    }

    public void ShowStoryPopup(StoryPopupData data, Action onClosed)
    {
        ResolveReferences();

        if (storyPopupUI != null)
        {
            storyPopupUI.Show(data, onClosed);
            return;
        }

        Debug.LogWarning("UIManager could not find StoryItemPopupUI. Continuing story popup callback without UI.");
        onClosed?.Invoke();
    }

    public void ShowDeathPanel(Action onRespawnClicked = null)
    {
        ResolveReferences();

        if (deathUI == null)
            return;

        deathUI.SetRespawnCallback(onRespawnClicked);
        deathUI.Show();
    }

    public void HideDeathPanel()
    {
        ResolveReferences();

        if (deathUI != null)
            deathUI.Hide();
    }

    public void ShowNotification(string message)
    {
        ResolveReferences();

        if (gameplayHUD != null)
            gameplayHUD.ShowNotification(message);
        else if (!string.IsNullOrWhiteSpace(message))
            Debug.Log($"HUD notification: {message}");
    }

    public void SetObjective(string main, string secondary)
    {
        ResolveReferences();

        if (gameplayHUD != null)
            gameplayHUD.SetObjective(main, secondary);
    }
}
