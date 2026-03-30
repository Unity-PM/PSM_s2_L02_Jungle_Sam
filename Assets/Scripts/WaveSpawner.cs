using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;


[System.Serializable]
public class Wave
{
    public string waveName;
    public GameObject enemyPrefab;
    public int count;
    public float rate; // co ile sekund pojawia się wróg
}



public class WaveSpawner : MonoBehaviour
{
    public enum SpawnState { Spawning, Waiting, Counting };

    public Wave[] waves;
    public Transform[] spawnPoints;
    public TextMeshProUGUI waveText; // UI do wyświetlania numeru fali

    private int _nextWave = 0;
    private SpawnState _state = SpawnState.Counting;
    private float _waveCountdown = 5f; // czas do pierwszej fali
    private float _searchCountdown = 1f;

    private void Start()
    {
        if (spawnPoints.Length == 0)
        {
            Debug.LogError("Brak punktów spawn! Dodaj je do WaveSpawner.");
        }
    }

    private void Update()
    {
        if (_state == SpawnState.Waiting)
        {
            if (!EnemyIsAlive()) WaveCompleted();
            else return;
        }

        if (_waveCountdown <= 0)
        {
            if (_state != SpawnState.Spawning) StartCoroutine(SpawnWave(waves[_nextWave]));
        }
        else
        {
            _waveCountdown -= Time.deltaTime;
        }
    }

    void WaveCompleted()
    {
        Debug.Log("Fala ukończona!");
        _state = SpawnState.Counting;
        _waveCountdown = 5f; // 10 sekund przerwy między falami

        if (_nextWave + 1 > waves.Length - 1)
        {
            _nextWave = 0; // Zapętlamy fale lub kończymy grę
            Debug.Log("Wszystkie fale pokonane! Zaczynamy od nowa (zwiększ poziom trudności)");
        }
        else
        {
            _nextWave++;
        }
    }

    bool EnemyIsAlive()
    {
        _searchCountdown -= Time.deltaTime;
        if (_searchCountdown <= 0f)
        {
            _searchCountdown = 1f;
            return GameObject.FindGameObjectWithTag("Enemy") != null;
        }
        return true;
    }

    IEnumerator SpawnWave(Wave wave)
    {
        Debug.Log("Spawnowanie fali: " + wave.waveName);
        _state = SpawnState.Spawning;
        if (waveText != null) waveText.text = "Wave: " + wave.waveName;

        for (int i = 0; i < wave.count; i++)
        {
            SpawnEnemy(wave.enemyPrefab);
            yield return new WaitForSeconds(1f / wave.rate);
        }

        _state = SpawnState.Waiting;
        yield break;
    }

    void SpawnEnemy(GameObject enemy)
    {
        Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Instantiate(enemy, sp.position, sp.rotation);
    }
}







