using System.Collections;
using UnityEngine;

public class EnemyBoss : MonoBehaviour
{
    public GameObject projectilePrefab;
    public GameObject warningAOEPrefab; // 장판 프리팹
    public Transform firePoint;
    public float patternInterval = 3f;

    private Transform player;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        StartCoroutine(BossPatternRoutine());
    }

    private IEnumerator BossPatternRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(patternInterval);

            int pattern = Random.Range(0, 2);
            if (pattern == 0)
            {
                // 패턴 1: 360도 투사체 발사 (8방향)[cite: 1]
                for (int i = 0; i < 8; i++)
                {
                    float angle = i * 45f;
                    Quaternion rot = Quaternion.Euler(0, 0, angle);
                    GameObject proj = Instantiate(projectilePrefab, transform.position, rot);
                    proj.GetComponent<Projectile>()?.Setup(15f, 0, "Player");
                }
            }
            else
            {
                // 패턴 2: 플레이어 위치에 장판 공격[cite: 1]
                if (player != null)
                {
                    Vector3 targetPos = player.position;
                    GameObject warning = Instantiate(warningAOEPrefab, targetPos, Quaternion.identity);
                    yield return new WaitForSeconds(1f); // 1초 경고 후 폭발/피해

                    // 장판 피해 판정
                    Collider2D hit = Physics2D.OverlapCircle(targetPos, 2f, LayerMask.GetMask("Player"));
                    if (hit != null)
                    {
                        hit.GetComponent<IDamageable>()?.TakeDamage(25f);
                    }
                    Destroy(warning);
                }
            }
        }
    }
}