using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public static DungeonGenerator Instance;

    public GameObject room1_Start;      // 1번: 시작방
    public GameObject[] roomNormal;     // 1~4번: 일반방 템플릿 배열
    public GameObject room5_Chest;      // 5번: 상자방
    public GameObject room6_Boss;       // 6번: 보스방
    public GameObject portalPrefab;     // F키 상호작용 포탈 프리팹

    public int totalRooms = 8;
    public float roomSize = 20f; // 20x20 크기

    private Dictionary<Vector2Int, RoomData> dungeonGrid = new Dictionary<Vector2Int, RoomData>();
    private Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        GenerateIsaacDungeon();
    }

    void GenerateIsaacDungeon()
    {
        List<Vector2Int> roomPositions = new List<Vector2Int>();
        Vector2Int currentPos = Vector2Int.zero;
        roomPositions.Add(currentPos);

        // 1. 길맥 배치 (Random Walk)
        while (roomPositions.Count < totalRooms - 2)
        {
            Vector2Int nextPos = currentPos + directions[Random.Range(0, 4)];
            if (!roomPositions.Contains(nextPos))
            {
                roomPositions.Add(nextPos);
            }
            currentPos = nextPos;
        }

        // 2. 시작방 생성
        SpawnRoomObject(room1_Start, Vector2Int.zero);

        // 3. 일반방들 생성
        for (int i = 1; i < roomPositions.Count; i++)
        {
            GameObject randNormal = roomNormal[Random.Range(0, roomNormal.Length)];
            SpawnRoomObject(randNormal, roomPositions[i]);
        }

        // 4. 상자방(5번), 보스방(6번) 막다른 길/끝점 배치
        Vector2Int chestPos = GetEndPosition(roomPositions);
        roomPositions.Add(chestPos);
        SpawnRoomObject(room5_Chest, chestPos);

        Vector2Int bossPos = GetEndPosition(roomPositions);
        SpawnRoomObject(room6_Boss, bossPos);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // 1. 플레이어 위치를 (0,0,0)으로 이동
            player.transform.position = new Vector3(0f, 0f, 0f);

            // 2. Rigidbody2D가 있다면 속도 초기화 (이동 잔상이 남지 않도록)
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector2.zero; // Unity 2023+ (구버전은 velocity)
            }
        }
        Vector2Int startPos = Vector2Int.zero;
        if (dungeonGrid.ContainsKey(startPos))
        {
            OnRoomCleared(dungeonGrid[startPos]);
        }
    }

    void SpawnRoomObject(GameObject prefab, Vector2Int gridPos)
    {
        Vector3 worldPos = new Vector3(gridPos.x * roomSize, gridPos.y * roomSize, 0);
        GameObject roomObj = Instantiate(prefab, worldPos, Quaternion.identity, transform);
        RoomData roomData = roomObj.GetComponent<RoomData>();
        dungeonGrid.Add(gridPos, roomData);
    }

    Vector2Int GetEndPosition(List<Vector2Int> existing)
    {
        foreach (var pos in existing)
        {
            foreach (var dir in directions)
            {
                Vector2Int target = pos + dir;
                if (!existing.Contains(target)) return target;
            }
        }
        return Vector2Int.zero;
    }

    // 방 클리어 시 상하좌우 연결 여부 확인하여 포탈 생성
    public void OnRoomCleared(RoomData currentRoom)
    {
        Vector2Int currentGridPos = Vector2Int.zero;
        foreach (var pair in dungeonGrid)
        {
            if (pair.Value == currentRoom)
            {
                currentGridPos = pair.Key;
                break;
            }
        }

        for (int i = 0; i < directions.Length; i++)
        {
            Vector2Int neighborPos = currentGridPos + directions[i];
            if (dungeonGrid.ContainsKey(neighborPos))
            {
                // 인접한 다음 방으로 가는 포탈 생성
                Transform pPoint = currentRoom.portalPoints[i];
                GameObject portalObj = Instantiate(portalPrefab, pPoint.position, Quaternion.identity, currentRoom.transform);

                Portal portal = portalObj.GetComponent<Portal>();
                portal.targetWorldPos = dungeonGrid[neighborPos].transform.position;
            }
        }
    }
}