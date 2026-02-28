using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterWaveSpawner : MonoBehaviour
{
    [Header("Wave Data")]
    public List<MonsterWave> Waves;

    [Header("References")]
    public GridMapSpawner Grid;

    [Header("Spawn Setting")]
    public float delayBetweenSpawns = 0.5f;
    private int currentWaveIndex = -1;
    private int aliveMonsters = 0;
    private Coroutine waveTimerCoroutine;


    private void Start()
    {
        if (Waves == null || Waves.Count == 0)
        {
            Debug.LogWarning("MonsterWaveSpawner: No waves configured!");
            return;
        }
        if (Grid == null)
        {
            Debug.LogError("MonsterWaveSpawner: Grid reference is missing!");
            return;
        }
        StartNextWave();
    }

    private void StartNextWave()
    {
        currentWaveIndex++;
        if (Waves == null || currentWaveIndex >= Waves.Count)
        {
            Debug.Log("All waves completed!");
            return;
        }

        MonsterWave wave = Waves[currentWaveIndex];
        Debug.Log($"Starting Wave {currentWaveIndex + 1}");
        SpawnWave(wave);
        if (wave.WaveDuration > 0f)
        {
            waveTimerCoroutine = StartCoroutine(WaveTimer(wave.WaveDuration));
        }
    }


    private void SpawnWave(MonsterWave wave)
    {
        if (wave == null || wave.MonsterGroups == null)
        {
            Debug.LogWarning("MonsterWaveSpawner: Wave or MonsterGroups is null!");
            return;
        }
        foreach (MonsterGroup group in wave.MonsterGroups)
        {
            if (group != null)
            {
                StartCoroutine(SpawnMonsterGroup(group));
            }
        }
    }

    private IEnumerator SpawnMonsterGroup(MonsterGroup group)
    {
        for (int i = 0; i < group.Count; i++)
        {
            SpawnEnemy(group.MonsterPrefab);
            yield return new WaitForSeconds(delayBetweenSpawns);
        }
    }

    private void SpawnEnemy(GameObject monsterPrefab)
    {
        if (monsterPrefab == null)
        {
            Debug.LogWarning("MonsterWaveSpawner: Monster prefab is null!");
            return;
        }
        if (Grid == null)
        {
            Debug.LogError("MonsterWaveSpawner: Grid reference is missing!");
            return;
        }

        Vector2Int cell = Grid.GetRandomEmptyCell();
        Vector3 pos = Grid.GridToWorld(cell);

        GameObject monster = Instantiate(monsterPrefab, pos + new Vector3(0,0.5f,0), Quaternion.identity);
        aliveMonsters++;

        monster.GetComponent<Monster>()?.Init(this);
    }

    public void OnEnemyDied()
    {
        aliveMonsters--;
        if (aliveMonsters <= 0)
        {
            if (waveTimerCoroutine != null)
            {
                StopCoroutine(waveTimerCoroutine);
            }
            StartNextWave();
        }
    }

    private IEnumerator WaveTimer(float duration)
    {
        yield return new WaitForSeconds(duration);
        Debug.Log("Wave timeout!");
        StartNextWave();
    }
}
