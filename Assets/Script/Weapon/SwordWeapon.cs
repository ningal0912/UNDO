using System.Collections;
using UnityEngine;

public class SwordWeapon : Weapon
{
    public float attackRange = 2f;
    public float attackAngle = 120f;
    public GameObject swordWavePrefab;
    private int comboCount = 0;

    public float FinalRange => attackRange * (1f + (int)rarity * 0.15f);

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

        int segments = 20;
        rangeLine.positionCount = segments + 2;

        Vector3 origin = transform.position;
        rangeLine.SetPosition(0, origin);

        float halfAngle = attackAngle / 2f;
        float startAngle = transform.eulerAngles.z - halfAngle;
        float angleStep = attackAngle / segments;

        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = (startAngle + (i * angleStep)) * Mathf.Deg2Rad;
            Vector3 point = origin + new Vector3(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle), 0f) * FinalRange;
            rangeLine.SetPosition(i + 1, point);
        }
    }

    public override void Shoot()
    {
        if (Time.time < lastAttackTime + AttackCooldown) return;
        lastAttackTime = Time.time;

        StartCoroutine(FlashWhiteEffect());

        comboCount++;
        if (rarity == WeaponRarity.Legendary && comboCount % 4 == 0)
        {
            if (swordWavePrefab != null) Instantiate(swordWavePrefab, transform.position, transform.rotation);
        }
        else
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, FinalRange, enemyLayer);
            foreach (var hit in hits)
            {
                Vector2 dirToTarget = (hit.transform.position - transform.position).normalized;
                if (Vector2.Angle(transform.right, dirToTarget) <= attackAngle / 2f)
                {
                    hit.GetComponent<IDamageable>()?.TakeDamage(Damage);
                }
            }
        }
    }

    private IEnumerator FlashWhiteEffect()
    {
        if (rangeLine != null)
        {
            rangeLine.startColor = attackColor;
            rangeLine.endColor = attackColor;
            yield return new WaitForSeconds(0.1f);
            rangeLine.startColor = normalColor;
            rangeLine.endColor = normalColor;
        }
    }
}