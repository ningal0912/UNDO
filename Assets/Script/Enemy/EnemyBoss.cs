using System.Collections;
using UnityEngine;

public class EnemyBoss : MonoBehaviour
{
    [Header("보스 설정")]
    public GameObject projectilePrefab;
    public GameObject warningAOEPrefab; // 장판 프리팹
    public Transform firePoint;

    [Header("모드별 패턴 주기 및 스탯")]
    public float normalPatternInterval = 5f; // 일반 모드 패턴 간격
    public float ragePatternInterval = 3f;   // 분노 모드 패턴 간격

    [Header("일반 모드 스탯")]
    public float normalProjDamage = 15f;
    public float normalAoeDamage = 25f;

    [Header("분노 모드 (2페이즈, 체력 30% 이하) 스탯")]
    public float rageProjDamage = 20f;
    public float rageAoeDamage = 35f;
    public float rageProjSpeedMultiplier = 1.4f; // 투사체 속도 빨라짐

    private Transform player;
    private EntityHealth entityHealth;
    private Rigidbody2D rb;
    private bool isRageMode = false;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        entityHealth = GetComponent<EntityHealth>();
        rb = GetComponent<Rigidbody2D>();

        if (firePoint == null) firePoint = transform;

        StartCoroutine(BossPatternRoutine());
    }

    void Update()
    {
        // 체력이 30% 이하가 되면 분노 모드(2페이즈) 돌입
        if (!isRageMode && entityHealth != null)
        {
            if (entityHealth.currentHealth <= entityHealth.maxHealth * 0.3f)
            {
                isRageMode = true;
                Debug.Log("<color=red>🔥 보스 분노 모드(2페이즈) 돌입! 패턴 간격 3초로 단축</color>");
            }
        }
    }

    private IEnumerator BossPatternRoutine()
    {
        yield return new WaitForSeconds(2f);

        while (entityHealth != null && entityHealth.currentHealth > 0)
        {
            float currentInterval = isRageMode ? ragePatternInterval : normalPatternInterval;
            yield return new WaitForSeconds(currentInterval);

            // 0, 1, 2 패턴 중 무작위 선택
            int pattern = Random.Range(0, 3);

            if (pattern == 0)
            {
                yield return StartCoroutine(Pattern1_ShootArrowsAtPlayer());
            }
            else if (pattern == 1)
            {
                yield return StartCoroutine(Pattern2_ClockwiseSectorShoot());
            }
            else
            {
                yield return StartCoroutine(Pattern3_DashAndAOE());
            }
        }
    }

    // -------------------------------------------------------------------------
    // [패턴 1] 플레이어 방향으로 화살 연사 (1페이즈: 5발 / 2페이즈: 7발)
    // -------------------------------------------------------------------------
    private IEnumerator Pattern1_ShootArrowsAtPlayer()
    {
        int count = isRageMode ? 7 : 5;
        float interval = isRageMode ? 0.3f : 0.5f;
        float damage = isRageMode ? rageProjDamage : normalProjDamage;

        if (ObjectPooler.Instance == null)
        {
            Debug.LogError("[EnemyBoss] ObjectPooler.Instance가 없습니다.");
            yield break;
        }

        if (projectilePrefab == null)
        {
            Debug.LogError("[EnemyBoss] projectilePrefab이 지정되지 않았습니다.");
            yield break;
        }

        for (int i = 0; i < count; i++)
        {
            if (player == null)
            {
                Debug.LogError("[EnemyBoss] Player를 찾을 수 없습니다.");
                yield break;
            }

            Vector3 spawnPos = firePoint != null
                ? firePoint.position
                : transform.position;

            Vector2 dir = (player.position - spawnPos).normalized;

            // 원형 투사체이므로 -90도 보정 없음
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            Quaternion rot = Quaternion.Euler(0, 0, angle);

            GameObject proj = ObjectPooler.Instance.SpawnFromPool(
                "Enemy",
                projectilePrefab,
                spawnPos,
                rot
            );

            if (proj == null)
            {
                Debug.LogError("[EnemyBoss] 투사체 생성 실패");
                yield break;
            }

            Projectile projScript = proj.GetComponent<Projectile>();

            if (projScript == null)
            {
                Debug.LogError("[EnemyBoss] 생성된 투사체에 Projectile 컴포넌트가 없습니다.");
                yield break;
            }

            projScript.Setup(
                damage,
                0,
                "Player",
                "Enemy"
            );

            if (isRageMode)
            {
                projScript.speed *= rageProjSpeedMultiplier;
            }

            yield return new WaitForSeconds(interval);
        }
    }

    // -------------------------------------------------------------------------
    // [패턴 2] 보스의 정중앙(firePoint)에서 +모양으로 시작해 시계방향으로 90도 회전하며 발사
    // -------------------------------------------------------------------------
    private IEnumerator Pattern2_ClockwiseSectorShoot()
    {
        float interval = isRageMode ? 0.2f : 0.25f;
        float damage = isRageMode ? rageProjDamage : normalProjDamage;

        if (ObjectPooler.Instance == null)
        {
            Debug.LogError("[EnemyBoss] ObjectPooler.Instance가 없습니다.");
            yield break;
        }

        if (projectilePrefab == null)
        {
            Debug.LogError("[EnemyBoss] projectilePrefab이 지정되지 않았습니다.");
            yield break;
        }

        float baseStartAngle = Random.Range(0, 4) * 90f;

        int totalSteps = 5;

        for (int i = 0; i < totalSteps; i++)
        {
            Vector3 spawnPos = firePoint != null
                ? firePoint.position
                : transform.position;

            float currentAngle =
                baseStartAngle - (i * (90f / (totalSteps - 1)));

            for (int arm = 0; arm < 4; arm++)
            {
                float armAngle = currentAngle + (arm * 90f);

                Quaternion rot =
                    Quaternion.Euler(0, 0, armAngle);

                GameObject proj = ObjectPooler.Instance.SpawnFromPool(
                    "Enemy",
                    projectilePrefab,
                    spawnPos,
                    rot
                );

                if (proj == null)
                {
                    Debug.LogError("[EnemyBoss] 패턴2 투사체 생성 실패");
                    continue;
                }

                Projectile projScript = proj.GetComponent<Projectile>();

                if (projScript == null)
                {
                    Debug.LogError(
                        "[EnemyBoss] 투사체에 Projectile 컴포넌트가 없습니다."
                    );
                    continue;
                }

                projScript.Setup(
                    damage,
                    0,
                    "Player",
                    "Enemy"
                );

                if (isRageMode)
                {
                    projScript.speed *= rageProjSpeedMultiplier;
                }
            }

            yield return new WaitForSeconds(interval);
        }
    }

