using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("추적 대상")]
    public Transform target; // 추적할 플레이어 Transform

    [Header("카메라 설정")]
    public float smoothSpeed = 5f; // 부드러운 이동 속도
    public Vector3 offset = new Vector3(0f, 0f, -10f); // 2D 카메라 Z축 유지(-10)

    private void LateUpdate()
    {
        // Target(플레이어)이 없거나 비활성화되어 있어도 에러 없이 감지
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            // 플레이어를 찾아 자동으로 Target으로 할당 (선택 사항)
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && player.activeInHierarchy)
            {
                target = player.transform;
            }
            return;
        }

        // Target 위치 + 오프셋 계산
        Vector3 desiredPosition = target.position + offset;

        // Lerp/SmoothDamp를 사용하여 카메라 부드럽게 이동
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        transform.position = smoothedPosition;
    }
}