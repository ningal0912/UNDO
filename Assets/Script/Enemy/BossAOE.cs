using System.Collections;
using UnityEngine;

public class BossAOE : MonoBehaviour
{
    [Header("색상")]
    [Tooltip("돌진 전 경고 색상")]
    public Color warningColor = new Color(1f, 0f, 0f, 0.35f);

    [Tooltip("돌진 후 실제 장판 색상")]
    public Color activeColor = new Color(1f, 0f, 0f, 0.85f);

    [Header("장판 설정")]
    [Tooltip("장판이 유지되는 시간")]
    public float duration = 3f;

    [Tooltip("피해를 주는 간격")]
    public float damageInterval = 0.2f;

    [Header("풀링")]
    public string poolTag = "BossAOE";

    private SpriteRenderer spriteRenderer;
    private Collider2D aoeCollider;

    private float damage;
    private Coroutine damageCoroutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        aoeCollider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        // 풀에서 다시 꺼내졌을 때 상태 초기화
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (aoeCollider == null)
            aoeCollider = GetComponent<Collider2D>();

        SetWarningState();
    }

    // =========================================================
    // 경고 상태
    // =========================================================
    public void SetWarningState()
    {
        // 이전 데미지 코루틴이 남아있다면 종료
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = warningColor;
        }

        // 경고 중에는 피해 판정 없음
        if (aoeCollider != null)
        {
            aoeCollider.enabled = false;
        }
    }

    // =========================================================
    // 실제 장판 활성화
    // =========================================================
    public void Activate(float damageAmount, float activeDuration)
    {
        damage = damageAmount;
        duration = activeDuration;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = activeColor;
        }

        if (aoeCollider != null)
        {
            aoeCollider.enabled = true;
        }

        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
        }

        damageCoroutine = StartCoroutine(DamageRoutine());
    }

    // =========================================================
    // 장판 데미지
    // =========================================================
    private IEnumerator DamageRoutine()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            DamagePlayersInside();

            yield return new WaitForSeconds(damageInterval);

            elapsed += damageInterval;
        }

        damageCoroutine = null;

        ReturnToPool();
    }

    // =========================================================
    // 장판 안에 있는 플레이어 찾기
    // =========================================================
    private void DamagePlayersInside()
    {
        if (aoeCollider == null)
            return;

        Bounds bounds = aoeCollider.bounds;

        Collider2D[] hits =
            Physics2D.OverlapBoxAll(
                bounds.center,
                bounds.size,
                transform.eulerAngles.z,
                LayerMask.GetMask("Player")
            );

        foreach (Collider2D hit in hits)
        {
            IDamageable damageable =
                hit.GetComponent<IDamageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }
        }
    }

    // =========================================================
    // ObjectPool 반환
    // =========================================================
    private void ReturnToPool()
    {
        if (ObjectPooler.Instance != null)
        {
            ObjectPooler.Instance.ReturnToPool(
                poolTag,
                gameObject
            );
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
