using UnityEngine;

public class Room : MonoBehaviour
{
    public bool doorTop, doorBottom, doorLeft, doorRight;
    public Vector2Int gridPosition; // 격자에서의 좌표 (예: 0,0 / 1,0 등)

    // 방 내부의 문(Door) 오브젝트나 벽을 활성화/비활성화할 때 사용
}