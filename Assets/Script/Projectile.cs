using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 15f; // 화살 날아가는 속도
    public float lifetime = 3f;
    private float damage;
    private int pierceCount = 0;
    private string targetTag;

    public void Setup(float damage, int pierce, string targetTag)
    {
        this.damage = damage;
        this.pierceCount = pierce;
        this.targetTag = targetTag;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // 오른쪽(+X) 방향으로 직선 이동
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(targetTag))
        {
            IDamageable damageable = collision.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }

            if (pierceCount <= 0)
            {
                Destroy(gameObject);
            }
            else
            {
                pierceCount--;
            }
        }
    }
}