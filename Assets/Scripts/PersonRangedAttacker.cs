using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PersonRangedAttacker : MonoBehaviour
{
    private const string RadiusTag = "rad";
    private const string TimerTag = "timer";
    private const string MonsterTag = "monster";
    private const string EnemyTag = "Enemy";

    [Header("Attack")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private Vector3 projectileSpawnOffset = new Vector3(1f, 0f, 0f);
    [SerializeField] private float attackInterval = 1f;
    [SerializeField] private float projectileLifetime = 2f;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private int projectileDamage = 10;

    [Header("Timer")]
    [SerializeField] private float timerDuration = 60f;

    private readonly List<Transform> monstersInRange = new List<Transform>();

    private TMP_Text cachedTimerText;
    private float attackTimer;
    private float timerRemaining;
    private TriggerForwarder radiusForwarder;
    private bool timerStarted;

    private void Awake()
    {
        SetupRadiusTrigger();
    }

    private void OnEnable()
    {
        attackTimer = 0f;
        timerStarted = false;
        timerRemaining = timerDuration;
        UpdateTimerText(0f);
    }

    private void Update()
    {
        if (timerStarted)
        {
            timerRemaining -= Time.deltaTime;

            if (timerRemaining < 0f)
            {
                timerRemaining = 0f;
                timerStarted = false;
            }

            UpdateTimerText(timerRemaining);
        }

        CleanupTargets();

        if (projectilePrefab == null)
        {
            return;
        }

        attackTimer -= Time.deltaTime;

        if (attackTimer > 0f)
        {
            return;
        }

        Transform target = GetCurrentTarget();
        if (target == null)
        {
            return;
        }

        FireProjectile(target);
        attackTimer = attackInterval;
    }

    private void SetupRadiusTrigger()
    {
        Transform radiusChild = FindChildWithTag(transform, RadiusTag);
        if (radiusChild == null)
        {
            Debug.LogWarning($"PersonRangedAttacker on {name}: child with tag '{RadiusTag}' was not found.");
            return;
        }

        Collider radiusCollider = radiusChild.GetComponent<Collider>();
        if (radiusCollider == null)
        {
            Debug.LogWarning($"PersonRangedAttacker on {name}: radius child needs a Collider.");
            return;
        }

        radiusCollider.isTrigger = true;

        radiusForwarder = radiusChild.GetComponent<TriggerForwarder>();
        if (radiusForwarder == null)
        {
            radiusForwarder = radiusChild.gameObject.AddComponent<TriggerForwarder>();
        }

        radiusForwarder.Initialize(this);
    }

    private void FireProjectile(Transform target)
    {
        Vector3 spawnPosition = projectileSpawnPoint != null
            ? projectileSpawnPoint.position
            : transform.position + projectileSpawnOffset;
        GameObject projectileObject = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        TimedProjectile projectile = projectileObject.GetComponent<TimedProjectile>();

        if (projectile == null)
        {
            projectile = projectileObject.AddComponent<TimedProjectile>();
        }

        projectile.Initialize(target, projectileSpeed, projectileLifetime, projectileDamage);
    }

    private Transform GetCurrentTarget()
    {
        CleanupTargets();

        if (monstersInRange.Count == 0)
        {
            return null;
        }

        return monstersInRange[0];
    }

    private void CleanupTargets()
    {
        for (int i = monstersInRange.Count - 1; i >= 0; i--)
        {
            if (monstersInRange[i] == null)
            {
                monstersInRange.RemoveAt(i);
            }
        }
    }

    private void UpdateTimerText(float timeValue)
    {
        if (cachedTimerText == null)
        {
            return;
        }

        int seconds = Mathf.FloorToInt(timeValue);
        int centiseconds = Mathf.FloorToInt((timeValue - seconds) * 100f);
        cachedTimerText.text = $"{seconds:00}:{centiseconds:00}";
    }

    public void StartPlacementTimer()
    {
        cachedTimerText = FindTimerText();

        if (cachedTimerText == null)
        {
            Debug.LogWarning($"PersonRangedAttacker on {name}: timer with tag '{TimerTag}' was not found near the placed object.");
            return;
        }

        timerRemaining = timerDuration;
        timerStarted = true;
        UpdateTimerText(timerRemaining);
    }

    private static Transform FindChildWithTag(Transform root, string tagName)
    {
        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);

            if (child.CompareTag(tagName))
            {
                return child;
            }

            Transform nestedChild = FindChildWithTag(child, tagName);
            if (nestedChild != null)
            {
                return nestedChild;
            }
        }

        return null;
    }

    private TMP_Text FindTimerText()
    {
        if (transform.parent != null)
        {
            Transform timerInParent = FindChildWithTag(transform.parent, TimerTag);
            if (timerInParent != null)
            {
                return timerInParent.GetComponent<TMP_Text>();
            }
        }

        Transform timerInSelf = FindChildWithTag(transform, TimerTag);
        if (timerInSelf != null)
        {
            return timerInSelf.GetComponent<TMP_Text>();
        }

        GameObject timerObject = GameObject.FindGameObjectWithTag(TimerTag);
        if (timerObject != null)
        {
            return timerObject.GetComponent<TMP_Text>();
        }

        return null;
    }

    internal void HandleRadiusEnter(Collider other)
    {
        if (!IsMonsterTarget(other))
        {
            return;
        }

        if (!monstersInRange.Contains(other.transform))
        {
            monstersInRange.Add(other.transform);
        }
    }

    internal void HandleRadiusExit(Collider other)
    {
        if (!IsMonsterTarget(other))
        {
            return;
        }

        monstersInRange.Remove(other.transform);
    }

    private bool IsMonsterTarget(Collider other)
    {
        return other.CompareTag(MonsterTag) || other.CompareTag(EnemyTag);
    }

    private sealed class TriggerForwarder : MonoBehaviour
    {
        private PersonRangedAttacker owner;

        internal void Initialize(PersonRangedAttacker attackerOwner)
        {
            owner = attackerOwner;
        }

        private void OnTriggerEnter(Collider other)
        {
            owner?.HandleRadiusEnter(other);
        }

        private void OnTriggerExit(Collider other)
        {
            owner?.HandleRadiusExit(other);
        }
    }
}
