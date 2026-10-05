using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Header("연결할 컴포넌트")]
    public EntityHealth targetHealth; // 체력을 가져올 대상 (플레이어 등)
    public Slider hpSlider;           // UI Slider 컴포넌트

    void Start()
    {
        // TargetHealth가 지정되지 않았다면 Player 태그를 가진 오브젝트에서 찾아 할당
        if (targetHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                targetHealth = player.GetComponent<EntityHealth>();
            }
        }

        // 초기 슬라이더 값 세팅
        if (targetHealth != null && hpSlider != null)
        {
            hpSlider.maxValue = targetHealth.maxHealth;
            hpSlider.value = targetHealth.currentHealth;
        }
    }

    void Update()
    {
        // 매 프레임마다 플레이어의 currentHealth 변화를 UI에 반영
        if (targetHealth != null && hpSlider != null)
        {
            hpSlider.value = targetHealth.currentHealth;
        }
    }
}