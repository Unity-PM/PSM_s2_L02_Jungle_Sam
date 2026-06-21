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
    [SerializeField] private bool hideVisualAfterPickup = true;
    [SerializeField] private GameObject visualRoot;
    [SerializeField] private UnityEvent onPickedUp;

    [Header("Player")]
    [SerializeField] private bool requirePlayerTag = true;
    [SerializeField] private string playerTag = "Player";

    [Header("Interaction")]
    [SerializeField] private string interactionKey = "E";
    [SerializeField] private string interactionText = "Podnieś radio";
    [SerializeField] private string pickupNotification = "RADIO GROM DIVISION ZABEZPIECZONE";

    [Header("Story Popup")]
    [SerializeField] private bool showStoryPopupBeforeEvents = false;
    [SerializeField] private StoryItemPopupUI storyPopupUI;

    [Header("Death Reset")]
    [SerializeField] private bool registerWithEncounterResetService = true;
    [SerializeField] private bool resetOnEncounterReset = true;
    [SerializeField] private string linkedArenaId = "Arena_DockStart";

    private bool _playerInRange;
    private bool _pickedUp;
    private bool _promptLogged;
    private Collider _triggerCollider;
    private Renderer[] _visualRenderers;
    private GameplayHUDController _hud;
    private InteractionPromptUI _interactionPromptUI;
    private EncounterResetService _encounterResetService;
    private ArenaEncounterController _linkedArena;

    public string PickupId => pickupId;
    public string DisplayName => displayName;
    public bool IsPickedUp => _pickedUp;

    private void Reset()
    {
        _triggerCollider = GetComponent<Collider>();
        _triggerCollider.isTrigger = true;
        visualRoot ??= gameObject;
    }

    private void Awake()
    {
        _triggerCollider = GetComponent<Collider>();

        if (_triggerCollider != null && !_triggerCollider.isTrigger)
            _triggerCollider.isTrigger = true;

        visualRoot ??= gameObject;
        CacheVisualRenderers();
        ResolveReferences();
        HidePrompt();
    }

    private void OnEnable()
    {
        ResolveReferences();

        if (registerWithEncounterResetService && _encounterResetService != null)
            _encounterResetService.RegisterEncounter(this);
    }

    private void OnDisable()
    {
        if (_encounterResetService != null)
            _encounterResetService.UnregisterEncounter(this);
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

        if (_triggerCollider != null)
            _triggerCollider.enabled = false;

        if (showStoryPopupBeforeEvents)
        {
            ResolveStoryPopup();

            if (storyPopupUI != null)
            {
                storyPopupUI.Show(CompletePickup);
                return;
            }

            Debug.LogWarning($"[{name}] Story popup requested for pickup '{pickupId}', but StoryItemPopupUI was not found. Continuing pickup events.", this);
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

        if (_linkedArena != null && _linkedArena.IsCompleted)
            return;

        ForceResetPickup();
        Debug.Log($"Story pickup reset after player death: {pickupId}");
    }

    private void CompletePickup()
    {
        try
        {
            onPickedUp?.Invoke();
        }
        catch (MissingReferenceException exception)
        {
            Debug.LogError($"[{name}] Pickup '{pickupId}' has an On Picked Up listener pointing to a destroyed object. Reassign the event target in Inspector. {exception.Message}", this);
        }

        ResolveHUD();

        if (_hud != null && !string.IsNullOrWhiteSpace(pickupNotification))
            _hud.ShowNotification(pickupNotification, 2.5f);

        if (hideVisualAfterPickup)
            SetVisualVisible(false);

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

        return !requirePlayerTag || other.CompareTag(playerTag);
    }

    private void ShowPrompt()
    {
        ResolveHUD();

        if (_hud != null)
        {
            _hud.ShowInteractionPrompt(interactionKey, interactionText);
            return;
        }

        if (_interactionPromptUI != null)
        {
            _interactionPromptUI.Show(interactionKey, interactionText);
            return;
        }

        if (!_promptLogged)
        {
            Debug.Log($"Interaction prompt: [{interactionKey}] - {interactionText}");
            _promptLogged = true;
        }
    }

    private void HidePrompt()
    {
        if (_interactionPromptUI != null)
            _interactionPromptUI.Hide();

        if (_hud != null)
            _hud.HideInteractionPrompt();
    }

    private void ResolveReferences()
    {
        ResolveHUD();
        ResolveStoryPopup();

        if (_encounterResetService == null)
            _encounterResetService = FindFirstObjectByType<EncounterResetService>();

        ResolveLinkedArena();
    }

    private void ResolveHUD()
    {
        if (_hud == null)
            _hud = GameplayHUDController.Instance;

        if (_hud == null)
            _hud = FindHUDInScene();

        if (_interactionPromptUI == null && _hud != null)
            _interactionPromptUI = _hud.GetComponentInChildren<InteractionPromptUI>(true);

        if (_interactionPromptUI == null && _hud == null)
            _interactionPromptUI = FindPromptInScene();
    }

    private void ResolveStoryPopup()
    {
        if (storyPopupUI == null)
            storyPopupUI = FindFirstObjectByType<StoryItemPopupUI>(FindObjectsInactive.Include);
    }

    private void ResolveLinkedArena()
    {
        if (_linkedArena != null || string.IsNullOrWhiteSpace(linkedArenaId))
            return;

        ArenaEncounterController[] arenas = FindObjectsByType<ArenaEncounterController>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (ArenaEncounterController arena in arenas)
        {
            if (arena != null && arena.ArenaId == linkedArenaId)
            {
                _linkedArena = arena;
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
