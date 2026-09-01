using UnityEngine;

public class EnemyMelee : MonoBehaviour
{
    public float speed = 3f;
    public float damage = 10f;
    public float attackRange = 1f;
    public float attackCooldown = 1f;

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

        if (dist > attackRange)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
        }
        else if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            player.GetComponent<IDamageable>()?.TakeDamage(damage);
        }
    }
}