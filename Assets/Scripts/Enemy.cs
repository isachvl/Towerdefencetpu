// Enemy.cs (расширенная версия)
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Характеристики")]
    public float speed = 2f;
    public int health = 1;
    public int reward = 50;

    private WaypointNetwork network;
    private WaypointNetwork.PathData currentPath;
    private int currentWaypointIndex = 0;
    private Vector3 targetPosition;
    private bool isMoving = true;

    [Header("Визуализация")]
    public GameObject deathEffect;

    // Новый метод инициализации с указанием точки спавна
    public void Initialize(WaypointNetwork pathNetwork, Transform spawnPoint)
    {
        network = pathNetwork;
        currentPath = network.GetPathForSpawn(spawnPoint);

        if (currentPath != null && currentPath.waypoints.Count > 0)
        {
            transform.position = spawnPoint.position;
            currentWaypointIndex = 0;
            SetNextTarget();
        }
        else
        {
            Debug.LogError($"Не удалось инициализировать врага: нет пути для {spawnPoint.name}");
            Destroy(gameObject);
        }
    }

    // Перегрузка для совместимости со старым кодом (один путь)
    public void Initialize(WaypointNetwork pathNetwork)
    {
        if (pathNetwork.paths.Count > 0)
        {
            var firstPath = pathNetwork.paths[0];
            Initialize(pathNetwork, firstPath.spawnPoint);
        }
    }

    private void Update()
    {
        if (!isMoving || currentPath == null) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            int nextIndex = network.GetNextWaypointIndex(currentPath, currentWaypointIndex);
            if (nextIndex != -1)
            {
                currentWaypointIndex = nextIndex;
                SetNextTarget();
            }
            else
            {
                ReachEnd();
            }
        }
    }

    private void SetNextTarget()
    {
        targetPosition = network.GetWaypointPosition(currentPath, currentWaypointIndex);

        // Поворачиваем врага в направлении движения
        if (transform.position != targetPosition)
        {
            Vector3 dir = (targetPosition - transform.position).normalized;
            transform.forward = dir;
        }
    }

    private void ReachEnd()
    {
        isMoving = false;
        GameManager.Instance?.TakeDamage(10);

        // Эффект достижения цели
        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }

        // Визуальный фидбек (мигание)
        StartCoroutine(DamageFlash());
    }

    private System.Collections.IEnumerator DamageFlash()
    {
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Color original = renderer.material.color;
            renderer.material.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            renderer.material.color = original;
        }
    }

    private void Die()
    {
        GameManager.Instance?.AddMoney(reward);

        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}