using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    [Header("WaveUI")]
    [SerializeField] private TextMeshProUGUI waveUIText;

    [Header("Notification UI")]
    [SerializeField] private GameObject notification;
    [SerializeField] private float notificationFadeInDuration = 0.25f;
    [SerializeField] private float notificationVisibleDuration = 0.6f;
    [SerializeField] private float notificationFadeOutDuration = 0.25f;
    [SerializeField] private float startScale = 0.5f; // Kích thước ban đầu lúc ẩn
    [SerializeField] private Ease showEase = Ease.OutBack; // Hiệu ứng lúc hiện (OutBack tạo cảm giác nẩy nhẹ)
    [SerializeField] private Ease hideEase = Ease.InBack;


    private void Start()
    {
        if (notification != null)
        {
            notification.gameObject.SetActive(false);
        }
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
        if (Waves == null && aliveMonsters ==0 || currentWaveIndex >= Waves.Count && aliveMonsters == 0)
        {
            Debug.Log("All waves completed!");
            GameManager.Instance?.OnGameWin();
            return;
        }

        //MonsterWave wave = Waves[currentWaveIndex];
        //Debug.Log($"Starting Wave {currentWaveIndex + 1}");
        //WaveUITextUpdate(currentWaveIndex + 1);
        //SpawnWave(wave);
        //if (wave.WaveDuration > 0f)
        //{
        //    waveTimerCoroutine = StartCoroutine(WaveTimer(wave.WaveDuration));
        //}
        StartCoroutine(ShowNotificationAndSpawn());
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

    private IEnumerator ShowNotificationAndSpawn()
    {
        if (notification != null && currentWaveIndex < Waves.Count)
        {
            // --- THIẾT LẬP BAN ĐẦU ---
            CanvasGroup canvasGroup = notification.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = notification.AddComponent<CanvasGroup>();

            // Đảm bảo ẩn hoàn toàn về Alpha và nhỏ lại về Scale
            canvasGroup.alpha = 0f;
            notification.transform.localScale = Vector3.one * startScale;
            notification.SetActive(true);

            // --- 1. HIỆU ỨNG XUẤT HIỆN (FADE IN + SCALE UP) ---
            // Chạy song song cả 2 hiệu ứng
            canvasGroup.DOFade(1f, notificationFadeInDuration);
            notification.transform.DOScale(1f, notificationFadeInDuration).SetEase(showEase);

            // Chờ hiệu ứng hiện xong + thời gian hiển thị
            yield return new WaitForSeconds(notificationFadeInDuration + notificationVisibleDuration);

            // --- 2. HIỆU ỨNG BIẾN MẤT (FADE OUT + SCALE DOWN) ---
            // Chạy song song cả 2 hiệu ứng
            canvasGroup.DOFade(0f, notificationFadeOutDuration);
            notification.transform.DOScale(startScale, notificationFadeOutDuration).SetEase(hideEase);

            // Chờ hiệu ứng ẩn xong
            yield return new WaitForSeconds(notificationFadeOutDuration);

            notification.SetActive(false);
        }

        // 3. Chờ thêm 1-2 giây ngẫu nhiên trước khi spawn như bạn yêu cầu
        yield return new WaitForSeconds(1f);

        // 4. Bắt đầu Spawn quái
        MonsterWave wave = Waves[currentWaveIndex];
        WaveUITextUpdate(currentWaveIndex + 1);
        SpawnWave(wave);

        if (wave.WaveDuration > 0f)
        {
            waveTimerCoroutine = StartCoroutine(WaveTimer(wave.WaveDuration));
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

    private void WaveUITextUpdate(int waveNumber)
    {
        if (waveUIText != null)
        {
            waveUIText.text = $"Wave: {waveNumber}/{Waves.Count}";
        }
    }
}
