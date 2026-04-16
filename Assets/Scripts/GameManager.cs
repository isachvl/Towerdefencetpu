// GameManager.cs
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public SpawnManager spawnerManager;

    [Header("Ресурсы")]
    public int money = 500;
    public int baseHealth = 100;

    [Header("UI")]
    public Text moneyText;
    public Text healthText;

    private float spawnTimer;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        UpdateUI(); 
        spawnerManager.onWaveStarted.AddListener(OnWaveStart);
        spawnerManager.onWaveCompleted.AddListener(OnWaveEnd);
        spawnerManager.onAllWavesCompleted.AddListener(OnGameWin);
    }

    void OnWaveStart()
    {
        // Показать UI уведомление
        //waveNotification.text = spawnerManager.GetWaveInfo();
    }

    void OnWaveEnd()
    {
    }

    void OnGameWin()
    {
        // Показать экран победы
        //winScreen.SetActive(true);
    }

    private void Update()
    {
    }

    public void AddMoney(int amount)
    {
        money += amount;
        UpdateUI();
    }

    public void SpendMoney(int amount)
    {
        if (money >= amount)
        {
            money -= amount;
            UpdateUI();
        }
    }

    public void TakeDamage(int damage)
    {
        baseHealth -= damage;
        UpdateUI();

        if (baseHealth <= 0)
        {
            Debug.Log("Game Over!");
            // Тут можно загрузить экран поражения
        }
    }

    private void UpdateUI()
    {
        if (moneyText != null)
            moneyText.text = $"Деньги: {money}";
        if (healthText != null)
            healthText.text = $"Здоровье: {baseHealth}";
    }
}