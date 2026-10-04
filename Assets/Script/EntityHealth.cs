using System; // Action 이벤트를 사용하기 위해 필수
using System.Collections;
using UnityEngine;

public class EntityHealth : MonoBehaviour, IDamageable
{
    [Header("체력 설정")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("무적 설정 (플레이어 전용)")]
    public bool useInvincibility = false;
    public float invincibilityDuration = 0.3f;
    public float blinkInterval = 0.05f;

    // RoomData 등 외부 스크립트에 사망 알림을 보내는 이벤트
    public event Action onDeath;

    private bool isInvincible = false;
    private SpriteRenderer spriteRenderer;

    void OnEnable()
    {
        // 오브젝트 풀링으로 재활용될 때 체력 및 상태 초기화
        currentHealth = maxHealth;
        isInvincible = false;
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
    }

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void TakeDamage(float amount)
    {
        if (isInvincible) return;

        currentHealth -= amount;
        Debug.Log($"{gameObject.name} 피격! 남은 체력: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else if (useInvincibility)
        {
            StartCoroutine(InvincibilityRoutine());
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        Debug.Log($"{gameObject.name} 체력 회복! 현재 체력: {currentHealth}");
    }

    private void Die()
    {
        // 1. RoomData에 사망 알림 (방 클리어 체크)
        onDeath?.Invoke();

        // 2. 플레이어인지 몬스터인지에 따른 처리
        if (useInvincibility)
        {
            // 플레이어라면 게임 오버 또는 파괴 처리
            Destroy(gameObject);
        }
        else
        {
            // 몬스터라면 오브젝트 풀링을 위해 파괴 대신 비활성화 후 반납
            gameObject.SetActive(false);
        }
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        float elapsedTime = 0f;

        while (elapsedTime < invincibilityDuration)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
            }

            yield return new WaitForSeconds(blinkInterval);
            elapsedTime += blinkInterval;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        isInvincible = false;
    }
}