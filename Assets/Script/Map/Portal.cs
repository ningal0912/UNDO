using UnityEngine;
using UnityEngine.InputSystem; // New Input System 기준

public class Portal : MonoBehaviour
{
    public Vector3 targetWorldPos;
    private bool isPlayerNearby = false;
    private Transform playerTransform;

    void Update()
    {
        if (isPlayerNearby && Keyboard.current.fKey.wasPressedThisFrame)
        {
            // 플레이어 순간이동
            playerTransform.position = targetWorldPos;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = true;
            playerTransform = collision.transform;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }
}