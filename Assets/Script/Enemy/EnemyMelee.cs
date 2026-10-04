using UnityEngine;

public class EnemyMelee : MonoBehaviour
{
    [Header("적 스탯")]
    public float speed = 3f;
    public float damage = 10f;
    public float attackRange = 0.2f; // 💡 꼭짓점 표면에서부터의 공격 허용 거리 (0.1f~0.3f 추천)
    public float attackCooldown = 1f;

    private Transform player;
    private Rigidbody2D rb;
    private Collider2D myCollider;       // 적 콜라이더
    private Collider2D playerCollider;   // 플레이어 콜라이더
    private float lastAttackTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<Collider2D>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
            playerCollider = p.GetComponent<Collider2D>();
        }
    }

    void Update()
    {
        if (player == null) return;

        // 플레이어 바라보기 (회전으로 인해 삼각형 꼭짓점이 플레이어를 향함)
        Vector2 dir = player.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // 💡 표면 간 실제 거리 계산 (겹쳤다면 0f)
        float surfaceDistance = GetSurfaceDistance();

        // 정삼각형 꼭짓점이 공격 범위 내에 들어오면 데미지 부여
        if (surfaceDistance <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            player.GetComponent<IDamageable>()?.TakeDamage(damage);
        }
    }

    void FixedUpdate()
    {
        if (player == null) return;

        float surfaceDistance = GetSurfaceDistance();

        // 표면이 아직 안 닿았을 때만 이동
        if (surfaceDistance > 0.05f)
        {
            Vector2 targetPos = Vector2.MoveTowards(rb.position, player.position, speed * Time.fixedDeltaTime);
            rb.MovePosition(targetPos);
        }
        else
        {
            // 삼각형 꼭짓점이 플레이어 표면에 밀착하면 물리 속도 0 고정
            rb.linearVelocity = Vector2.zero; // (구버전 Unity: rb.velocity = Vector2.zero;)
        }
    }

    // 두 콜라이더의 가장 가까운 표면 간 거리를 반환하는 함수
    private float GetSurfaceDistance()
    {
        if (myCollider != null && playerCollider != null)
        {
            ColliderDistance2D colDist = myCollider.Distance(playerCollider);
            return colDist.isOverlapped ? 0f : colDist.distance;
        }

        // 콜라이더가 없을 경우 예외 처리 (중심점 거리 계산)
        return Vector2.Distance(transform.position, player.position);
    }
}