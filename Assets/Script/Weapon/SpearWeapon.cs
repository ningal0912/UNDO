using System.Collections;
using UnityEngine;

public class SpearWeapon : Weapon
{
    public float innerRadius = 1f;
    public float outerRadius = 3f;
    public float thrustLength = 4.5f;
    public float thrustWidth = 1f;

    private int attackPattern = 0;
    private float comboTimer;

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
        if (Time.time - comboTimer > 2f) attackPattern = 0;
        UpdateRangeIndicator();
    }

    public override void UpdateRangeIndicator()
    {
        if (rangeLine == null) return;

        if (attackPattern == 0 || attackPattern == 1) // 도넛 패턴
        {
            int segments = 30;
            rangeLine.positionCount = segments + 1;
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * (360f / segments) * Mathf.Deg2Rad;
                Vector3 point = transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * outerRadius;
                rangeLine.SetPosition(i, point);
            }
        }
        else if (attackPattern == 2) // 찌르기 직사각형 패턴
        {
            rangeLine.positionCount = 5;
            float currentThrust = (rarity == WeaponRarity.Legendary) ? thrustLength * 1.5f : thrustLength;

            Vector3 right = transform.right * currentThrust;
            Vector3 up = transform.up * (thrustWidth / 2f);
            Vector3 origin = transform.position;

            rangeLine.SetPosition(0, origin + up);
            rangeLine.SetPosition(1, origin + right + up);
            rangeLine.SetPosition(2, origin + right - up);
            rangeLine.SetPosition(3, origin - up);
            rangeLine.SetPosition(4, origin + up);
        }
    }

    public override void Shoot()
    {
        if (Time.time < lastAttackTime + AttackCooldown) return;

        lastAttackTime = Time.time;
        comboTimer = Time.time;

        StartCoroutine(FlashWhiteEffect());

        if (attackPattern == 0 || attackPattern == 1)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, outerRadius, enemyLayer);
            foreach (var hit in hits)
            {
                if (Vector2.Distance(transform.position, hit.transform.position) >= innerRadius)
                {
                    hit.GetComponent<IDamageable>()?.TakeDamage(Damage);
                }
            }
        }
        else if (attackPattern == 2)
        {
            float currentThrust = (rarity == WeaponRarity.Legendary) ? thrustLength * 1.5f : thrustLength;
            Vector2 boxSize = new Vector2(currentThrust, thrustWidth);
            Vector2 boxCenter = (Vector2)transform.position + (Vector2)transform.right * (currentThrust / 2f);

            Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, boxSize, transform.eulerAngles.z, enemyLayer);
            foreach (var hit in hits)
            {
                hit.GetComponent<IDamageable>()?.TakeDamage(Damage);
            }
        }

        attackPattern = (attackPattern + 1) % 3;
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