using UnityEngine;

[DisallowMultipleComponent]
public class ObjectiveOnStoryPickup : MonoBehaviour, IEncounterResettable
{
    [Header("Objective")]
    [SerializeField] private GameplayHUDController hud;
    [SerializeField] private string objectiveText = "Przetrwaj atak zainfekowanych";
    [SerializeField] private string secondaryObjectiveText = "Utrzymaj pozycję przy nabrzeżu";
    [SerializeField] private bool showNotification = true;
    [SerializeField] private string notificationText = "CEL ZAKTUALIZOWANY";
    [SerializeField] private bool updateOnlyOnce = true;

    [Header("Death Reset")]
    [SerializeField] private EncounterResetService encounterResetService;
    [SerializeField] private bool registerWithEncounterResetService = true;
    [SerializeField] private bool resetObjectiveOnEncounterReset = true;
    [SerializeField] private string resetObjectiveText = "Znajdź źródło sygnału";
    [SerializeField] private string resetSecondaryObjectiveText = "Przedostań się przez nabrzeże";
    [SerializeField] private ArenaEncounterController linkedArena;
    [SerializeField] private string linkedArenaId = "Arena_DockStart";

    private bool _updated;

    private void Awake()
    {
        ResolveEncounterResetService();
        ResolveLinkedArena();
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

    [ContextMenu("Update Objective")]
    public void UpdateObjective()
    {
        if (updateOnlyOnce && _updated)
            return;

        _updated = true;
        ResolveHUD();

        if (hud != null)
        {
            hud.SetObjective(objectiveText, secondaryObjectiveText);

            if (showNotification)
                hud.ShowNotification(notificationText);
        }

        Debug.Log($"CEL ZAKTUALIZOWANY: {objectiveText}");
    }

    public void ResetEncounter()
    {
        if (!resetObjectiveOnEncounterReset || !_updated)
            return;

        ResolveHUD();
        ResolveLinkedArena();

        if (linkedArena != null && linkedArena.IsCompleted)
            return;

        _updated = false;

        if (hud != null)
            hud.SetObjective(resetObjectiveText, resetSecondaryObjectiveText);

        Debug.Log($"CEL COFNIĘTY PO ŚMIERCI: {resetObjectiveText}");
    }

    private void ResolveHUD()
    {
        if (hud == null)
            hud = GameplayHUDController.Instance;

        if (hud == null)
        {
            GameplayHUDController[] controllers = FindObjectsByType<GameplayHUDController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            hud = controllers.Length > 0 ? controllers[0] : null;
        }
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
}
