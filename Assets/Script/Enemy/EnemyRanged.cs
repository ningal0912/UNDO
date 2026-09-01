using UnityEngine;

public class EnemyRanged : MonoBehaviour
{
    public float speed = 2.5f;
    public float keepDistance = 5f;
    public float attackCooldown = 2f;
    public GameObject projectilePrefab;
    public Transform firePoint;

    private Transform player;
    private float lastAttackTime;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        // 플레이어 바라보기
        Vector2 dir = player.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        if (dist > keepDistance)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
        }

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            proj.GetComponent<Projectile>()?.Setup(10f, 0, "Player");
        }
    }
}