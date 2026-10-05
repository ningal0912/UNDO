using System.Collections;
using UnityEngine;
using DG.Tweening;

public class EnemyBoss : MonoBehaviour
{
    // =========================================================
    // 기본 설정
    // =========================================================

    [Header("========== 기본 설정 ==========")]

    public EntityHealth entityHealth;
    public Rigidbody2D rb;

    [Header("플레이어")]
    public Transform player;

    [Header("탄환")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("AOE")]
    public GameObject warningAOEPrefab;


    // =========================================================
    // 페이즈
    // =========================================================

    [Header("========== 페이즈 설정 ==========")]

    [Tooltip("이 체력 비율 이하가 되면 2페이즈")]
    [Range(0f, 1f)]
    public float phase2HealthPercent = 0.3f;

    private bool isPhase2 = false;


    // =========================================================
    // 패턴 사이 간격
    // =========================================================

    [Header("========== 패턴 사이 간격 ==========")]

    [Tooltip("1페이즈 패턴 사이 대기 시간")]
    public float phase1PatternInterval = 5f;

    [Tooltip("2페이즈 패턴 사이 대기 시간")]
    public float phase2PatternInterval = 3f;


    // =========================================================
    // 패턴 1
    // =========================================================

    [Header("========== 패턴 1 : 플레이어 방향 연속 발사 ==========")]

    [Header("1페이즈")]

    [Tooltip("1페이즈 패턴1 총 발사 횟수")]
    public int phase1Pattern1BulletCount = 5;

    [Tooltip("1페이즈 패턴1 발사 간격")]
    public float phase1Pattern1FireInterval = 0.5f;

    [Tooltip("1페이즈 패턴1 탄환 속도")]
    public float phase1Pattern1BulletSpeed = 15f;


    [Header("2페이즈")]

    [Tooltip("2페이즈 패턴1 총 발사 횟수")]
    public int phase2Pattern1BulletCount = 7;

    [Tooltip("2페이즈 패턴1 발사 간격")]
    public float phase2Pattern1FireInterval = 0.3f;

    [Tooltip("2페이즈 패턴1 탄환 속도")]
    public float phase2Pattern1BulletSpeed = 20f;


    // =========================================================
    // 패턴 2
    // =========================================================

    [Header("========== 패턴 2 : 4방향 회전 발사 ==========")]

    [Header("1페이즈")]

    [Tooltip("1페이즈 패턴2 총 발사 횟수")]
    public int phase1Pattern2BulletCount = 5;

    [Tooltip("1페이즈 패턴2 발사 간격")]
    public float phase1Pattern2FireInterval = 0.25f;

    [Tooltip("1페이즈 패턴2 탄환 속도")]
    public float phase1Pattern2BulletSpeed = 15f;


    [Header("2페이즈")]

    [Tooltip("2페이즈 패턴2 총 발사 횟수")]
    public int phase2Pattern2BulletCount = 5;

    [Tooltip("2페이즈 패턴2 발사 간격")]
    public float phase2Pattern2FireInterval = 0.2f;

    [Tooltip("2페이즈 패턴2 탄환 속도")]
    public float phase2Pattern2BulletSpeed = 22f;


    [Header("패턴 2 회전 설정")]

    [Tooltip("시계 방향으로 회전할 총 각도")]
    public float pattern2TotalRotation = 90f;

    [Tooltip("한 번 발사할 때마다 회전하는 각도")]
    public float pattern2RotationPerShot = 22.5f;


    // =========================================================
    // 패턴 3
    // =========================================================

    [Header("========== 패턴 3 : 대시 + AOE ==========")]

    [Header("대시")]

    [Tooltip("대시 거리. 8이면 약 8타일")]
    public float dashDistance = 8f;

    [Tooltip("대시 이동 시간")]
    public float dashDuration = 0.25f;

    [Tooltip("대시 전 경고 시간")]
    public float dashWarningTime = 0.8f;

    [Tooltip("2번째 대시까지 대기 시간")]
    public float dashBetweenDelay = 0.3f;


