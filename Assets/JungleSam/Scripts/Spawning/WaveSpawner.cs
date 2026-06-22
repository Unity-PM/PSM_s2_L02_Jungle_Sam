using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;



public class WaveSpawner : MonoBehaviour, IEncounterResettable
{
    [System.Serializable]
    private class SpawnerUnityEvent : UnityEvent<WaveSpawner> { }

    [System.Serializable]
    private class WaveUnityEvent : UnityEvent<int, string> { }

    [System.Serializable]
    private class EnemyUnityEvent : UnityEvent<GameObject> { }

    [System.Serializable]
    private class CountUnityEvent : UnityEvent<int> { }

    public event System.Action<WaveSpawner> SpawnerStarted;
    public event System.Action<WaveSpawner> SpawnerFinished;
    public event System.Action<WaveSpawner, int, string> WaveStarted;
    public event System.Action<WaveSpawner, int, string> WaveCompleted;
    public event System.Action<WaveSpawner, GameObject> EnemySpawned;
    public event System.Action<WaveSpawner, EnemyAI> SpawnedEnemyDied;

    private enum SpawnState
    {
        Idle,
        Counting,
        Spawning,
        Waiting,
        Finished
    }

    [System.Serializable]
    public class Wave
    {
        public string waveName = "Wave";
        public GameObject enemyPrefab;
        public GameObject[] alternateEnemyPrefabs;
        public bool randomizeEnemyPrefab = false;
        public int count = 5;
        public float rate = 1f;
        public int maxAliveAtOnce = 5;
        public float startDelay = -1f;
        public float completionDelay = 0f;
    }

    [Header("Waves")]
    [SerializeField] private Wave[] waves;
    [SerializeField] private bool loopWaves = false;
    [SerializeField] private bool startOnPlay = true;
    [SerializeField] private float timeBetweenWaves = 5f;
    [SerializeField] private bool reuseLastValidPrefabForEmptyWaves = true;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float navMeshSampleRadius = 2f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI waveText;

    [Header("Events")]
    [SerializeField] private SpawnerUnityEvent onSpawnerStarted;
    [SerializeField] private SpawnerUnityEvent onSpawnerFinished;
    [SerializeField] private WaveUnityEvent onWaveStarted;
    [SerializeField] private WaveUnityEvent onWaveCompleted;
    [SerializeField] private EnemyUnityEvent onEnemySpawned;
    [SerializeField] private EnemyUnityEvent onEnemyDied;
    [SerializeField] private CountUnityEvent onEnemiesAliveChanged;

    [Header("Death Reset")]
    [SerializeField] private EncounterResetService encounterResetService;
    [SerializeField] private bool registerWithEncounterResetService = true;
    [SerializeField] private bool resetToFirstWaveOnPlayerDeath = true;
    [SerializeField] private bool restartAfterDeathReset = false;

    private int _nextWaveIndex;
    private int _enemiesAlive;
    private int _enemiesSpawnedInCurrentWave;
    private float _waveCountdown;
    private SpawnState _state = SpawnState.Idle;
    private Coroutine _spawnRoutine;
    private readonly List<GameObject> _spawnedEnemyObjects = new List<GameObject>();
    private readonly List<EnemyAI> _spawnedEnemies = new List<EnemyAI>();
    private readonly List<MutantStalkerAI> _spawnedMutants = new List<MutantStalkerAI>();

    public bool IsFinished => _state == SpawnState.Finished;
    public bool IsRunning => _state == SpawnState.Counting || _state == SpawnState.Spawning || _state == SpawnState.Waiting;
    public int CurrentWaveIndex => waves != null && waves.Length > 0 ? Mathf.Clamp(_nextWaveIndex, 0, waves.Length - 1) : 0;
    public int TotalWaves => waves != null ? waves.Length : 0;
    public int EnemiesAlive => _enemiesAlive;
    public int EnemiesSpawnedInCurrentWave => _enemiesSpawnedInCurrentWave;

    private void Awake()
    {
        ResolveEncounterResetService();
    }

    private void OnEnable()
    {
        if (registerWithEncounterResetService && encounterResetService != null)
            encounterResetService.RegisterEncounter(this);
    }

    private void OnDisable()
    {
        if (encounterResetService != null)
            encounterResetService.UnregisterEncounter(this);
    }

    private void Start()
    {
        ValidateSetup();

        _waveCountdown = timeBetweenWaves;

        if (startOnPlay)
            StartSpawner();
        else
            SetWaveText("Arena inactive");
    }

    private void Update()
    {
        if (_state != SpawnState.Counting)
            return;

        _waveCountdown -= Time.deltaTime;
        SetWaveText($"Next wave: {Mathf.CeilToInt(_waveCountdown)}");

        if (_waveCountdown <= 0f)
            StartCurrentWave();
    }

