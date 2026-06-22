using UnityEngine;

[DisallowMultipleComponent]
public class ArenaStartOnStoryPickup : MonoBehaviour
{
    [Header("Arena")]
    [SerializeField] private ArenaEncounterController arenaController;
    [SerializeField] private string arenaId = "Arena_DockStart";
    [SerializeField] private bool startOnlyOnce = true;

    private bool _started;

    [ContextMenu("Start Arena From Story Pickup")]
    public void StartArenaFromStoryPickup()
    {
        if (startOnlyOnce && _started)
            return;

        ArenaEncounterController arena = arenaController != null ? arenaController : FindArenaById(arenaId);

        if (arena == null)
        {
            Debug.LogWarning($"[{name}] Story pickup could not find arena with id '{arenaId}'. Available arena ids: {GetAvailableArenaIds()}", this);
            return;
        }

        arena.StartArena();
        _started = true;
        Debug.Log($"Story pickup started arena: {arena.ArenaId}");
    }

    private static ArenaEncounterController FindArenaById(string targetArenaId)
    {
        ArenaEncounterController[] arenas = FindObjectsByType<ArenaEncounterController>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (ArenaEncounterController arena in arenas)
        {
            if (arena != null && arena.ArenaId == targetArenaId)
                return arena;
        }

        return null;
    }

    private static string GetAvailableArenaIds()
    {
        ArenaEncounterController[] arenas = FindObjectsByType<ArenaEncounterController>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        if (arenas == null || arenas.Length == 0)
            return "none";

        System.Text.StringBuilder builder = new System.Text.StringBuilder();

        foreach (ArenaEncounterController arena in arenas)
        {
            if (arena == null)
                continue;

            if (builder.Length > 0)
                builder.Append(", ");

            builder.Append(arena.ArenaId);
        }

        return builder.Length > 0 ? builder.ToString() : "none";
    }
}
