public interface ISaveGameService
{
    bool HasSave(string userId);
    SaveGameData LoadSave(string userId);
    void SaveGame(SaveGameData data);
    void DeleteSave(string userId);
    SaveGameData CreateNewSave(string userId, string gameplaySceneName);
}
