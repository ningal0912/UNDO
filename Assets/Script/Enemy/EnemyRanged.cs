using UnityEngine;

public class EnemyRanged : MonoBehaviour
{
    [Header("적 스탯 및 설정")]
    public float speed = 2.5f;
    public float keepDistance = 5f;
    public float attackCooldown = 2f;
    public float damage = 10f;
    public GameObject projectilePrefab;
    public Transform firePoint;

    private Transform player;
    private Rigidbody2D rb;
    private float lastAttackTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        if (firePoint == null)
        {
            firePoint = transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        // 플레이어 바라보기 (회전)
        Vector2 dir = player.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // 공격 로직 (타이머)
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            Attack();
        }
    }

    void FixedUpdate()
    {
        if (player == null) return;

        float dist = Vector2.Distance(rb.position, player.position);

        // 일정 거리보다 멀 때만 플레이어를 향해 물리 이동
        if (dist > keepDistance)
        {
            Vector2 targetPos = Vector2.MoveTowards(rb.position, player.position, speed * Time.fixedDeltaTime);
            rb.MovePosition(targetPos);
        }
        else
        {
            // 💡 [핵심 수정] 일정 거리 이내로 들어오면 물리 속도를 0으로 만들어 밀림/슬라이딩 현상 방지
            rb.linearVelocity = Vector2.zero; // (Unity 2023 이상 / 구버전은 rb.velocity = Vector2.zero;)
        }
    }

    private void Attack()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning($"{gameObject.name}: projectilePrefab이 지정되지 않았습니다.");
            return;
        }

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Quaternion spawnRot = firePoint != null ? firePoint.rotation : transform.rotation;

        GameObject proj = ObjectPooler.Instance.SpawnFromPool("Enemy", projectilePrefab, spawnPos, spawnRot);

        Projectile projectileScript = proj.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.Setup(damage, 0, "Player", "Enemy");
        }
    }
}