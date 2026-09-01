using System; // System.Action 사용을 위해 필수
using UnityEngine;

public class EntityHealth : MonoBehaviour, IDamageable
{
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isPlayer = false;

    // --- 사망 이벤트 전달용 Action (RoomData에서 구독) ---
    public Action onDeath;
    // ---------------------------------------------------

    void OnEnable()
    {
        // 오브젝트 풀링으로 재활용될 때 체력 초기화
        currentHealth = maxHealth;
    }

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // 1. 사망 이벤트(Action) 호출하여 RoomData에 적 처치 알림
        onDeath?.Invoke();

        if (isPlayer)
        {
            Debug.Log("플레이어 사망 - 게임 오버");
            // 게임 오버 처리 로직
        }
        else
        {
            // 이벤트 전구 구독 해제 (중복 방지)
            onDeath = null;
        }
    }
}