    public void StartSpawner()
    {
        if (_state != SpawnState.Idle && _state != SpawnState.Finished)
            return;

        if (waves == null || waves.Length == 0)
        {
            Debug.LogError($"[{name}] Brak fal w WaveSpawner.");
            return;
        }

        _state = SpawnState.Counting;
        _waveCountdown = GetCurrentWaveDelay();
        SpawnerStarted?.Invoke(this);
        onSpawnerStarted?.Invoke(this);
    }

    [ContextMenu("Restart Spawner From First Wave")]
    public void RestartFromFirstWave()
    {
        ResetEncounter();
        _nextWaveIndex = 0;
        StartSpawner();
    }

    [ContextMenu("Stop Spawner And Clear Enemies")]
    public void StopSpawnerAndClearEnemies()
    {
        if (_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }

        ClearSpawnedEnemies();
        _enemiesAlive = 0;
        _enemiesSpawnedInCurrentWave = 0;
        _waveCountdown = timeBetweenWaves;
        _state = SpawnState.Idle;
        SetWaveText("Arena inactive");
        NotifyEnemiesAliveChanged();
    }

    [ContextMenu("Skip Current Wave")]
    public void SkipCurrentWave()
    {
        if (_state == SpawnState.Finished || waves == null || waves.Length == 0)
            return;

        if (_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }

        ClearSpawnedEnemies();
        _enemiesAlive = 0;
        _enemiesSpawnedInCurrentWave = 0;
        NotifyEnemiesAliveChanged();
        CompleteWave();
    }

    private void StartCurrentWave()
    {
        if (_spawnRoutine != null)
            StopCoroutine(_spawnRoutine);

        if (_nextWaveIndex < 0 || _nextWaveIndex >= waves.Length)
        {
            FinishSpawner();
            return;
        }

        _spawnRoutine = StartCoroutine(SpawnWave(waves[_nextWaveIndex]));
    }

    private IEnumerator SpawnWave(Wave wave)
    {
        _state = SpawnState.Spawning;
        _enemiesAlive = 0;
        _enemiesSpawnedInCurrentWave = 0;

        SetWaveText($"Wave: {wave.waveName}");
        Debug.Log($"[{name}] Start fali: {wave.waveName}");
        WaveStarted?.Invoke(this, _nextWaveIndex, wave.waveName);
        onWaveStarted?.Invoke(_nextWaveIndex, wave.waveName);
        NotifyEnemiesAliveChanged();

        if (GetEnemyPrefabForCurrentWave(wave) == null)
        {
            Debug.LogError($"[{name}] Fala {wave.waveName} nie ma żadnego poprawnego enemyPrefab.");
            CompleteWave();
            yield break;
        }

        int targetCount = Mathf.Max(0, wave.count);
        float spawnDelay = wave.rate > 0f ? 1f / wave.rate : 1f;

        while (_enemiesSpawnedInCurrentWave < targetCount)
        {
            GameObject enemyPrefab = GetEnemyPrefabForCurrentWave(wave);

            if (enemyPrefab == null)
            {
                Debug.LogError($"[{name}] Fala {wave.waveName} ma zniszczony albo niepodpięty enemyPrefab. Przerywam falę.");
                _state = SpawnState.Idle;
                yield break;
            }

            if (_enemiesAlive < Mathf.Max(1, wave.maxAliveAtOnce))
            {
                bool spawned = TrySpawnEnemy(enemyPrefab);

                if (spawned)
                    _enemiesSpawnedInCurrentWave++;

                yield return new WaitForSeconds(spawnDelay);
            }
            else
            {
                yield return null;
            }
        }

        _state = SpawnState.Waiting;
        SetWaveText($"Enemies left: {_enemiesAlive}");

        while (_enemiesAlive > 0)
        {
            SetWaveText($"Enemies left: {_enemiesAlive}");
            yield return null;
        }

        if (wave.completionDelay > 0f)
            yield return new WaitForSeconds(wave.completionDelay);

        CompleteWave();
    }

