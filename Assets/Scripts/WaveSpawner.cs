using UnityEngine;
using System.Collections;
using TMPro;

public class WaveSpawner : MonoBehaviour
{
    public enum SpawnState { Spawning, Waiting, Counting };

    [System.Serializable]
    public class Wave
    {
        public string waveName;
        public GameObject enemyPrefab;
        public int count;
        public float rate;
    }

    public Wave[] waves;
    public Transform[] spawnPoints;
    public TextMeshProUGUI waveText;

    private int _nextWave = 0;
    private SpawnState _state = SpawnState.Counting;
    private float _waveCountdown = 5f;

    // Licznik żywych wrogów w obecnej fali
    private int _enemiesAlive = 0;

    private void Start()
    {
        if (spawnPoints.Length == 0)
        {
            Debug.LogError("Brak punktów spawn w WaveSpawner!");
        }
    }

    private void OnEnable()
    {
        // Zapisujemy się na "wiedzę" o śmierci wroga
        EnemyAI.OnEnemyDied += TrackEnemyDeath;
    }

    private void OnDisable()
    {
        // Sprzątamy pamięć przy wyłączeniu obiektu
        EnemyAI.OnEnemyDied -= TrackEnemyDeath;
    }

    private void Update()
    {
        if (_state == SpawnState.Waiting)
        {
            if (_enemiesAlive <= 0)
            {
                WaveCompleted();
            }
            else
            {
                return; // Czekamy aż gracz wyrżnie resztę fali
            }
        }

        if (_waveCountdown <= 0)
        {
            if (_state != SpawnState.Spawning)
            {
                StartCoroutine(SpawnWave(waves[_nextWave]));
            }
        }
        else
        {
            _waveCountdown -= Time.deltaTime;
        }
    }

    private void TrackEnemyDeath()
    {
        _enemiesAlive--;
    }

    void WaveCompleted()
    {
        Debug.Log("Fala ukończona!");
        _state = SpawnState.Counting;
        _waveCountdown = 5f; // 5 sekund przerwy między falami

        if (_nextWave + 1 > waves.Length - 1)
        {
            _nextWave = 0; // Zapętlamy fale w MVP
            Debug.Log("Wszystkie fale pokonane! Reset pętli.");
        }
        else
        {
            _nextWave++;
        }
    }

    IEnumerator SpawnWave(Wave wave)
    {
        Debug.Log("Spawnowanie fali: " + wave.waveName);
        _state = SpawnState.Spawning;
        if (waveText != null) waveText.text = "Wave: " + wave.waveName;

        _enemiesAlive = wave.count; // Ustawiamy oczekiwaną liczbę wrogów do zabicia

        for (int i = 0; i < wave.count; i++)
        {
            SpawnEnemy(wave.enemyPrefab);
            yield return new WaitForSeconds(1f / wave.rate);
        }

        _state = SpawnState.Waiting;
    }

    void SpawnEnemy(GameObject enemy)
    {
        if (spawnPoints.Length == 0) return;
        Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Instantiate(enemy, sp.position, sp.rotation);
    }
}   