using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
[DisallowMultipleComponent]
public class StoryPickupInteractable : MonoBehaviour, IEncounterResettable
{
    [Header("Pickup")]
    [SerializeField] private string pickupId = "StoryPickup_GromRadio";
    [SerializeField] private string displayName = "Radio Grom Division";
    [SerializeField] private string interactionPrompt = "E - Podnieś radio";
    [SerializeField] private bool destroyAfterPickup = true;
    [SerializeField] private GameObject visualRoot;
    [SerializeField] private UnityEvent onPickedUp;

    [Header("Player Filter")]
    [SerializeField] private bool requirePlayerTag = true;
    [SerializeField] private string playerTag = "Player";

    [Header("Optional Prompt UI")]
    [SerializeField] private GameObject promptRoot;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private InteractionPromptUI interactionPromptUI;
    [SerializeField] private GameplayHUDController hud;
    [SerializeField] private bool autoFindHud = true;
    [SerializeField] private string interactionKey = "E";
    [SerializeField] private string interactionText = "Podnieś radio";
    [SerializeField] private string pickupNotification = "RADIO GROM DIVISION ZABEZPIECZONE";

    [Header("Optional Story Popup")]
    [SerializeField] private bool showStoryPopupBeforeEvents = false;
    [SerializeField] private StoryPopupData storyPopupData;
    [SerializeField] private StoryItemPopupUI storyPopupUI;
    [SerializeField] private string popupTitle = "DOKUMENT GROM DIVISION";
    [SerializeField] private string popupSubtitle = "Raport terenowy // Sektor kościoła";
    [TextArea(4, 10)]
    [SerializeField] private string popupBody = "Zabezpieczono fragment raportu Grom Division.\n\nBlack Orchid przeniosła część materiału biologicznego do zabudowań obok kościoła.\nOddział zabezpieczający zgłaszał wzrost agresji zainfekowanych w rejonie cmentarza.\n\nOstatni wpis:\n«Nie otwierać domu bez wsparcia. Sygnał nad strefą wpływa na mutanty.»";
    [SerializeField] private string popupButtonText = "KONTYNUUJ";

    [Header("Death Reset")]
    [SerializeField] private EncounterResetService encounterResetService;
    [SerializeField] private bool registerWithEncounterResetService = true;
    [SerializeField] private bool resetOnEncounterReset = true;
    [SerializeField] private ArenaEncounterController linkedArena;
    [SerializeField] private string linkedArenaId = "Arena_DockStart";

    private bool _playerInRange;
    private bool _pickedUp;
    private bool _promptLogged;
    private Collider _triggerCollider;
    private Renderer[] _visualRenderers;

    public string PickupId => pickupId;
    public string DisplayName => displayName;
    public bool IsPickedUp => _pickedUp;

    private void Reset()
    {
        _triggerCollider = GetComponent<Collider>();
        _triggerCollider.isTrigger = true;

        if (visualRoot == null)
            visualRoot = gameObject;
    }

    private void Awake()
    {
        _triggerCollider = GetComponent<Collider>();

        if (_triggerCollider != null && !_triggerCollider.isTrigger)
            _triggerCollider.isTrigger = true;

        if (visualRoot == null)
            visualRoot = gameObject;

        CacheVisualRenderers();
        ResolveHUD();
        ResolveStoryPopup();
        ResolveEncounterResetService();
        ResolveLinkedArena();
        HidePrompt();
    }

    private void OnEnable()
    {
        ResolveEncounterResetService();

        if (registerWithEncounterResetService && encounterResetService != null)
            encounterResetService.RegisterEncounter(this);
    }

    private void OnDisable()
    {
        if (encounterResetService != null)
            encounterResetService.UnregisterEncounter(this);
    }

    private void Update()
    {
        if (!_playerInRange || _pickedUp)
            return;

        ShowPrompt();

        Keyboard keyboard = Keyboard.current;

        if (keyboard != null && keyboard.eKey.wasPressedThisFrame)
            PickUp();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_pickedUp || !IsPlayer(other))
            return;

