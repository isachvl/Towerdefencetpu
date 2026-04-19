using UnityEngine;

public class TimedProjectile : MonoBehaviour
{
    private const string MonsterTag = "monster";
    private const string EnemyTag = "Enemy";

    private Transform target;
    private Enemy targetEnemy;
    private Collider targetCollider;
    private float speed;
    private float lifetime;
    private int damage;
    private float spawnTime;
    private bool hasHitTarget;
    private float hitDistance = 0.35f;

    public void Initialize(Transform targetTransform, float projectileSpeed, float projectileLifetime, int projectileDamage)
    {
        target = targetTransform;
        targetEnemy = targetTransform != null ? targetTransform.GetComponent<Enemy>() : null;
        targetCollider = targetTransform != null ? targetTransform.GetComponent<Collider>() : null;
        speed = projectileSpeed;
        lifetime = projectileLifetime;
        damage = projectileDamage;
        spawnTime = Time.time;

        EnsureTriggerSetup();
    }

    private void Update()
    {
        if (Time.time - spawnTime >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 targetPoint = GetTargetPoint();
        transform.position = Vector3.MoveTowards(transform.position, targetPoint, speed * Time.deltaTime);
        transform.LookAt(targetPoint);

        if (!hasHitTarget && Vector3.Distance(transform.position, targetPoint) <= hitDistance)
        {
            ApplyDamage();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(MonsterTag) && !other.CompareTag(EnemyTag))
        {
            return;
        }

        targetEnemy = FindEnemyComponent(other.transform);
        if (targetEnemy != null)
        {
            ApplyDamage();
        }
    }

    private Vector3 GetTargetPoint()
    {
        if (targetCollider != null)
        {
            return targetCollider.bounds.center;
        }

        return target.position + Vector3.up;
    }

    private void ApplyDamage()
    {
        if (hasHitTarget)
        {
            return;
        }

        hasHitTarget = true;

        if (targetEnemy == null && target != null)
        {
            targetEnemy = FindEnemyComponent(target);
        }

        if (targetEnemy != null)
        {
            targetEnemy.TakeDamage(damage);
        }

        Destroy(gameObject);
    }

    private void EnsureTriggerSetup()
    {
        Collider projectileCollider = GetComponent<Collider>();
        if (projectileCollider != null)
        {
            projectileCollider.isTrigger = true;
        }

        Rigidbody projectileBody = GetComponent<Rigidbody>();
        if (projectileBody == null)
        {
            projectileBody = gameObject.AddComponent<Rigidbody>();
        }

        projectileBody.isKinematic = true;
        projectileBody.useGravity = false;
    }

    private Enemy FindEnemyComponent(Transform source)
    {
        if (source == null)
        {
            return null;
        }

        Enemy enemy = source.GetComponent<Enemy>();
        if (enemy != null)
        {
            return enemy;
        }

        enemy = source.GetComponentInParent<Enemy>();
        if (enemy != null)
        {
            return enemy;
        }

        return source.GetComponentInChildren<Enemy>();
    }
}
