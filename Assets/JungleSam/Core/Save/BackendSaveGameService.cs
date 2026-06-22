using System;

public class BackendSaveGameService : ISaveGameService
{
    // TODO: Replace LocalJsonSaveGameService with this implementation when the online backend is available.
    // This placeholder intentionally performs no HTTP requests in the MVP.

    public bool HasSave(string userId)
    {
        throw new NotImplementedException("Backend save game service is not implemented yet.");
    }

    public SaveGameData LoadSave(string userId)
    {
        throw new NotImplementedException("Backend save game service is not implemented yet.");
    }

    public void SaveGame(SaveGameData data)
    {
        throw new NotImplementedException("Backend save game service is not implemented yet.");
    }

    public void DeleteSave(string userId)
    {
        throw new NotImplementedException("Backend save game service is not implemented yet.");
    }

    public SaveGameData CreateNewSave(string userId, string gameplaySceneName)
    {
        throw new NotImplementedException("Backend save game service is not implemented yet.");
    }
}
