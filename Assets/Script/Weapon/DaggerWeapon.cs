using System.Collections;
using UnityEngine;

public class DaggerWeapon : Weapon
{
    public float attackRange = 1f;
    public LayerMask projectileLayer;

    public override float AttackCooldown => attackCooldown / (1f + (int)rarity * 0.4f);

    void Start()
    {
        if (rangeLine != null)
        {
            rangeLine.startColor = normalColor;
            rangeLine.endColor = normalColor;
            rangeLine.useWorldSpace = true;
        }
    }

    void Update()
    {
        UpdateRangeIndicator();
    }

    public override void UpdateRangeIndicator()
    {
        if (rangeLine == null) return;

        int segments = 24;
        rangeLine.positionCount = segments + 1;
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * (360f / segments) * Mathf.Deg2Rad;
            Vector3 point = transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * attackRange;
            rangeLine.SetPosition(i, point);
        }
    }

    public override void Shoot()
    {
        if (Time.time < lastAttackTime + AttackCooldown) return;
        lastAttackTime = Time.time;

        StartCoroutine(FlashWhiteEffect());

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);
        foreach (var hit in hits)
        {
            hit.GetComponent<IDamageable>()?.TakeDamage(Damage);
        }

        if (rarity == WeaponRarity.Legendary)
        {
            Collider2D[] projectiles = Physics2D.OverlapCircleAll(transform.position, attackRange, projectileLayer);
            foreach (var proj in projectiles) Destroy(proj.gameObject);
        }
    }

    private IEnumerator FlashWhiteEffect()
    {
        if (rangeLine != null)
        {
            rangeLine.startColor = attackColor;
            rangeLine.endColor = attackColor;
            yield return new WaitForSeconds(0.08f);
            rangeLine.startColor = normalColor;
            rangeLine.endColor = normalColor;
        }
    }
}