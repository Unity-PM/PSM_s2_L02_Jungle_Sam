using System.Collections.Generic;
using UnityEngine;

public interface IEncounterResettable
{
    void ResetEncounter();
}

public class EncounterResetService : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool logReset = true;

    private readonly List<GameObject> _registeredEnemies = new List<GameObject>();
    private readonly List<IEncounterResettable> _resettableEncounters = new List<IEncounterResettable>();

    public void RegisterEncounter(IEncounterResettable encounter)
    {
        if (encounter == null || _resettableEncounters.Contains(encounter))
            return;

        _resettableEncounters.Add(encounter);
    }

    public void UnregisterEncounter(IEncounterResettable encounter)
    {
        if (encounter == null)
            return;

        _resettableEncounters.Remove(encounter);
    }

    public void RegisterSpawnedEnemy(GameObject enemy)
    {
        if (enemy == null || _registeredEnemies.Contains(enemy))
            return;

        _registeredEnemies.Add(enemy);
    }

    public void UnregisterSpawnedEnemy(GameObject enemy)
    {
        if (enemy == null)
            return;

        _registeredEnemies.Remove(enemy);
    }

    public void ResetActiveEncounter()
    {
        for (int i = _resettableEncounters.Count - 1; i >= 0; i--)
            _resettableEncounters[i]?.ResetEncounter();

        if (_registeredEnemies.Count > 0)
            ClearRegisteredEnemies();

        if (logReset)
            Debug.Log($"[{name}] Active encounter reset requested.");
    }

    public void ClearRegisteredEnemies()
    {
        for (int i = _registeredEnemies.Count - 1; i >= 0; i--)
        {
            GameObject enemy = _registeredEnemies[i];

            if (enemy != null)
                Destroy(enemy);
        }

        _registeredEnemies.Clear();
    }
}
