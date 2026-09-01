using System.Collections; // 필수 추가
using UnityEngine;

public class BowWeapon : Weapon
{
    public GameObject arrowPrefab;
    public Transform firePoint;
    public float maxDistance = 10f;

    // PierceCount 프로퍼티 추가
    public int PierceCount => (int)rarity;

    public override float AttackCooldown => attackCooldown / (1f + (int)rarity * 0.2f);

    public override void Shoot()
    {
        if (Time.time < lastAttackTime + AttackCooldown) return;
        lastAttackTime = Time.time;

        if (rarity == WeaponRarity.Legendary)
        {
            StartCoroutine(TripleShotRoutine());
        }
        else
        {
            FireArrow();
        }
    }

    public override void UpdateRangeIndicator()
    {
        // 활은 범위 표시 기능 제외
        if (rangeLine != null) rangeLine.enabled = false;
    }

    private void FireArrow()
    {
        if (arrowPrefab != null && firePoint != null)
        {
            GameObject arrow = Instantiate(arrowPrefab, firePoint.position, firePoint.rotation);
            arrow.GetComponent<Projectile>()?.Setup(Damage, PierceCount, "Enemy");
        }
    }

    // TripleShotRoutine 코루틴 함수 추가
    private IEnumerator TripleShotRoutine()
    {
        for (int i = 0; i < 3; i++)
        {
            FireArrow();
            yield return new WaitForSeconds(0.08f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Transform startPt = firePoint != null ? firePoint : transform;
        Gizmos.DrawRay(startPt.position, startPt.right * maxDistance);
        Gizmos.DrawWireSphere(startPt.position + startPt.right * maxDistance, 0.2f);
    }
}