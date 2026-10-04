using System.Collections;
using UnityEngine;

public class BowWeapon : Weapon
{
    [Header("무기 설정")]
    public GameObject arrowPrefab;
    public Transform firePoint;
    public float maxDistance = 10f;

    // PierceCount 프로퍼티 추가
    public int PierceCount => (int)rarity;

    public override float AttackCooldown => attackCooldown / (1f + (int)rarity * 0.2f);

    void Start()
    {
        // firePoint가 비어있다면 씬에 있는 "Player" 태그 오브젝트를 찾아 자동 설정
        if (firePoint == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Transform holder = player.transform.Find("WeaponHolder");
                firePoint = holder != null ? holder : player.transform;
            }
        }
    }

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
            // 1. 마우스 위치를 바탕으로 발사 방향 및 각도 계산
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0f;

            Vector2 fireDirection = (mousePosition - firePoint.position).normalized;
            float angle = Mathf.Atan2(fireDirection.y, fireDirection.x) * Mathf.Rad2Deg;
            Quaternion spawnRotation = Quaternion.Euler(0, 0, angle);

            // 2. "Player" 풀 태그를 첫 번째 인수로 전달하여 화살 가져오기
            GameObject arrow = ObjectPooler.Instance.SpawnFromPool("Player", arrowPrefab, firePoint.position, spawnRotation);

            // 3. Setup에 4개 인수 전달 (데미지, 관통수, 타겟태그, 소속풀태그)
            Projectile projectile = arrow.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Setup(Damage, PierceCount, "Enemy", "Player");
            }
        }
    }

    // TripleShotRoutine 코루틴 함수
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