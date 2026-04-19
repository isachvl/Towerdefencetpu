// SimpleMeleeTower.cs
// Простая башня ближнего боя с триггером
using UnityEngine;

public class SimpleMeleeTower : MonoBehaviour
{
    [Header("Характеристики")]
    public int damage = 20;
    public float attackCooldown = 1f;     // Задержка между атаками
    public int upgradeCost = 100;
    public int sellPrice = 75;

    [Header("Компоненты")]
    public GameObject attackEffect;        // Эффект удара (опционально)

    private float currentCooldown = 0f;
    private Enemy currentEnemy = null;
    private int level = 1;
    private int baseDamage;

    private void Start()
    {
        baseDamage = damage;
    }

    private void Update()
    {
        // Отсчет кулдауна
        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
        }

        // Если есть враг в триггере и кулдаун прошел - атакуем
        if (currentEnemy != null && currentCooldown <= 0)
        {
            Attack();
            currentCooldown = attackCooldown;
        }
    }

    private void Attack()
    {
        if (currentEnemy == null) return;

        // Наносим урон
        currentEnemy.TakeDamage(damage);

        // Эффект атаки
        if (attackEffect != null)
        {
            GameObject effect = Instantiate(attackEffect, transform.position, Quaternion.identity);
            Destroy(effect, 0.5f);
        }

        Debug.Log($"Башня атакует! Урон: {damage}");
    }

    // Вход врага в триггер
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Если врага еще нет - запоминаем
            if (currentEnemy == null)
            {
                currentEnemy = other.GetComponent<Enemy>();
            }
        }
    }

    // Выход врага из триггера
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy") && other.GetComponent<Enemy>() == currentEnemy)
        {
            currentEnemy = null;
        }
    }

    // Улучшение башни
    public void Upgrade()
    {
        level++;
        damage = Mathf.RoundToInt(baseDamage * (1 + level * 0.5f));
        attackCooldown *= 0.9f; // Уменьшаем задержку на 10%

        // Визуальное изменение
        transform.localScale += Vector3.one * 0.2f;

        Debug.Log($"Башня улучшена до {level} уровня! Урон: {damage}");
    }

    // Продажа башни
    public int Sell()
    {
        Debug.Log($"Башня продана за {sellPrice}");
        Destroy(gameObject);
        return sellPrice;
    }

    public int GetUpgradeCost() => upgradeCost * level;
    public int GetSellPrice() => sellPrice;
    public int GetLevel() => level;
    public int GetDamage() => damage;
}

// ============ УПРОЩЕННЫЙ СЛОТ ДЛЯ БАШНИ ============

public class SimpleTowerSlot : MonoBehaviour
{
    [Header("Настройки")]
    public GameObject towerPrefab;          // Префаб башни
    public int towerCost = 100;

    [Header("Визуализация")]
    public Material defaultMaterial;
    public Material highlightMaterial;
    public Material occupiedMaterial;

    private bool isOccupied = false;
    private GameObject currentTower;
    private Renderer slotRenderer;
    private Material originalMaterial;

    public bool IsOccupied => isOccupied;

    private void Start()
    {
        slotRenderer = GetComponent<Renderer>();
        if (slotRenderer != null)
        {
            originalMaterial = slotRenderer.material;
        }
    }

    private void OnMouseEnter()
    {
        if (!isOccupied && slotRenderer != null)
        {
            slotRenderer.material = highlightMaterial ?? defaultMaterial;
        }
    }

    private void OnMouseExit()
    {
        if (!isOccupied && slotRenderer != null)
        {
            slotRenderer.material = originalMaterial;
        }
    }

    private void OnMouseDown()
    {
        if (isOccupied)
        {
            // Если башня уже есть - показываем меню
            ShowTowerMenu();
        }
        else
        {
            // Пытаемся построить башню
            TryBuildTower();
        }
    }

    private void TryBuildTower()
    {
        // Проверяем деньги
        if (GameManager.Instance != null && GameManager.Instance.money < towerCost)
        {
            Debug.Log($"Недостаточно денег! Нужно {towerCost}");
            return;
        }

        // Строим башню
        BuildTower();

        // Снимаем деньги
        GameManager.Instance?.SpendMoney(towerCost);
    }

    public void BuildTower()
    {
        if (towerPrefab == null)
        {
            Debug.LogError("Нет префаба башни!");
            return;
        }

        currentTower = Instantiate(towerPrefab, transform.position, Quaternion.identity);
        isOccupied = true;

        // Меняем материал
        if (slotRenderer != null && occupiedMaterial != null)
        {
            slotRenderer.material = occupiedMaterial;
        }

        Debug.Log($"Башня построена на {gameObject.name}");
    }

    private void ShowTowerMenu()
    {
        // Простое управление через консоль
        Debug.Log($"Башня на {gameObject.name}: Нажмите U для улучшения, S для продажи");

        // Или вызываем события для GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ShowTowerActions(this);
        }
    }

    public void UpgradeTower()
    {
        if (!isOccupied) return;

        SimpleMeleeTower tower = currentTower.GetComponent<SimpleMeleeTower>();
        if (tower == null) return;

        int cost = tower.GetUpgradeCost();

        if (GameManager.Instance != null && GameManager.Instance.money < cost)
        {
            Debug.Log($"Недостаточно денег для улучшения! Нужно {cost}");
            return;
        }

        tower.Upgrade();
        GameManager.Instance?.SpendMoney(cost);
    }

    public void SellTower()
    {
        if (!isOccupied) return;

        SimpleMeleeTower tower = currentTower.GetComponent<SimpleMeleeTower>();
        int sellPrice = tower != null ? tower.Sell() : 50;

        GameManager.Instance?.AddMoney(sellPrice);
        isOccupied = false;
        currentTower = null;

        // Возвращаем материал
        if (slotRenderer != null)
        {
            slotRenderer.material = originalMaterial;
        }
    }

    public SimpleMeleeTower GetTower()
    {
        return currentTower?.GetComponent<SimpleMeleeTower>();
    }
}