        _playerInRange = true;
        ShowPrompt();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other))
            return;

        _playerInRange = false;
        HidePrompt();
    }

    [ContextMenu("Pick Up")]
    public void PickUp()
    {
        if (_pickedUp)
            return;

        _pickedUp = true;
        _playerInRange = false;
        HidePrompt();

        if (destroyAfterPickup || visualRoot != null)
            SetVisualVisible(false);

        if (_triggerCollider != null)
            _triggerCollider.enabled = false;

        if (showStoryPopupBeforeEvents)
        {
            ResolveStoryPopup();

            if (storyPopupData != null)
            {
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowStoryPopup(storyPopupData, CompletePickup);
                    return;
                }

                if (storyPopupUI != null)
                {
                    storyPopupUI.Show(storyPopupData, CompletePickup);
                    return;
                }
            }

            if (storyPopupUI != null)
            {
                storyPopupUI.Show(popupTitle, popupSubtitle, popupBody, popupButtonText, CompletePickup);
                return;
            }

            Debug.LogWarning($"[{name}] Story popup requested for pickup '{pickupId}', but StoryItemPopupUI was not found. Continuing pickup events.");
        }

        CompletePickup();
    }

    [ContextMenu("Debug Reset Pickup")]
    public void DebugResetPickup()
    {
        ForceResetPickup();
    }

    public void ResetEncounter()
    {
        if (!resetOnEncounterReset || !_pickedUp)
            return;

        ResolveLinkedArena();

        if (linkedArena != null && linkedArena.IsCompleted)
            return;

        ForceResetPickup();
        Debug.Log($"Story pickup reset after player death: {pickupId}");
    }

    private void CompletePickup()
    {
        onPickedUp?.Invoke();

        if (hud != null && !string.IsNullOrWhiteSpace(pickupNotification))
            hud.ShowNotification(pickupNotification, 2.5f);

        Debug.Log($"Story pickup collected: {pickupId} ({displayName})");
    }

    private void ForceResetPickup()
    {
        _pickedUp = false;
        _playerInRange = false;
        _promptLogged = false;

        if (storyPopupUI != null)
            storyPopupUI.Hide();

        SetVisualVisible(true);

        if (_triggerCollider != null)
            _triggerCollider.enabled = true;

        HidePrompt();
    }

    private bool IsPlayer(Collider other)
    {
        if (other == null)
            return false;

        if (!requirePlayerTag)
            return true;

        return other.CompareTag(playerTag);
    }

    private void ShowPrompt()
    {
        ResolveHUD();

        if (hud != null)
        {
            hud.ShowInteractionPrompt(interactionKey, interactionText);
            return;
        }

        if (interactionPromptUI != null)
        {
            interactionPromptUI.Show(interactionKey, interactionText);
            return;
        }

        if (promptText != null)
            promptText.text = interactionPrompt;

        if (promptRoot != null)
        {
            promptRoot.SetActive(true);
            return;
        }

        if (!_promptLogged)
        {
            Debug.Log($"Interaction prompt: {interactionPrompt}");
            _promptLogged = true;
        }
    }

    private void HidePrompt()
    {
        if (interactionPromptUI != null)
            interactionPromptUI.Hide();

        if (hud != null)
            hud.HideInteractionPrompt();

        if (promptRoot != null)
            promptRoot.SetActive(false);
    }

    private void ResolveHUD()
    {
        if (!autoFindHud)
            return;

        if (hud == null)
            hud = GameplayHUDController.Instance;

        if (hud == null)
            hud = FindHUDInScene();

        if (interactionPromptUI == null && hud != null)
            interactionPromptUI = hud.GetComponentInChildren<InteractionPromptUI>(true);

        if (interactionPromptUI == null && hud == null)
            interactionPromptUI = FindPromptInScene();
    }

    private void ResolveStoryPopup()
    {
        if (storyPopupUI == null)
            storyPopupUI = FindFirstObjectByType<StoryItemPopupUI>(FindObjectsInactive.Include);
    }

    private void ResolveEncounterResetService()
    {
        if (encounterResetService == null)
            encounterResetService = FindFirstObjectByType<EncounterResetService>();
    }

    private void ResolveLinkedArena()
    {
        if (linkedArena != null || string.IsNullOrWhiteSpace(linkedArenaId))
            return;

        ArenaEncounterController[] arenas = FindObjectsByType<ArenaEncounterController>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (ArenaEncounterController arena in arenas)
        {
            if (arena != null && arena.ArenaId == linkedArenaId)
            {
                linkedArena = arena;
                return;
            }
        }
    }

    private void CacheVisualRenderers()
    {
        Transform visualTransform = visualRoot != null ? visualRoot.transform : transform;
        _visualRenderers = visualTransform.GetComponentsInChildren<Renderer>(true);
    }

    private void SetVisualVisible(bool visible)
    {
        if (visualRoot != null && visualRoot != gameObject)
        {
            visualRoot.SetActive(visible);
            return;
        }

        if (_visualRenderers == null || _visualRenderers.Length == 0)
            CacheVisualRenderers();

        if (_visualRenderers == null)
            return;

        foreach (Renderer visualRenderer in _visualRenderers)
        {
            if (visualRenderer != null)
                visualRenderer.enabled = visible;
        }
    }

    private static GameplayHUDController FindHUDInScene()
    {
        GameplayHUDController[] controllers = FindObjectsByType<GameplayHUDController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        return controllers.Length > 0 ? controllers[0] : null;
    }

    private static InteractionPromptUI FindPromptInScene()
    {
        InteractionPromptUI[] prompts = FindObjectsByType<InteractionPromptUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        return prompts.Length > 0 ? prompts[0] : null;
    }
}