// -------------------------------------------------------------------------
// [패턴 3] 플레이어 방향 돌진
// 1페이즈 : 1회 돌진
// 2페이즈 : 2회 연속 돌진
//
// 돌진 전 : 빨간색 경고 영역 표시
// 돌진 후 : 해당 위치에 빨간 장판 생성
// 장판 : 3초 동안 유지
// -------------------------------------------------------------------------
    private IEnumerator Pattern3_DashAndAOE()
    {
        int dashCount = isRageMode ? 2 : 1;

        float aoeDamage = isRageMode
            ? rageAoeDamage
            : normalAoeDamage;

        // ObjectPooler 확인
        if (ObjectPooler.Instance == null)
        {
            Debug.LogError("[Boss] ObjectPooler.Instance가 없습니다.");
            yield break;
        }

        // AOE 프리팹 확인
        if (warningAOEPrefab == null)
        {
            Debug.LogError("[Boss] warningAOEPrefab이 지정되지 않았습니다.");
            yield break;
        }

        for (int d = 0; d < dashCount; d++)
        {
            if (player == null)
            {
                Debug.LogError("[Boss] Player를 찾을 수 없습니다.");
                yield break;
            }

            // ---------------------------------------------------------
            // 1. 현재 플레이어 위치를 목표 위치로 저장
            // ---------------------------------------------------------
            Vector3 targetPos = player.position;

            // ---------------------------------------------------------
            // 2. 돌진 전 빨간색 경고 영역 생성
            // ---------------------------------------------------------
            GameObject warningAOE =
                ObjectPooler.Instance.SpawnFromPool(
                    "BossAOE",
                    warningAOEPrefab,
                    targetPos,
                    Quaternion.identity
                );

            if (warningAOE == null)
            {
                Debug.LogError("[Boss] 경고 AOE 생성 실패");
                yield break;
            }

            // 경고 시간
            yield return new WaitForSeconds(0.8f);

            // ---------------------------------------------------------
            // 3. 경고 영역 제거
            // ---------------------------------------------------------
            ObjectPooler.Instance.ReturnToPool(
                "BossAOE",
                warningAOE
            );

            // ---------------------------------------------------------
            // 4. 플레이어 방향으로 돌진
            // ---------------------------------------------------------
            if (rb != null)
            {
                Vector2 dashDir =
                    (targetPos - transform.position).normalized;

                float dashSpeed = isRageMode ? 26f : 22f;
                float dashDuration = isRageMode ? 0.15f : 0.15f;

                float elapsed = 0f;

                while (elapsed < dashDuration)
                {
                    rb.MovePosition(
                        rb.position +
                        dashDir *
                        dashSpeed *
                        Time.fixedDeltaTime
                    );

                    elapsed += Time.fixedDeltaTime;

                    yield return new WaitForFixedUpdate();
                }
            }
            else
            {
                // Rigidbody2D가 없다면 목표 위치로 순간 이동
                transform.position = targetPos;
            }

            // ---------------------------------------------------------
            // 5. 돌진이 끝난 위치에 장판 생성
            // ---------------------------------------------------------
            GameObject floorAOE =
                ObjectPooler.Instance.SpawnFromPool(
                    "BossAOE",
                    warningAOEPrefab,
                    transform.position,
                    Quaternion.identity
                );

            if (floorAOE == null)
            {
                Debug.LogError("[Boss] 장판 AOE 생성 실패");
                yield break;
            }

            // ---------------------------------------------------------
            // 6. 장판 3초 유지
            //
            // 장판 오브젝트가 자체적으로 데미지를 처리한다면
            // 여기서는 3초 후 반환만 하면 됩니다.
            // ---------------------------------------------------------
            StartCoroutine(
                ReturnAOEAfterDelay(
                    floorAOE,
                    3f
                )
            );

            // ---------------------------------------------------------
            // 7. 2페이즈라면 두 번째 돌진 준비
            // ---------------------------------------------------------
            if (isRageMode && d < dashCount - 1)
            {
                // 첫 번째 돌진 후 잠깐 대기
                yield return new WaitForSeconds(0.3f);
            }
        }
    }


    // -------------------------------------------------------------------------
    // AOE를 일정 시간 후 ObjectPool로 반환
    // -------------------------------------------------------------------------
    private IEnumerator ReturnAOEAfterDelay(
        GameObject aoe,
        float duration)
    {
        yield return new WaitForSeconds(duration);

        if (aoe != null && ObjectPooler.Instance != null)
        {
            ObjectPooler.Instance.ReturnToPool(
                "BossAOE",
                aoe
            );
        }
    }
}
