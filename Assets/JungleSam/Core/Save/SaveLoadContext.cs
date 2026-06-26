using UnityEngine;
using UnityEngine.SceneManagement;

public static class SaveLoadContext
{
    public static SaveGameData PendingSave { get; private set; }
    public static bool HasPendingSave => PendingSave != null;

    public static void SetPendingSave(SaveGameData save)
    {
        PendingSave = save;
    }

    public static SaveGameData ConsumePendingSave()
    {
        SaveGameData save = PendingSave;
        PendingSave = null;
        return save;
    }

    public static void Clear()
    {
        PendingSave = null;
    }
}

public static class GameplaySaveSystem
{
    public static bool IsRestoringSave { get; private set; }

    public static void BeginRestore()
    {
        IsRestoringSave = true;
    }

    public static void EndRestore()
    {
        IsRestoringSave = false;
    }

    public static void SaveCurrentProgress(string reason = "")
    {
        SaveGameData data = CreateSnapshot();

        if (data == null)
            return;

        Save(data, reason);
    }

    public static void SaveCheckpoint(CheckpointVolume checkpoint)
    {
        if (checkpoint == null)
            return;

        SaveGameData data = CreateSnapshot();

        if (data == null)
            return;

        data.checkpointId = checkpoint.CheckpointId;
        Save(data, $"checkpoint {checkpoint.CheckpointId}");
    }

    public static void SaveArenaCompleted(ArenaEncounterController arena, string objectiveText = null, string secondaryObjectiveText = null)
    {
        if (arena == null)
            return;

        SaveGameData data = CreateSnapshot();

        if (data == null)
            return;

        RemoveValue(data.startedEncounters, arena.ArenaId);
        AddUnique(data.completedEncounters, arena.ArenaId);

        if (!string.IsNullOrWhiteSpace(objectiveText))
            data.currentObjective = objectiveText;

        if (!string.IsNullOrWhiteSpace(secondaryObjectiveText))
            data.secondaryObjective = secondaryObjectiveText;

        Save(data, $"arena {arena.ArenaId}");
    }

    public static void SaveArenaStarted(ArenaEncounterController arena)
    {
        if (arena == null)
            return;

        SaveGameData data = CreateSnapshot();

        if (data == null || data.completedEncounters.Contains(arena.ArenaId))
            return;

        AddUnique(data.startedEncounters, arena.ArenaId);
        Save(data, $"arena started {arena.ArenaId}");
    }

    public static void SaveStoryPickupCollected(StoryPickupInteractable pickup)
    {
        if (pickup == null)
            return;

        SaveGameData data = CreateSnapshot();

        if (data == null)
            return;

        AddUnique(data.collectedStoryPickups, pickup.PickupId);
        Save(data, $"story pickup {pickup.PickupId}");
    }

    public static void SaveObjective(string objectiveText, string secondaryObjectiveText)
    {
        SaveGameData data = CreateSnapshot();

        if (data == null)
            return;

        data.currentObjective = objectiveText ?? string.Empty;
        data.secondaryObjective = secondaryObjectiveText ?? string.Empty;
        Save(data, "objective");
    }

    public static SaveGameData LoadCurrentUserSave()
    {
        if (!AuthSession.IsLoggedIn)
            return null;

        return new LocalJsonSaveGameService().LoadSave(AuthSession.CurrentUser.userId);
    }

    public static void RestoreCurrentWorldState()
    {
        SaveGameData save = LoadCurrentUserSave();

        if (save == null)
            return;

        GameplaySaveLoader.RestoreWorldState(save);
    }

    private static SaveGameData CreateSnapshot()
    {
        if (IsRestoringSave || !AuthSession.IsLoggedIn)
            return null;

        AuthUserData user = AuthSession.CurrentUser;
        string sceneName = SceneManager.GetActiveScene().name;
        LocalJsonSaveGameService saveService = new LocalJsonSaveGameService();
        SaveGameData data = saveService.LoadSave(user.userId) ?? saveService.CreateNewSave(user.userId, sceneName);

        data.userId = user.userId;
        data.sceneName = sceneName;

        CheckpointVolume checkpoint = CheckpointManager.Instance != null
            ? CheckpointManager.Instance.CurrentCheckpoint
            : null;

        if (checkpoint != null)
            data.checkpointId = checkpoint.CheckpointId;

        GameObject playerRoot = ResolvePlayerRoot();

        if (playerRoot != null)
        {
            Transform playerTransform = playerRoot.transform;
            data.hasPlayerTransform = true;
            data.playerPositionX = playerTransform.position.x;
            data.playerPositionY = playerTransform.position.y;
            data.playerPositionZ = playerTransform.position.z;
            data.playerRotationY = playerTransform.eulerAngles.y;

            PlayerHealth playerHealth = playerRoot.GetComponentInChildren<PlayerHealth>(true);
            if (playerHealth != null)
                data.health = Mathf.CeilToInt(playerHealth.CurrentHealth);

            PlayerStats playerStats = playerRoot.GetComponentInChildren<PlayerStats>(true);
            if (playerStats != null)
                data.armor = Mathf.CeilToInt(playerStats.CurrentArmor);
        }

        GameplayHUDController hud = GameplayHUDController.Instance != null
            ? GameplayHUDController.Instance
            : Object.FindFirstObjectByType<GameplayHUDController>(FindObjectsInactive.Include);

        if (hud != null)
        {
            if (!string.IsNullOrWhiteSpace(hud.CurrentObjectiveText))
                data.currentObjective = hud.CurrentObjectiveText;

            data.secondaryObjective = hud.CurrentSecondaryObjectiveText ?? string.Empty;
        }

        return data;
    }

    private static GameObject ResolvePlayerRoot()
    {
        PlayerControlLock playerControlLock = Object.FindFirstObjectByType<PlayerControlLock>(FindObjectsInactive.Include);

        if (playerControlLock != null)
            return playerControlLock.gameObject;

        GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");

        if (taggedPlayer != null)
            return taggedPlayer;

        PlayerHealth playerHealth = Object.FindFirstObjectByType<PlayerHealth>(FindObjectsInactive.Include);
        return playerHealth != null ? playerHealth.gameObject : null;
    }

    private static void Save(SaveGameData data, string reason)
    {
        new LocalJsonSaveGameService().SaveGame(data);

        if (!string.IsNullOrWhiteSpace(reason))
            Debug.Log($"[GameplaySaveSystem] Saved progress: {reason}");
    }

    private static void AddUnique(System.Collections.Generic.List<string> list, string value)
    {
        if (list == null || string.IsNullOrWhiteSpace(value) || list.Contains(value))
            return;

        list.Add(value);
    }

    private static void RemoveValue(System.Collections.Generic.List<string> list, string value)
    {
        if (list == null || string.IsNullOrWhiteSpace(value))
            return;

        list.Remove(value);
    }
}
