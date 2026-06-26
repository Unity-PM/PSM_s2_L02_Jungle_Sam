using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class GameplaySaveLoader : MonoBehaviour
{
    [SerializeField] private bool enableDebugLogs = true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSceneHook()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void RegisterSceneHook()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        TryCreateLoaderForScene(SceneManager.GetActiveScene());
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryCreateLoaderForScene(scene);
    }

    private static void TryCreateLoaderForScene(Scene scene)
    {
        if (!SaveLoadContext.HasPendingSave || !scene.IsValid())
            return;

        SaveGameData pendingSave = SaveLoadContext.PendingSave;

        if (pendingSave == null)
            return;

        if (!string.IsNullOrWhiteSpace(pendingSave.sceneName) && pendingSave.sceneName != scene.name)
            return;

        GameObject loaderObject = new GameObject("GameplaySaveLoader");
        SceneManager.MoveGameObjectToScene(loaderObject, scene);
        loaderObject.AddComponent<GameplaySaveLoader>();
    }

    private void Start()
    {
        StartCoroutine(ApplyPendingSaveAfterSceneStart());
    }

    private IEnumerator ApplyPendingSaveAfterSceneStart()
    {
        yield return null;

        SaveGameData save = SaveLoadContext.ConsumePendingSave();

        if (save == null)
        {
            Destroy(gameObject);
            yield break;
        }

        ApplySave(save);
        Destroy(gameObject);
    }

    private void ApplySave(SaveGameData save)
    {
        GameplaySaveSystem.BeginRestore();

        try
        {
            GameObject playerRoot = ResolvePlayerRoot();

            RestoreWorldStateInternal(save);
            ApplySpawnPosition(save, playerRoot);
            ApplyPlayerStats(save, playerRoot);

            Log($"Applied save for user '{save.userId}' in scene '{SceneManager.GetActiveScene().name}'. Checkpoint: {save.checkpointId}");
        }
        finally
        {
            GameplaySaveSystem.EndRestore();
        }
    }

    public static void RestoreWorldState(SaveGameData save)
    {
        GameplaySaveSystem.BeginRestore();

        try
        {
            RestoreWorldStateInternal(save);
        }
        finally
        {
            GameplaySaveSystem.EndRestore();
        }
    }

    private static void RestoreWorldStateInternal(SaveGameData save)
    {
        if (save == null)
            return;

        ApplyCompletedEncounters(save);
        ApplyCollectedStoryPickups(save);
        ApplyStartedEncounters(save);
        ApplyObjective(save);
    }

    private static void ApplyCompletedEncounters(SaveGameData save)
    {
        if (save.completedEncounters == null || save.completedEncounters.Count == 0)
            return;

        ArenaEncounterController[] arenas = FindObjectsByType<ArenaEncounterController>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (ArenaEncounterController arena in arenas)
        {
            if (arena == null || string.IsNullOrWhiteSpace(arena.ArenaId))
                continue;

            if (save.completedEncounters.Contains(arena.ArenaId))
                arena.RestoreCompletedFromSave(false);
        }
    }

    private static void ApplyStartedEncounters(SaveGameData save)
    {
        if (save.startedEncounters == null || save.startedEncounters.Count == 0)
            return;

        ArenaEncounterController[] arenas = FindObjectsByType<ArenaEncounterController>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (ArenaEncounterController arena in arenas)
        {
            if (arena == null || string.IsNullOrWhiteSpace(arena.ArenaId))
                continue;

            if (save.completedEncounters != null && save.completedEncounters.Contains(arena.ArenaId))
                continue;

            if (save.startedEncounters.Contains(arena.ArenaId))
                arena.RestoreStartedFromSave();
        }
    }

    private static void ApplyCollectedStoryPickups(SaveGameData save)
    {
        if (save.collectedStoryPickups == null || save.collectedStoryPickups.Count == 0)
            return;

        StoryPickupInteractable[] pickups = FindObjectsByType<StoryPickupInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (StoryPickupInteractable pickup in pickups)
        {
            if (pickup == null || string.IsNullOrWhiteSpace(pickup.PickupId))
                continue;

            if (save.collectedStoryPickups.Contains(pickup.PickupId))
                pickup.RestoreCollectedFromSave();
        }
    }

    private void ApplySpawnPosition(SaveGameData save, GameObject playerRoot)
    {
        if (playerRoot == null)
        {
            Debug.LogWarning("GameplaySaveLoader could not find player root. Saved position was not applied.", this);
            return;
        }

        CheckpointManager checkpointManager = CheckpointManager.Instance != null
            ? CheckpointManager.Instance
            : FindFirstObjectByType<CheckpointManager>(FindObjectsInactive.Include);

        if (checkpointManager == null)
        {
            if (save.hasPlayerTransform)
            {
                TeleportPlayerToSavedTransform(save, playerRoot);
                return;
            }

            Debug.LogWarning("GameplaySaveLoader could not find CheckpointManager. Saved position was not applied.", this);
            return;
        }

        CheckpointVolume checkpoint = FindCheckpoint(save.checkpointId);

        if (checkpoint != null)
            checkpointManager.RegisterCheckpoint(checkpoint);
        else if (!string.IsNullOrWhiteSpace(save.checkpointId))
            Debug.LogWarning($"GameplaySaveLoader could not find checkpoint '{save.checkpointId}'. Using current/fallback respawn point.", this);

        if (save.hasPlayerTransform)
        {
            TeleportPlayerToSavedTransform(save, playerRoot);
            return;
        }

        checkpointManager.RespawnPlayer(playerRoot);
    }

    private void ApplyPlayerStats(SaveGameData save, GameObject playerRoot)
    {
        if (playerRoot == null)
            return;

        PlayerHealth playerHealth = playerRoot.GetComponentInChildren<PlayerHealth>(true);

        if (playerHealth != null)
            ApplyHealth(playerHealth, save.health);

        PlayerStats playerStats = playerRoot.GetComponentInChildren<PlayerStats>(true);

        if (playerStats != null)
            playerStats.SetArmor(save.armor);

        GameplayHUDController hud = GameplayHUDController.Instance != null
            ? GameplayHUDController.Instance
            : FindFirstObjectByType<GameplayHUDController>(FindObjectsInactive.Include);

        if (hud != null)
        {
            float maxHealth = playerHealth != null ? playerHealth.MaxHealth : 100f;
            float maxArmor = playerStats != null ? playerStats.MaxArmor : 100f;
            hud.SetHealth(save.health, maxHealth);
            hud.SetArmor(save.armor, maxArmor);
        }
    }

    private static void ApplyHealth(PlayerHealth playerHealth, int savedHealth)
    {
        if (playerHealth == null)
            return;

        playerHealth.RestoreFullHealth();

        float targetHealth = Mathf.Clamp(savedHealth, 1f, playerHealth.MaxHealth);
        float damageToApply = playerHealth.MaxHealth - targetHealth;

        if (damageToApply > 0f)
            playerHealth.TakeDamage(damageToApply);
    }

    private static void ApplyObjective(SaveGameData save)
    {
        if (string.IsNullOrWhiteSpace(save.currentObjective))
            return;

        string secondary = !string.IsNullOrWhiteSpace(save.secondaryObjective)
            ? save.secondaryObjective
            : save.missionStage;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetObjective(save.currentObjective, secondary);
            return;
        }

        GameplayHUDController hud = GameplayHUDController.Instance != null
            ? GameplayHUDController.Instance
            : FindFirstObjectByType<GameplayHUDController>(FindObjectsInactive.Include);

        if (hud != null)
            hud.SetObjective(save.currentObjective, secondary);
    }

    private static GameObject ResolvePlayerRoot()
    {
        PlayerControlLock playerControlLock = FindFirstObjectByType<PlayerControlLock>(FindObjectsInactive.Include);

        if (playerControlLock != null)
            return playerControlLock.gameObject;

        PlayerDeathHandler deathHandler = FindFirstObjectByType<PlayerDeathHandler>(FindObjectsInactive.Include);

        if (deathHandler != null)
            return deathHandler.gameObject;

        GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");

        if (taggedPlayer != null)
            return taggedPlayer;

        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>(FindObjectsInactive.Include);
        return playerHealth != null ? playerHealth.gameObject : null;
    }

    private static CheckpointVolume FindCheckpoint(string checkpointId)
    {
        if (string.IsNullOrWhiteSpace(checkpointId))
            return null;

        CheckpointVolume[] checkpoints = FindObjectsByType<CheckpointVolume>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (CheckpointVolume checkpoint in checkpoints)
        {
            if (checkpoint != null && checkpoint.CheckpointId == checkpointId)
                return checkpoint;
        }

        return null;
    }

    private static void TeleportPlayerToSavedTransform(SaveGameData save, GameObject playerRoot)
    {
        Vector3 savedPosition = new Vector3(save.playerPositionX, save.playerPositionY, save.playerPositionZ);
        Quaternion savedRotation = Quaternion.Euler(0f, save.playerRotationY, 0f);
        TeleportPlayer(playerRoot, savedPosition, savedRotation);
    }

    private static void TeleportPlayer(GameObject player, Vector3 position, Quaternion rotation)
    {
        CharacterController characterController = player.GetComponent<CharacterController>();
        Rigidbody rigidbody = player.GetComponent<Rigidbody>();

        bool characterControllerWasEnabled = characterController != null && characterController.enabled;

        if (characterController != null)
            characterController.enabled = false;

        if (rigidbody != null)
        {
            rigidbody.linearVelocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }

        player.transform.SetPositionAndRotation(position, rotation);

        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
            playerController.ResetLook(rotation);

        if (characterController != null)
            characterController.enabled = characterControllerWasEnabled;
    }

    private void Log(string message)
    {
        if (enableDebugLogs)
            Debug.Log($"[GameplaySaveLoader] {message}", this);
    }
}
