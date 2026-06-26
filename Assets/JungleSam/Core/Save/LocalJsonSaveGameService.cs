using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LocalJsonSaveGameService : ISaveGameService
{
    private readonly bool _enableDebugLogs;

    public LocalJsonSaveGameService(bool enableDebugLogs = false)
    {
        _enableDebugLogs = enableDebugLogs;
    }

    public string SavesFolderPath => Path.Combine(GetDataRootPath(), "Saves");

    public bool HasSave(string userId)
    {
        string path = GetSavePath(userId);
        return !string.IsNullOrEmpty(path) && File.Exists(path);
    }

    public SaveGameData LoadSave(string userId)
    {
        string path = GetSavePath(userId);

        if (string.IsNullOrEmpty(path) || !File.Exists(path))
            return null;

        try
        {
            string json = File.ReadAllText(path);
            SaveGameData data = JsonUtility.FromJson<SaveGameData>(json);
            NormalizeSaveData(data);
            Log($"Loaded save for user '{userId}'. Path: {path}");
            return data;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Could not load save file. Path: {path}\n{exception}");
            return null;
        }
    }

    public void SaveGame(SaveGameData data)
    {
        if (data == null || string.IsNullOrWhiteSpace(data.userId))
        {
            Debug.LogWarning("SaveGame was called without valid SaveGameData or userId.");
            return;
        }

        EnsureSavesFolderExists();
        NormalizeSaveData(data);
        data.savedAtUtc = DateTime.UtcNow.ToString("o");

        string path = GetSavePath(data.userId);
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);

        Log($"Saved game for user '{data.userId}'. Path: {path}");
    }

    public void DeleteSave(string userId)
    {
        string path = GetSavePath(userId);

        if (string.IsNullOrEmpty(path) || !File.Exists(path))
            return;

        File.Delete(path);
        Log($"Deleted save for user '{userId}'. Path: {path}");
    }

    public SaveGameData CreateNewSave(string userId, string gameplaySceneName)
    {
        return new SaveGameData
        {
            userId = userId ?? string.Empty,
            sceneName = string.IsNullOrWhiteSpace(gameplaySceneName) ? string.Empty : gameplaySceneName.Trim(),
            checkpointId = string.Empty,
            missionStage = "DockStart",
            currentObjective = "Znajdź źródło sygnału",
            secondaryObjective = "Przedostań się przez nabrzeże",
            health = 100,
            armor = 100,
            activeWeaponId = string.Empty,
            ammo762 = 120,
            ammo9mm = 48,
            startedEncounters = new List<string>(),
            completedEncounters = new List<string>(),
            collectedStoryPickups = new List<string>(),
            savedAtUtc = DateTime.UtcNow.ToString("o")
        };
    }

    public string GetSavePathForDebug(string userId)
    {
        return GetSavePath(userId);
    }

    private string GetSavePath(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return string.Empty;

        return Path.Combine(SavesFolderPath, $"{SanitizeFileName(userId)}_save.json");
    }

    private static string GetDataRootPath()
    {
        return Path.Combine(Application.persistentDataPath, "JungleSam");
    }

    private void EnsureSavesFolderExists()
    {
        Directory.CreateDirectory(SavesFolderPath);
    }

    private static void NormalizeSaveData(SaveGameData data)
    {
        if (data == null)
            return;

        data.userId ??= string.Empty;
        data.sceneName ??= string.Empty;
        data.checkpointId ??= string.Empty;
        if (data.checkpointId == "Checkpoint_Start_Boat")
            data.checkpointId = string.Empty;
        else if (data.checkpointId == "Checkpoint_Start")
            data.checkpointId = "Checkpoint_AfterDockArena";
        else if (data.checkpointId == "Checkpoint")
            data.checkpointId = "Checkpoint_Church";
        data.missionStage ??= string.Empty;
        data.currentObjective ??= string.Empty;
        data.secondaryObjective ??= string.Empty;
        data.activeWeaponId ??= string.Empty;
        data.startedEncounters ??= new List<string>();
        data.completedEncounters ??= new List<string>();
        data.collectedStoryPickups ??= new List<string>();
        data.savedAtUtc ??= string.Empty;
    }

    private static string SanitizeFileName(string value)
    {
        string sanitized = value.Trim();

        foreach (char invalidChar in Path.GetInvalidFileNameChars())
            sanitized = sanitized.Replace(invalidChar, '_');

        return sanitized;
    }

    private void Log(string message)
    {
        if (_enableDebugLogs)
            Debug.Log($"[LocalJsonSaveGameService] {message}");
    }
}
