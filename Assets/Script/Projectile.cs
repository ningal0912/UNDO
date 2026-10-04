using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 15f;
    public float lifetime = 3f;

    private float damage;
    private int pierceCount = 0;
    private string targetTag;
    private string poolTag; // 👈 어떤 풀 소속인지 저장할 변수
    private float currentTimer;

    // Setup 함수에 poolTag를 추가로 받습니다.
    public void Setup(float damage, int pierce, string targetTag, string poolTag)
    {
        this.damage = damage;
        this.pierceCount = pierce;
        this.targetTag = targetTag;
        this.poolTag = poolTag;
        this.currentTimer = lifetime;
    }

    void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);

        currentTimer -= Time.deltaTime;
        if (currentTimer <= 0)
        {
            Despawn();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 내 타겟 태그와 부딪혔을 때만 데미지 부여
        if (collision.CompareTag(targetTag))
        {
            IDamageable damageable = collision.GetComponent<IDamageable>();
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
    }

    private void Despawn()
    {
        if (ObjectPooler.Instance != null)
        {
            // 자신의 소속 풀 태그를 함께 넘겨서 반납
            ObjectPooler.Instance.ReturnToPool(poolTag, gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}