    private bool TrySpawnEnemy(GameObject enemyPrefab)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError($"[{name}] Nie można zespawnować przeciwnika: enemyPrefab jest pusty albo został zniszczony.");
            return false;
        }

        Transform spawnPoint = GetRandomValidSpawnPoint();

        if (spawnPoint == null)
        {
            Debug.LogWarning($"[{name}] Brak poprawnych spawn pointów. Używam pozycji WaveSpawner jako awaryjnego miejsca spawnu.", this);
            spawnPoint = transform;
        }

        Vector3 spawnPosition = spawnPoint.position;
        Quaternion spawnRotation = spawnPoint.rotation;

        if (NavMesh.SamplePosition(spawnPosition, out NavMeshHit hit, navMeshSampleRadius, NavMesh.AllAreas))
            spawnPosition = hit.position;
        else
            Debug.LogWarning($"[{name}] Spawn point {spawnPoint.name} nie leży blisko NavMesh.");

        GameObject enemyObj = Instantiate(enemyPrefab, spawnPosition, spawnRotation);

        EnemyAI enemyAI = enemyObj.GetComponentInChildren<EnemyAI>(true);
        MutantStalkerAI mutantAI = enemyObj.GetComponentInChildren<MutantStalkerAI>(true);

        if (enemyAI == null && mutantAI == null)
        {
            Debug.LogError($"[{name}] Prefab {enemyPrefab.name} nie ma EnemyAI ani MutantStalkerAI. Użyj gameplayowego prefabu przeciwnika, nie samego modelu/mesha.");
            Destroy(enemyObj);
            return false;
        }

        _spawnedEnemyObjects.Add(enemyObj);

        if (enemyAI != null)
        {
            enemyAI.Died += OnSpawnedEnemyDied;
            _spawnedEnemies.Add(enemyAI);
        }

        if (mutantAI != null)
        {
            mutantAI.Died += OnSpawnedMutantDied;
            _spawnedMutants.Add(mutantAI);
        }

        if (encounterResetService != null)
            encounterResetService.RegisterSpawnedEnemy(enemyObj);

        _enemiesAlive++;
        EnemySpawned?.Invoke(this, enemyObj);
        onEnemySpawned?.Invoke(enemyObj);
        NotifyEnemiesAliveChanged();

        return true;
    }

    private void OnSpawnedEnemyDied(EnemyAI enemy)
    {
        if (enemy != null)
        {
            enemy.Died -= OnSpawnedEnemyDied;
            _spawnedEnemies.Remove(enemy);

            GameObject enemyRoot = FindSpawnedRoot(enemy.gameObject);

            if (enemyRoot != null)
                _spawnedEnemyObjects.Remove(enemyRoot);

            if (encounterResetService != null)
                encounterResetService.UnregisterSpawnedEnemy(enemyRoot != null ? enemyRoot : enemy.gameObject);

            SpawnedEnemyDied?.Invoke(this, enemy);
            onEnemyDied?.Invoke(enemy.gameObject);
        }

        _enemiesAlive = Mathf.Max(0, _enemiesAlive - 1);
        NotifyEnemiesAliveChanged();
    }

    private void OnSpawnedMutantDied(MutantStalkerAI mutant)
    {
        if (mutant != null)
        {
            mutant.Died -= OnSpawnedMutantDied;
            _spawnedMutants.Remove(mutant);

            GameObject mutantRoot = FindSpawnedRoot(mutant.gameObject);

            if (mutantRoot != null)
                _spawnedEnemyObjects.Remove(mutantRoot);

            if (encounterResetService != null)
                encounterResetService.UnregisterSpawnedEnemy(mutantRoot != null ? mutantRoot : mutant.gameObject);

            onEnemyDied?.Invoke(mutant.gameObject);
        }

        _enemiesAlive = Mathf.Max(0, _enemiesAlive - 1);
        NotifyEnemiesAliveChanged();
    }

    public void ResetEncounter()
    {
        if (_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }

        ClearSpawnedEnemies();

        if (resetToFirstWaveOnPlayerDeath)
            _nextWaveIndex = 0;

        _enemiesAlive = 0;
        _enemiesSpawnedInCurrentWave = 0;
        _waveCountdown = timeBetweenWaves;
        _state = SpawnState.Idle;

        SetWaveText(restartAfterDeathReset ? "Arena reset" : "Arena inactive");

        if (restartAfterDeathReset)
            StartSpawner();
    }

    private void CompleteWave()
    {
        Debug.Log($"[{name}] Fala ukończona.");
        WaveCompleted?.Invoke(this, _nextWaveIndex, GetCurrentWaveName());
        onWaveCompleted?.Invoke(_nextWaveIndex, GetCurrentWaveName());

        if (_nextWaveIndex + 1 >= waves.Length)
        {
            if (!loopWaves)
            {
                FinishSpawner();
                return;
            }

            _nextWaveIndex = 0;
        }
        else
        {
            _nextWaveIndex++;
        }

        _waveCountdown = GetCurrentWaveDelay();
        _state = SpawnState.Counting;
    }

    private void ValidateSetup()
    {
        if (GetValidSpawnPointCount() == 0)
            Debug.LogWarning($"[{name}] Brak poprawnych spawn pointów w Inspectorze. Fala użyje pozycji WaveSpawner jako fallbacku.", this);

        if (waves == null || waves.Length == 0)
            Debug.LogError($"[{name}] Brak Waves w Inspectorze.");
    }

    private Transform GetRandomValidSpawnPoint()
    {
        int validCount = GetValidSpawnPointCount();

        if (validCount == 0)
            return null;

        int selectedIndex = Random.Range(0, validCount);
        int currentIndex = 0;

        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint == null)
                continue;

            if (currentIndex == selectedIndex)
                return spawnPoint;

            currentIndex++;
        }

        return null;
    }

    private int GetValidSpawnPointCount()
    {
        if (spawnPoints == null)
            return 0;

        int count = 0;

        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint != null)
                count++;
        }

        return count;
    }

    private void SetWaveText(string text)
    {
        if (waveText != null)
            waveText.text = text;
    }

    private void FinishSpawner()
    {
        _state = SpawnState.Finished;
        SetWaveText("All waves completed");
        SpawnerFinished?.Invoke(this);
        onSpawnerFinished?.Invoke(this);
    }

    private float GetCurrentWaveDelay()
    {
        if (waves == null || _nextWaveIndex < 0 || _nextWaveIndex >= waves.Length)
            return timeBetweenWaves;

        float waveDelay = waves[_nextWaveIndex].startDelay;
        return waveDelay >= 0f ? waveDelay : timeBetweenWaves;
    }

    private string GetCurrentWaveName()
    {
        if (waves == null || _nextWaveIndex < 0 || _nextWaveIndex >= waves.Length)
            return string.Empty;

        return waves[_nextWaveIndex].waveName;
    }

    private static bool HasAnyEnemyPrefab(Wave wave)
    {
        if (wave == null)
            return false;

        if (wave.enemyPrefab != null)
            return true;

        if (wave.alternateEnemyPrefabs == null)
            return false;

        foreach (GameObject prefab in wave.alternateEnemyPrefabs)
        {
            if (prefab != null)
                return true;
        }

        return false;
    }

    private static GameObject GetEnemyPrefabForSpawn(Wave wave)
    {
        if (wave == null)
            return null;

        if (!wave.randomizeEnemyPrefab)
            return wave.enemyPrefab;

        List<GameObject> validPrefabs = new List<GameObject>();

        if (wave.enemyPrefab != null)
            validPrefabs.Add(wave.enemyPrefab);

        if (wave.alternateEnemyPrefabs != null)
        {
            foreach (GameObject prefab in wave.alternateEnemyPrefabs)
            {
                if (prefab != null)
                    validPrefabs.Add(prefab);
            }
        }

        if (validPrefabs.Count == 0)
            return null;

        return validPrefabs[Random.Range(0, validPrefabs.Count)];
    }

    private GameObject GetEnemyPrefabForCurrentWave(Wave wave)
    {
        GameObject prefab = GetEnemyPrefabForSpawn(wave);

        if (prefab != null || !reuseLastValidPrefabForEmptyWaves || waves == null)
            return prefab;

        for (int i = _nextWaveIndex - 1; i >= 0; i--)
        {
            prefab = GetEnemyPrefabForSpawn(waves[i]);

            if (prefab != null)
            {
                Debug.LogWarning($"[{name}] Fala {wave.waveName} nie ma prefabów. Używam prefabów z wcześniejszej fali: {waves[i].waveName}.");
                return prefab;
            }
        }

        return null;
    }

    private void NotifyEnemiesAliveChanged()
    {
        onEnemiesAliveChanged?.Invoke(_enemiesAlive);
    }

    private void ResolveEncounterResetService()
    {
        if (encounterResetService == null)
            encounterResetService = FindFirstObjectByType<EncounterResetService>();
    }

    private void ClearSpawnedEnemies()
    {
        for (int i = _spawnedEnemies.Count - 1; i >= 0; i--)
        {
            EnemyAI enemy = _spawnedEnemies[i];

            if (enemy == null)
                continue;

            enemy.Died -= OnSpawnedEnemyDied;
        }

        for (int i = _spawnedMutants.Count - 1; i >= 0; i--)
        {
            MutantStalkerAI mutant = _spawnedMutants[i];

            if (mutant == null)
                continue;

            mutant.Died -= OnSpawnedMutantDied;
        }

        for (int i = _spawnedEnemyObjects.Count - 1; i >= 0; i--)
        {
            GameObject enemyObject = _spawnedEnemyObjects[i];

            if (enemyObject == null)
                continue;

            if (encounterResetService != null)
                encounterResetService.UnregisterSpawnedEnemy(enemyObject);

            if (enemyObject != null)
                Destroy(enemyObject);
        }

        _spawnedEnemyObjects.Clear();
        _spawnedEnemies.Clear();
        _spawnedMutants.Clear();
    }

    private GameObject FindSpawnedRoot(GameObject childOrRoot)
    {
        if (childOrRoot == null)
            return null;

        for (int i = _spawnedEnemyObjects.Count - 1; i >= 0; i--)
        {
            GameObject root = _spawnedEnemyObjects[i];

            if (root == null)
                continue;

            if (root == childOrRoot || childOrRoot.transform.IsChildOf(root.transform))
                return root;
        }

        return null;
    }
}