    [Header("AOE")]

    [Tooltip("AOE 지속 시간")]
    public float dashAOEDuration = 3f;

    [Tooltip("대시 경로 AOE 폭")]
    public float dashAOEWidth = 1.5f;

    [Tooltip("1페이즈 AOE 데미지")]
    public float phase1AOEDamage = 25f;

    [Tooltip("2페이즈 AOE 데미지")]
    public float phase2AOEDamage = 35f;


    // =========================================================
    // 대시 연출
    // =========================================================

    [Header("========== 대시 연출 ==========")]

    public float chargeScale = 0.85f;
    public float dashScale = 1.15f;

    private Vector3 originalScale;


    // =========================================================
    // 삼각형 보스 방향
    // =========================================================

    [Header("========== 보스 방향 ==========")]

    [Tooltip("활성화하면 보스의 한 꼭짓점이 플레이어를 바라봄")]
    public bool facePlayer = true;

    [Tooltip("삼각형 Sprite의 꼭짓점 방향에 맞춰 조절")]
    public float triangleRotationOffset = -90f;


    // =========================================================
    // 기타
    // =========================================================

    private Coroutine patternCoroutine;


    // =========================================================
    // Start
    // =========================================================

    private void Start()
    {
        originalScale = transform.localScale;

        // 플레이어 찾기
        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
            else
            {
                Debug.LogError(
                    "[EnemyBoss] Player 태그를 가진 오브젝트를 찾지 못했습니다."
                );
            }
        }

        // 체력
        if (entityHealth == null)
        {
            entityHealth =
                GetComponent<EntityHealth>();
        }

        // Rigidbody
        if (rb == null)
        {
            rb =
                GetComponent<Rigidbody2D>();
        }

        // 필수 오브젝트 검사
        if (projectilePrefab == null)
        {
            Debug.LogError(
                "[EnemyBoss] projectilePrefab이 연결되지 않았습니다."
            );
        }

        if (warningAOEPrefab == null)
        {
            Debug.LogError(
                "[EnemyBoss] warningAOEPrefab이 연결되지 않았습니다."
            );
        }

        if (ObjectPooler.Instance == null)
        {
            Debug.LogError(
                "[EnemyBoss] ObjectPooler가 씬에 없습니다."
            );
        }

