// SpawnerManager.cs
// Полный контроль над волнами и спавном врагов с разных точек
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [System.Serializable]
    public class WaveEnemy
    {
        [Header("Тип врага")]
        public GameObject enemyPrefab;

        [Header("Количество")]
        public int count;

        [Header("Задержка между спавнами (сек)")]
        public float spawnDelay;

        [Header("С какой точки спавнить (опционально)")]
        public Transform specificSpawnPoint; // null = случайная точка
    }

    [System.Serializable]
    public class Wave
    {
        [Header("Название волны")]
        public string waveName;

        [Header("Какие враги и в каком порядке")]
        public List<WaveEnemy> enemies = new List<WaveEnemy>();

        [Header("Задержка перед волной (сек)")]
        public float preWaveDelay;

        [Header("Задержка после волны (сек)")]
        public float postWaveDelay;

        [Header("Можно ли пропустить волну (опционально)")]
        public bool canSkip;
    }

    [Header("Главные настройки")]
    [Tooltip("Ссылка на сеть путей (WaypointNetwork)")]
    public WaypointNetwork pathNetwork;

    [Tooltip("Все возможные точки спавна на карте")]
    public Transform[] spawnPoints;

    [Tooltip("Список всех волн")]
    public List<Wave> waves = new List<Wave>();

    [Header("Настройки отладки")]
    public bool autoStart;
    public bool loopWaves;  // Зациклить волны после последней
    public float globalSpawnDelay; // Глобальная задержка между всеми спавнами

    [Header("События (опционально)")]
    public UnityEngine.Events.UnityEvent onWaveStarted;
    public UnityEngine.Events.UnityEvent onWaveCompleted;
    public UnityEngine.Events.UnityEvent onAllWavesCompleted;

    // Приватные переменные
    private int currentWaveIndex = 0;
    private bool isSpawning = false;
    private bool allWavesComplete = false;
    private int totalEnemiesSpawned = 0;
    private int totalEnemiesKilled = 0;

    // Свойства для доступа извне
    public int CurrentWaveIndex => currentWaveIndex;
    public bool IsSpawning => isSpawning;
    public bool AllWavesComplete => allWavesComplete;
    public float WaveProgress => GetWaveProgress();

    private void Start()
    {
        if (autoStart)
        {
            StartWaves();
        }
    }

    private void Update()
    {
        // Для отладки - пропуск волны по клавише (если разрешено)
        if (Input.GetKeyDown(KeyCode.Space) && isSpawning && waves[currentWaveIndex].canSkip)
        {
            SkipCurrentWave();
        }
    }

    /// <summary>
    /// Запустить все волны с начала
    /// </summary>
    public void StartWaves()
    {
        if (waves.Count == 0)
        {
            Debug.LogError("Нет ни одной волны в SpawnerManager!");
            return;
        }

        currentWaveIndex = 0;
        allWavesComplete = false;
        StartCoroutine(RunWaves());
    }

    /// <summary>
    /// Остановить все спавны
    /// </summary>
    public void StopWaves()
    {
        StopAllCoroutines();
        isSpawning = false;
    }

    /// <summary>
    /// Пропустить текущую волну (убить всех врагов)
    /// </summary>
    public void SkipCurrentWave()
    {
        if (!isSpawning) return;

        // Убиваем всех врагов на сцене
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }

        Debug.Log($"Волна {waves[currentWaveIndex].waveName} пропущена!");
    }

    /// <summary>
    /// Запустить конкретную волну по индексу
    /// </summary>
    public void StartWave(int waveIndex)
    {
        if (waveIndex < 0 || waveIndex >= waves.Count)
        {
            Debug.LogError($"Неверный индекс волны: {waveIndex}");
            return;
        }

        StopWaves();
        currentWaveIndex = waveIndex;
        allWavesComplete = false;
        StartCoroutine(RunWavesFromCurrent());
    }

    private IEnumerator RunWaves()
    {
        for (int i = 0; i < waves.Count; i++)
        {
            currentWaveIndex = i;
            yield return StartCoroutine(SpawnWave(waves[i]));

            if (!loopWaves && currentWaveIndex >= waves.Count - 1)
            {
                break;
            }
        }

        allWavesComplete = true;
        onAllWavesCompleted?.Invoke();
        Debug.Log("🎉 ВСЕ ВОЛНЫ ПРОЙДЕНЫ! Поздравляю! 🎉");
    }

    private IEnumerator RunWavesFromCurrent()
    {
        for (int i = currentWaveIndex; i < waves.Count; i++)
        {
            currentWaveIndex = i;
            yield return StartCoroutine(SpawnWave(waves[i]));

            if (!loopWaves && currentWaveIndex >= waves.Count - 1)
            {
                break;
            }
        }

        allWavesComplete = true;
        onAllWavesCompleted?.Invoke();
    }

    private IEnumerator SpawnWave(Wave wave)
    {
        // Ждем перед волной
        if (wave.preWaveDelay > 0)
        {
            Debug.Log($"⏰ Волна '{wave.waveName}' начнется через {wave.preWaveDelay} секунд...");
            yield return new WaitForSeconds(wave.preWaveDelay);
        }

        isSpawning = true;
        onWaveStarted?.Invoke();

        Debug.Log($"⚔️ НАЧАЛАСЬ ВОЛНА: {wave.waveName} ⚔️");
        Debug.Log($"📊 В волне {wave.enemies.Count} типов врагов");

        // Спавним каждого врага из списка
        foreach (WaveEnemy waveEnemy in wave.enemies)
        {
            if (waveEnemy.enemyPrefab == null)
            {
                Debug.LogWarning("Пропущен враг: отсутствует префаб!");
                continue;
            }

            Debug.Log($"Спавн: {waveEnemy.enemyPrefab.name} x{waveEnemy.count} (задержка {waveEnemy.spawnDelay}с)");

            for (int i = 0; i < waveEnemy.count; i++)
            {
                if (!isSpawning) yield break; // Если волну прервали

                SpawnEnemy(waveEnemy.enemyPrefab, waveEnemy.specificSpawnPoint);
                totalEnemiesSpawned++;

                // Задержка между спавнами одного типа
                if (i < waveEnemy.count - 1)
                {
                    yield return new WaitForSeconds(waveEnemy.spawnDelay + globalSpawnDelay);
                }
            }
        }

        isSpawning = false;

        // Ждем, пока все враги не будут убиты
        Debug.Log($"📢 Волна {wave.waveName} отспавнена. Ожидаем уничтожения всех врагов...");

        while (GetEnemiesCount() > 0)
        {
            yield return new WaitForSeconds(0.5f);
        }

        totalEnemiesKilled = totalEnemiesSpawned;

        Debug.Log($"✅ ВОЛНА ЗАВЕРШЕНА: {wave.waveName}");
        onWaveCompleted?.Invoke();

        // Ждем после волны
        if (wave.postWaveDelay > 0)
        {
            Debug.Log($"⏸️ Пауза {wave.postWaveDelay} секунд перед следующей волной...");
            yield return new WaitForSeconds(wave.postWaveDelay);
        }
    }

    private void SpawnEnemy(GameObject enemyPrefab, Transform specificSpawnPoint = null)
    {
        if (enemyPrefab == null) return;

        // Определяем точку спавна
        Transform spawnPoint;
        if (specificSpawnPoint != null)
        {
            spawnPoint = specificSpawnPoint;
        }
        else if (spawnPoints != null && spawnPoints.Length > 0)
        {
            // Выбираем случайную точку спавна
            spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        }
        else
        {
            Debug.LogError("Нет точек спавна! Добавь их в SpawnerManager или укажи specificSpawnPoint");
            return;
        }

        // Создаем врага
        GameObject enemyObj = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

        // Инициализируем врага с его точкой спавна (для выбора пути)
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        if (enemy != null && pathNetwork != null)
        {
            enemy.Initialize(pathNetwork, spawnPoint);
        }
        else if (enemy != null && pathNetwork == null)
        {
            Debug.LogError("PathNetwork не назначен в SpawnerManager!");
        }

        enemyObj.tag = "Enemy";
    }

    private int GetEnemiesCount()
    {
        return GameObject.FindGameObjectsWithTag("Enemy").Length;
    }

    private float GetWaveProgress()
    {
        if (waves.Count == 0) return 0;
        return (float)(currentWaveIndex + 1) / waves.Count;
    }

    /// <summary>
    /// Получить информацию о текущей волне (для UI)
    /// </summary>
    public string GetWaveInfo()
    {
        if (currentWaveIndex >= waves.Count)
            return "Все волны завершены";

        Wave current = waves[currentWaveIndex];
        return $"Волна {currentWaveIndex + 1}/{waves.Count}: {current.waveName}";
    }

    /// <summary>
    /// Получить прогресс текущей волны (0-1)
    /// </summary>
    public float GetCurrentWaveProgress()
    {
        if (!isSpawning) return 1f;

        // Можно расширить для точного подсчета отспавненных врагов
        return 0.5f;
    }

    // Визуализация в редакторе
    private void OnDrawGizmosSelected()
    {
        if (spawnPoints == null) return;

        Gizmos.color = Color.red;
        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint != null)
            {
                Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + Vector3.up * 2f);
            }
        }
    }
}