using UnityEngine;

[DisallowMultipleComponent]
public class ObjectiveOnStoryPickup : MonoBehaviour, IEncounterResettable
{
    [Header("Objective")]
    [SerializeField] private string objectiveText = "Przetrwaj atak zainfekowanych";
    [SerializeField] private string secondaryObjectiveText = "Utrzymaj pozycję przy nabrzeżu";
    [SerializeField] private bool showNotification = true;
    [SerializeField] private string notificationText = "CEL ZAKTUALIZOWANY";
    [SerializeField] private bool updateOnlyOnce = true;

    [Header("Death Reset")]
    [SerializeField] private bool registerWithEncounterResetService = true;
    [SerializeField] private bool resetObjectiveOnEncounterReset = true;
    [SerializeField] private string resetObjectiveText = "Znajdź źródło sygnału";
    [SerializeField] private string resetSecondaryObjectiveText = "Przedostań się przez nabrzeże";
    [SerializeField] private string linkedArenaId = "Arena_DockStart";

    private bool _updated;
    private GameplayHUDController _hud;
    private EncounterResetService _encounterResetService;
    private ArenaEncounterController _linkedArena;

    private void Awake()
    {
        ResolveEncounterResetService();
        ResolveLinkedArena();
    }

    private void OnEnable()
    {
        ResolveEncounterResetService();

        if (registerWithEncounterResetService && _encounterResetService != null)
            _encounterResetService.RegisterEncounter(this);
    }

    private void OnDisable()
    {
        if (_encounterResetService != null)
            _encounterResetService.UnregisterEncounter(this);
    }

    [ContextMenu("Update Objective")]
    public void UpdateObjective()
    {
        if (updateOnlyOnce && _updated)
            return;

        _updated = true;
        ResolveHUD();

        if (_hud != null)
        {
            _hud.SetObjective(objectiveText, secondaryObjectiveText);
            GameplaySaveSystem.SaveObjective(objectiveText, secondaryObjectiveText);

            if (showNotification)
                _hud.ShowNotification(notificationText);
        }

        Debug.Log($"CEL ZAKTUALIZOWANY: {objectiveText}");
    }

    public void ResetEncounter()
    {
        if (!resetObjectiveOnEncounterReset || !_updated)
            return;

        ResolveHUD();
        ResolveLinkedArena();

        if (_linkedArena != null && _linkedArena.IsCompleted)
            return;

        _updated = false;

        if (_hud != null)
            _hud.SetObjective(resetObjectiveText, resetSecondaryObjectiveText);

        Debug.Log($"CEL COFNIĘTY PO ŚMIERCI: {resetObjectiveText}");
    }

    private void ResolveHUD()
    {
        if (_hud == null)
            _hud = GameplayHUDController.Instance;

        if (_hud == null)
        {
            GameplayHUDController[] controllers = FindObjectsByType<GameplayHUDController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            _hud = controllers.Length > 0 ? controllers[0] : null;
        }
    }

    private void ResolveEncounterResetService()
    {
        if (_encounterResetService == null)
            _encounterResetService = FindFirstObjectByType<EncounterResetService>();
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
}