        // 패턴 시작
        patternCoroutine =
            StartCoroutine(BossPatternRoutine());
    }


    // =========================================================
    // Update
    // =========================================================

    private void Update()
    {
        CheckPhase();

        if (facePlayer)
        {
            RotateTowardPlayer();
        }
    }


    // =========================================================
    // 페이즈 확인
    // =========================================================

    private void CheckPhase()
    {
        if (entityHealth == null)
            return;

        float phase2Health =
            entityHealth.maxHealth *
            phase2HealthPercent;

        if (!isPhase2 &&
            entityHealth.currentHealth <= phase2Health)
        {
            isPhase2 = true;

            Debug.Log(
                "=============================="
            );

            Debug.Log(
                "보스 2페이즈 진입!"
            );

            Debug.Log(
                "=============================="
            );
        }
    }


    // =========================================================
    // 보스가 플레이어를 바라보기
    // =========================================================

    private void RotateTowardPlayer()
    {
        if (player == null)
            return;

        Vector2 direction =
            player.position -
            transform.position;

        if (direction.sqrMagnitude < 0.001f)
            return;

        float angle =
            Mathf.Atan2(
                direction.y,
                direction.x
            ) * Mathf.Rad2Deg;

        transform.rotation =
            Quaternion.Euler(
                0f,
                0f,
                angle + triangleRotationOffset
            );
    }


    // =========================================================
    // 전체 패턴 루틴
    // =========================================================

    private IEnumerator BossPatternRoutine()
    {
        // 보스 등장 후 첫 패턴까지 대기
        yield return new WaitForSeconds(2f);

        while (true)
        {
            // 패턴 선택
            int patternIndex =
                Random.Range(0, 3);

            switch (patternIndex)
            {
                case 0:

                    yield return StartCoroutine(
                        Pattern1_ShootArrowsAtPlayer()
                    );

                    break;


                case 1:

                    yield return StartCoroutine(
                        Pattern2_ClockwiseSectorShoot()
                    );

                    break;


                case 2:

                    yield return StartCoroutine(
                        Pattern3_DashAndAOE()
                    );

                    break;
            }

            // 패턴이 끝난 후 대기
            float patternInterval =
                isPhase2
                ? phase2PatternInterval
                : phase1PatternInterval;

            yield return new WaitForSeconds(
                patternInterval
            );
        }
    }


    // =========================================================
    // 패턴 1
    //
    // 플레이어를 향해
    // 정해진 횟수만큼 연속 발사
    // =========================================================

    private IEnumerator Pattern1_ShootArrowsAtPlayer()
    {
        if (player == null)
            yield break;

        int bulletCount;
        float fireInterval;
        float bulletSpeed;

        if (ObjectPooler.Instance == null || projectilePrefab == null)
        {
            Debug.LogError("[EnemyBoss] ObjectPooler 인스턴스 또는 projectilePrefab이 비어 있습니다!");
            yield break;
        }

        // -----------------------------------------
        // 페이즈별 설정
        // -----------------------------------------

        if (isPhase2)
        {
            bulletCount =
                phase2Pattern1BulletCount;

            fireInterval =
                phase2Pattern1FireInterval;

            bulletSpeed =
                phase2Pattern1BulletSpeed;
        }
        else
        {
            bulletCount =
                phase1Pattern1BulletCount;

            fireInterval =
                phase1Pattern1FireInterval;

            bulletSpeed =
                phase1Pattern1BulletSpeed;
        }

        bulletCount =
            Mathf.Max(0, bulletCount);


        // -----------------------------------------
        // 발사
        // -----------------------------------------

        for (int i = 0;
             i < bulletCount;
             i++)
        {
            if (player == null)
                yield break;

            Vector2 spawnPosition;

            if (firePoint != null)
            {
                spawnPosition =
                    firePoint.position;
            }
            else
            {
                spawnPosition =
                    transform.position;
            }


            // 플레이어 방향
            Vector2 direction =
                (
                    (Vector2)player.position -
                    spawnPosition
                ).normalized;


            if (direction.sqrMagnitude < 0.001f)
            {
                direction =
                    Vector2.right;
            }


            // 탄환 회전
            float angle =
                Mathf.Atan2(
                    direction.y,
                    direction.x
                ) * Mathf.Rad2Deg;


            // 풀에서 탄환 가져오기
            GameObject projectile =
                ObjectPooler.Instance.SpawnFromPool(
                    "Enemy",
                    projectilePrefab,
                    spawnPosition,
                    Quaternion.Euler(
                        0f,
                        0f,
                        angle
                    )
                );


            if (projectile != null)
            {
                Projectile projectileScript =
                    projectile.GetComponent<Projectile>();

                if (projectileScript != null)
                {
                    projectileScript.Setup(
                        10f,
                        0,
                        "Player",
                        "Enemy"
                    );

                    // Inspector에서 설정한 탄속
                    projectileScript.speed =
                        bulletSpeed;
                }
            }


            // 다음 발사까지 대기
            yield return new WaitForSeconds(
                fireInterval
            );
        }
    }


    // =========================================================
    // 패턴 2
    //
    // 상 / 하 / 좌 / 우 4방향에서 시작
    //
    // 한 번에 4발 발사
    // ↓
    // 22.5도 시계방향 회전
    // ↓
    // 다시 4발 발사
    // ↓
    // ...
    //
    // 총 90도 회전하면 종료
    // =========================================================

    private IEnumerator Pattern2_ClockwiseSectorShoot()
    {
        int bulletCount;
        float fireInterval;
        float bulletSpeed;

        if (ObjectPooler.Instance == null || projectilePrefab == null)
        {
            Debug.LogError("[EnemyBoss] ObjectPooler 인스턴스 또는 projectilePrefab이 비어 있습니다!");
            yield break;
        }

        // -----------------------------------------
        // 페이즈별 설정
        // -----------------------------------------

        if (isPhase2)
        {
            bulletCount =
                phase2Pattern2BulletCount;

            fireInterval =
                phase2Pattern2FireInterval;

            bulletSpeed =
                phase2Pattern2BulletSpeed;
        }
        else
        {
            bulletCount =
                phase1Pattern2BulletCount;

            fireInterval =
                phase1Pattern2FireInterval;

            bulletSpeed =
                phase1Pattern2BulletSpeed;
        }


        bulletCount =
            Mathf.Max(1, bulletCount);


        // -----------------------------------------
        // 시작 방향
        //
        // 0 / 90 / 180 / 270
        //
        // 즉 상하좌우 중 하나에서 시작
        // -----------------------------------------

        float baseAngle =
            Random.Range(0, 4) * 90f;


        // -----------------------------------------
        // 총 몇 번 발사할지 계산
        //
        // 예:
        // 총 회전 90도
        // 1회당 22.5도
        //
        // 0
        // 22.5
        // 45
        // 67.5
        // 90
        //
        // = 총 5회
        // -----------------------------------------

        int shotCount =
            Mathf.FloorToInt(
                pattern2TotalRotation /
                pattern2RotationPerShot
            ) + 1;


        // Inspector에서 설정한 탄환 개수와
        // 실제 회전 횟수를 혼동하지 않도록
        // 최소 1회 보장
        shotCount =
            Mathf.Max(1, shotCount);


        // -----------------------------------------
        // 회전하면서 발사
        // -----------------------------------------

        for (int shot = 0;
             shot < shotCount;
             shot++)
        {
            // 시계방향 회전
            float currentAngle =
                baseAngle -
                shot * pattern2RotationPerShot;


            // -------------------------------------
            // 4방향 발사
            //
            // 현재 방향
            // +90
            // +180
            // +270
            // -------------------------------------

            for (int directionIndex = 0;
                 directionIndex < bulletCount;
                 directionIndex++)
            {
                float bulletAngle =
                    currentAngle +
                    directionIndex * 90f;


                Vector2 direction =
                    new Vector2(
                        Mathf.Cos(
                            bulletAngle *
                            Mathf.Deg2Rad
                        ),

                        Mathf.Sin(
                            bulletAngle *
                            Mathf.Deg2Rad
                        )
                    ).normalized;


                Vector2 spawnPosition;

                if (firePoint != null)
                {
                    spawnPosition =
                        firePoint.position;
                }
                else
                {
                    spawnPosition =
                        transform.position;
                }


                // ---------------------------------
                // 탄환 생성
                // ---------------------------------

                GameObject projectile =
                    ObjectPooler.Instance.SpawnFromPool(
                        "Enemy",
                        projectilePrefab,
                        spawnPosition,
                        Quaternion.Euler(
                            0f,
                            0f,
                            bulletAngle
                        )
                    );


                if (projectile != null)
                {
                    Projectile projectileScript =
                        projectile.GetComponent<Projectile>();

                    if (projectileScript != null)
                    {
                        projectileScript.Setup(
                            10f,
                            0,
                            "Player",
                            "Enemy"
                        );

                        projectileScript.speed =
                            bulletSpeed;
                    }
                }
            }


            // -------------------------------------
            // 다음 회전까지 대기
            // -------------------------------------

            if (shot < shotCount - 1)
            {
                yield return new WaitForSeconds(
                    fireInterval
                );
            }
        }
    }


    // =========================================================
    // 패턴 3
    //
    // 1페이즈 = 1회 대시
    // 2페이즈 = 2회 대시
    // =========================================================

    private IEnumerator Pattern3_DashAndAOE()
    {
        if (player == null)
            yield break;


        int dashCount =
            isPhase2 ? 2 : 1;


        float aoeDamage =
            isPhase2
            ? phase2AOEDamage
            : phase1AOEDamage;


        for (int dashIndex = 0;
             dashIndex < dashCount;
             dashIndex++)
        {
            // -------------------------------------
            // 시작 위치
            // -------------------------------------

            Vector2 startPosition;

            if (rb != null)
            {
                startPosition =
                    rb.position;
            }
            else
            {
                startPosition =
                    transform.position;
            }


            // -------------------------------------
            // 플레이어 방향
            // -------------------------------------

            Vector2 direction =
                (
                    (Vector2)player.position -
                    startPosition
                ).normalized;


            if (direction.sqrMagnitude < 0.001f)
            {
                direction =
                    Vector2.right;
            }


            // -------------------------------------
            // 플레이어까지 가지 않고
            // 지정한 거리만큼만 대시
            // -------------------------------------

            Vector2 dashTarget =
                startPosition +
                direction * dashDistance;


            // -------------------------------------
            // AOE 중심
            // -------------------------------------

            Vector2 aoeCenter =
                startPosition +
                direction *
                (dashDistance * 0.5f);


            // -------------------------------------
            // AOE 생성
            // -------------------------------------

            GameObject aoeObject =
                ObjectPooler.Instance.SpawnFromPool(
                    "BossAOE",
                    warningAOEPrefab,
                    aoeCenter,
                    Quaternion.identity
                );


            BossAOE aoe = null;


            if (aoeObject != null)
            {
                aoe =
                    aoeObject.GetComponent<BossAOE>();


                if (aoe != null)
                {
                    float aoeAngle =
                        Mathf.Atan2(
                            direction.y,
                            direction.x
                        ) * Mathf.Rad2Deg;


                    aoeObject.transform.rotation =
                        Quaternion.Euler(
                            0f,
                            0f,
                            aoeAngle
                        );


                    // 긴 방향 = 대시 거리
                    // 짧은 방향 = AOE 폭
                    aoeObject.transform.localScale =
                        new Vector3(
                            dashDistance,
                            dashAOEWidth,
                            1f
                        );


                    // 반투명 경고
                    aoe.SetWarningState();
                }
            }


            // -------------------------------------
            // 대시 준비 연출
            // -------------------------------------

            transform.DOKill();


            Sequence chargeSequence =
                DOTween.Sequence();


            chargeSequence.Append(
                transform.DOScale(
                    originalScale *
                    chargeScale,

                    dashWarningTime *
                    0.5f
                )
            );


            chargeSequence.Append(
                transform.DOScale(
                    originalScale *
                    dashScale,

                    dashWarningTime *
                    0.2f
                )
            );


            chargeSequence.Append(
                transform.DOScale(
                    originalScale,

                    dashWarningTime *
                    0.3f
                )
            );


            yield return
                chargeSequence
                .WaitForCompletion();


            // -------------------------------------
            // 대시
            // -------------------------------------

            if (rb != null)
            {
                rb.bodyType =
                    RigidbodyType2D.Kinematic;
            }


            transform.DOMove(
                dashTarget,
                dashDuration
            )
            .SetEase(
                Ease.InQuad
            );


            yield return new WaitForSeconds(
                dashDuration
            );


            // -------------------------------------
            // AOE 활성화
            // -------------------------------------

            if (aoe != null)
            {
                aoe.Activate(
                    aoeDamage,
                    dashAOEDuration
                );
            }


            // -------------------------------------
            // 2페이즈 두 번째 대시
            // -------------------------------------

            if (dashIndex <
                dashCount - 1)
            {
                yield return
                    new WaitForSeconds(
                        dashBetweenDelay
                    );
            }
        }
    }


    // =========================================================
    // 비활성화
    // =========================================================

    private void OnDisable()
    {
        transform.DOKill();

        transform.localScale =
            originalScale;
        // 보스 사망 시 처리
        if (GameOverPanelUI.Instance != null)
        {
            GameOverPanelUI.Instance.ShowEndPanel("VICTORY!");
        }
    }
}
