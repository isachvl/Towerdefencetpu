// GameManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public SpawnManager spawnerManager;

    [Header("Ресурсы")]
    public int money = 500;
    public int baseHealth = 100;

    [Header("UI")]
    public TMP_Text moneyText;
    public TMP_Text healthText;

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

    public void RequestBuildTower(SimpleTowerSlot slot, int cost)
    {
        if (money >= cost)
        {
            SpendMoney(cost);
            slot.BuildTower();
        }
        else
        {
            Debug.Log($"Недостаточно денег для постройки! Нужно {cost}, есть {money}");
        }
    }

    public void RequestUpgradeTower(SimpleTowerSlot slot, SimpleMeleeTower tower, int cost)
    {
        if (money >= cost)
        {
            SpendMoney(cost);
            tower.Upgrade();
        }
        else
        {
            Debug.Log($"Недостаточно денег для улучшения! Нужно {cost}, есть {money}");
        }
    }

    private SimpleTowerSlot selectedSlotForMenu;

    public void ShowTowerActions(SimpleTowerSlot slot)
    {
        selectedSlotForMenu = slot;
        Debug.Log("Нажмите U для улучшения, S для продажи");
    }

    private void Update()
    {
        if (selectedSlotForMenu != null)
        {
            if (Input.GetKeyDown(KeyCode.U))
            {
                selectedSlotForMenu.UpgradeTower();
                selectedSlotForMenu = null;
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                selectedSlotForMenu.SellTower();
                selectedSlotForMenu = null;
            }
        }
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