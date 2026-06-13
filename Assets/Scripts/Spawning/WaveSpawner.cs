using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;



public class WaveSpawner : MonoBehaviour
{
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
        public int count = 5;
        public float rate = 1f;
        public int maxAliveAtOnce = 5;
    }

    [Header("Waves")]
    [SerializeField] private Wave[] waves;
    [SerializeField] private bool loopWaves = false;
    [SerializeField] private bool startOnPlay = true;
    [SerializeField] private float timeBetweenWaves = 5f;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float navMeshSampleRadius = 2f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI waveText;

    private int _nextWaveIndex;
    private int _enemiesAlive;
    private int _enemiesSpawnedInCurrentWave;
    private float _waveCountdown;
    private SpawnState _state = SpawnState.Idle;
    private Coroutine _spawnRoutine;

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
        _waveCountdown = timeBetweenWaves;
    }

    private void StartCurrentWave()
    {
        if (_spawnRoutine != null)
            StopCoroutine(_spawnRoutine);

        _spawnRoutine = StartCoroutine(SpawnWave(waves[_nextWaveIndex]));
    }

    private IEnumerator SpawnWave(Wave wave)
    {
        _state = SpawnState.Spawning;
        _enemiesAlive = 0;
        _enemiesSpawnedInCurrentWave = 0;

        SetWaveText($"Wave: {wave.waveName}");
        Debug.Log($"[{name}] Start fali: {wave.waveName}");

        if (wave.enemyPrefab == null)
        {
            Debug.LogError($"[{name}] Fala {wave.waveName} nie ma enemyPrefab.");
            CompleteWave();
            yield break;
        }

        int targetCount = Mathf.Max(0, wave.count);
        float spawnDelay = wave.rate > 0f ? 1f / wave.rate : 1f;

        while (_enemiesSpawnedInCurrentWave < targetCount)
        {
            if (_enemiesAlive < Mathf.Max(1, wave.maxAliveAtOnce))
            {
                bool spawned = TrySpawnEnemy(wave.enemyPrefab);

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

        CompleteWave();
    }

    private bool TrySpawnEnemy(GameObject enemyPrefab)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError($"[{name}] Brak spawn pointów.");
            return false;
        }

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        Vector3 spawnPosition = spawnPoint.position;
        Quaternion spawnRotation = spawnPoint.rotation;

        if (NavMesh.SamplePosition(spawnPosition, out NavMeshHit hit, navMeshSampleRadius, NavMesh.AllAreas))
            spawnPosition = hit.position;
        else
            Debug.LogWarning($"[{name}] Spawn point {spawnPoint.name} nie leży blisko NavMesh.");

        GameObject enemyObj = Instantiate(enemyPrefab, spawnPosition, spawnRotation);

        if (!enemyObj.TryGetComponent(out EnemyAI enemyAI))
        {
            Debug.LogError($"[{name}] Prefab {enemyPrefab.name} nie ma EnemyAI.");
            Destroy(enemyObj);
            return false;
        }

        enemyAI.Died += OnSpawnedEnemyDied;
        _enemiesAlive++;

        return true;
    }

    private void OnSpawnedEnemyDied(EnemyAI enemy)
    {
        if (enemy != null)
            enemy.Died -= OnSpawnedEnemyDied;

        _enemiesAlive = Mathf.Max(0, _enemiesAlive - 1);
    }

    private void CompleteWave()
    {
        Debug.Log($"[{name}] Fala ukończona.");

        if (_nextWaveIndex + 1 >= waves.Length)
        {
            if (!loopWaves)
            {
                _state = SpawnState.Finished;
                SetWaveText("All waves completed");
                return;
            }

            _nextWaveIndex = 0;
        }
        else
        {
            _nextWaveIndex++;
        }

        _waveCountdown = timeBetweenWaves;
        _state = SpawnState.Counting;
    }

    private void ValidateSetup()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            Debug.LogError($"[{name}] Brak spawn pointów w Inspectorze.");

        if (waves == null || waves.Length == 0)
            Debug.LogError($"[{name}] Brak Waves w Inspectorze.");
    }

    private void SetWaveText(string text)
    {
        if (waveText != null)
            waveText.text = text;
    }
}