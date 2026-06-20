using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
[DisallowMultipleComponent]
public class ArenaEncounterController : MonoBehaviour, IEncounterResettable
{
    [Header("Arena")]
    [SerializeField] private string arenaId = "Arena_DockStart";
    [SerializeField] private WaveSpawner waveSpawner;
    [SerializeField] private bool startOnPlayerEnter = true;
    [SerializeField] private bool completeOnce = true;

    [Header("Player Filter")]
    [SerializeField] private bool requirePlayerTag = true;
    [SerializeField] private string playerTag = "Player";

    [Header("Checkpoint")]
    [SerializeField] private bool activateCheckpointOnComplete = true;
    [SerializeField] private CheckpointVolume checkpointOnComplete;

    [Header("Events")]
    [SerializeField] private UnityEvent onArenaStarted;
    [SerializeField] private UnityEvent onArenaCompleted;

    [Header("Arena Gates")]
    [SerializeField] private ArenaGateController[] arenaGates;
    [SerializeField] private bool closeGatesOnStart = true;
    [SerializeField] private bool openGatesOnComplete = true;
    [SerializeField] private bool openGatesOnReset = true;
    [SerializeField] private bool closeGatesOnDeathReset = true;

    [Header("Scene Hooks")]
    [SerializeField] private GameObject[] enableOnStart;
    [SerializeField] private GameObject[] disableOnStart;
    [SerializeField] private GameObject[] enableOnComplete;
    [SerializeField] private GameObject[] disableOnComplete;

    [Header("Death Reset")]
    [SerializeField] private EncounterResetService encounterResetService;
    [SerializeField] private bool registerWithEncounterResetService = true;

    private bool _started;
    private bool _completed;

    public string ArenaId => arenaId;
    public bool IsStarted => _started;
    public bool IsCompleted => _completed;
    public event System.Action<ArenaEncounterController> ArenaStarted;
    public event System.Action<ArenaEncounterController> ArenaCompleted;

    private void Reset()
    {
        Collider triggerCollider = GetComponent<Collider>();
        triggerCollider.isTrigger = true;

        if (waveSpawner == null)
            waveSpawner = GetComponentInChildren<WaveSpawner>();

        if (checkpointOnComplete == null)
            checkpointOnComplete = GetComponentInChildren<CheckpointVolume>();

        if (arenaGates == null || arenaGates.Length == 0)
            arenaGates = GetComponentsInChildren<ArenaGateController>();
    }

    private void Awake()
    {
        Collider triggerCollider = GetComponent<Collider>();

        if (triggerCollider != null && !triggerCollider.isTrigger)
            triggerCollider.isTrigger = true;

        if (waveSpawner == null)
            waveSpawner = GetComponentInChildren<WaveSpawner>();

        if (encounterResetService == null)
            encounterResetService = FindFirstObjectByType<EncounterResetService>();
    }

    private void OnEnable()
    {
        if (waveSpawner != null)
            waveSpawner.SpawnerFinished += OnSpawnerFinished;

        if (registerWithEncounterResetService && encounterResetService != null)
            encounterResetService.RegisterEncounter(this);
    }

    private void OnDisable()
    {
        if (waveSpawner != null)
            waveSpawner.SpawnerFinished -= OnSpawnerFinished;

        if (encounterResetService != null)
            encounterResetService.UnregisterEncounter(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!startOnPlayerEnter)
            return;

        if (completeOnce && _completed)
            return;

        if (requirePlayerTag && !other.CompareTag(playerTag))
            return;

        StartArena();
    }

    [ContextMenu("Start Arena")]
    public void StartArena()
    {
        if (_started && waveSpawner != null && waveSpawner.IsRunning)
            return;

        if (completeOnce && _completed)
            return;

        _started = true;
        SetGatesClosed(closeGatesOnStart);
        SetObjectsActive(enableOnStart, true);
        SetObjectsActive(disableOnStart, false);
        ArenaStarted?.Invoke(this);
        onArenaStarted?.Invoke();

        if (waveSpawner != null)
        {
            waveSpawner.StartSpawner();
            Debug.Log($"Arena started: {arenaId}");
        }
        else
        {
            Debug.LogWarning($"[{name}] Arena '{arenaId}' has no WaveSpawner assigned.");
        }
    }

    [ContextMenu("Complete Arena")]
    public void CompleteArena()
    {
        if (completeOnce && _completed)
            return;

        _completed = true;
        _started = false;

        if (openGatesOnComplete)
            SetGatesClosed(false);

        SetObjectsActive(enableOnComplete, true);
        SetObjectsActive(disableOnComplete, false);

        if (activateCheckpointOnComplete && checkpointOnComplete != null)
        {
            CheckpointManager manager = CheckpointManager.Instance != null
                ? CheckpointManager.Instance
                : FindFirstObjectByType<CheckpointManager>();

            if (manager != null)
                manager.RegisterCheckpoint(checkpointOnComplete);
            else
                Debug.LogWarning($"[{name}] Arena '{arenaId}' completed but CheckpointManager was not found.");
        }

        Debug.Log($"Arena completed: {arenaId}");
        ArenaCompleted?.Invoke(this);
        onArenaCompleted?.Invoke();
    }

    public void ResetEncounter()
    {
        if (_completed)
            return;

        _started = false;

        if (closeGatesOnDeathReset)
            SetGatesClosed(true);
        else if (openGatesOnReset)
            SetGatesClosed(false);

        SetObjectsActive(enableOnStart, false);
        SetObjectsActive(disableOnStart, true);

        Debug.Log($"Arena reset: {arenaId}");
    }

    private void OnSpawnerFinished(WaveSpawner finishedSpawner)
    {
        if (finishedSpawner != waveSpawner)
            return;

        CompleteArena();
    }

    private static void SetObjectsActive(GameObject[] objects, bool active)
    {
        if (objects == null)
            return;

        foreach (GameObject obj in objects)
        {
            if (obj != null)
                obj.SetActive(active);
        }
    }

    private void SetGatesClosed(bool closed)
    {
        if (arenaGates == null)
            return;

        foreach (ArenaGateController gate in arenaGates)
        {
            if (gate != null)
                gate.SetClosed(closed);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = _completed ? Color.green : Color.cyan;

        Collider arenaCollider = GetComponent<Collider>();

        if (arenaCollider is BoxCollider boxCollider)
        {
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
            Gizmos.matrix = oldMatrix;
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, 2f);
        }
    }
}
