using System.Reflection;
using UnityEngine;

[DisallowMultipleComponent]
public class ArenaStartOnStoryPickup : MonoBehaviour
{
    [Header("Arena")]
    [SerializeField] private ArenaEncounterController arenaEncounterController;
    [SerializeField] private WaveSpawner waveSpawner;
    [SerializeField] private MonoBehaviour arenaController;
    [SerializeField] private bool autoFindArenaById = true;
    [SerializeField] private string arenaId = "Arena_DockStart";
    [SerializeField] private string startMethodName = "StartArena";
    [SerializeField] private bool startOnlyOnce = true;

    private bool _started;

    private void Reset()
    {
        arenaEncounterController = FindFirstObjectByType<ArenaEncounterController>();
    }

    [ContextMenu("Start Arena From Story Pickup")]
    public void StartArenaFromStoryPickup()
    {
        if (startOnlyOnce && _started)
            return;

        if (TryStartTypedArena())
        {
            _started = true;
            return;
        }

        if (TryStartByReflection())
        {
            _started = true;
            return;
        }

        Debug.LogWarning($"[{name}] Story pickup could not start arena. Assign ArenaEncounterController, WaveSpawner, or a controller with method '{startMethodName}'.");
    }

    private bool TryStartTypedArena()
    {
        if (arenaEncounterController == null && autoFindArenaById)
            arenaEncounterController = FindArenaById(arenaId);

        if (arenaEncounterController != null)
        {
            arenaEncounterController.StartArena();
            Debug.Log($"Story pickup started arena: {arenaEncounterController.ArenaId}");
            return true;
        }

        if (waveSpawner != null)
        {
            waveSpawner.StartSpawner();
            Debug.Log($"Story pickup started WaveSpawner: {waveSpawner.name}");
            return true;
        }

        if (arenaController is ArenaEncounterController typedArena)
        {
            typedArena.StartArena();
            Debug.Log($"Story pickup started arena: {typedArena.ArenaId}");
            return true;
        }

        if (arenaController is WaveSpawner typedSpawner)
        {
            typedSpawner.StartSpawner();
            Debug.Log($"Story pickup started WaveSpawner: {typedSpawner.name}");
            return true;
        }

        return false;
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

    private bool TryStartByReflection()
    {
        if (arenaController == null || string.IsNullOrWhiteSpace(startMethodName))
            return false;

        MethodInfo startMethod = arenaController.GetType().GetMethod(
            startMethodName,
            BindingFlags.Instance | BindingFlags.Public
        );

        if (startMethod == null || startMethod.GetParameters().Length > 0)
            return false;

        startMethod.Invoke(arenaController, null);
        Debug.Log($"Story pickup invoked {arenaController.GetType().Name}.{startMethodName}().");
        return true;
    }
}
