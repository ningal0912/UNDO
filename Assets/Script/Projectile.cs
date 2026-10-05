using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("기본 탄환 설정")]
    public float speed = 15f;
    public float lifetime = 3f;

    private float baseSpeed;

    private float damage;
    private int pierceCount = 0;
    private string targetTag;
    private string poolTag;
    private float currentTimer;


    private void Awake()
    {
        baseSpeed = speed;
    }


    public void Setup(
        float damage,
        int pierce,
        string targetTag,
        string poolTag)
    {
        this.damage = damage;
        this.pierceCount = pierce;
        this.targetTag = targetTag;
        this.poolTag = poolTag;

        // 풀에서 다시 꺼냈을 때 기본 속도로 초기화
        speed = baseSpeed;

        currentTimer = lifetime;
    }


    private void Update()
    {
        // 탄환은 자신의 로컬 X축 방향으로 이동
        transform.Translate(
            Vector3.right * speed * Time.deltaTime,
            Space.Self
        );

        currentTimer -= Time.deltaTime;

        if (currentTimer <= 0f)
        {
            Despawn();
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag(targetTag))
            return;

        IDamageable damageable =
            collision.GetComponentInParent<IDamageable>();

        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }

        if (pierceCount <= 0)
        {
            Despawn();
        }
        else
        {
            pierceCount--;
        }
    }


    private void Despawn()
    {
        if (ObjectPooler.Instance != null)
        {
            ObjectPooler.Instance.ReturnToPool(
                poolTag,
                gameObject
            );
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
