using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public static DungeonGenerator Instance;

    [Header("방 프리팹 설정")]
    public GameObject room1_Start;
    public GameObject[] roomNormal;
    public GameObject room5_Chest;
    public GameObject room6_Boss;
    public GameObject portalPrefab;

    [Header("던전 생성 옵션")]
    public int totalRooms = 8;
    public float roomSize = 25f;

    private Dictionary<Vector2Int, RoomData>
        dungeonGrid =
        new Dictionary<Vector2Int, RoomData>();

    private Vector2Int[] directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    // ============================================================
    // Singleton
    // ============================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ============================================================
    // Start
    // ============================================================

    private void Start()
    {
        GenerateIsaacDungeon();
    }

    // ============================================================
    // New Dungeon
    // ============================================================

    public void GenerateIsaacDungeon()
    {
        dungeonGrid.Clear();

        List<Vector2Int> normalRoomPositions =
            new List<Vector2Int>();

        List<Vector2Int> allOccupiedPositions =
            new List<Vector2Int>();

        Vector2Int currentPos =
            Vector2Int.zero;

        normalRoomPositions.Add(
            currentPos
        );

        allOccupiedPositions.Add(
            currentPos
        );

        // --------------------------------------------------------
        // 일반방 위치
        // --------------------------------------------------------

        int targetNormalCount =
            Mathf.Max(
                1,
                totalRooms - 2
            );

        while (
            normalRoomPositions.Count <
            targetNormalCount
        )
        {
            Vector2Int nextPos =
                currentPos +
                directions[
                    Random.Range(
                        0,
                        directions.Length
                    )
                ];

            if (!allOccupiedPositions.Contains(
                nextPos))
            {
                normalRoomPositions.Add(
                    nextPos
                );

                allOccupiedPositions.Add(
                    nextPos
                );
            }

            currentPos =
                nextPos;
        }

        // --------------------------------------------------------
        // 시작방
        // --------------------------------------------------------

        SpawnRoomObject(
            room1_Start,
            Vector2Int.zero,
            -1
        );

        // --------------------------------------------------------
        // 일반방
        // --------------------------------------------------------

        for (int i = 1;
             i < normalRoomPositions.Count;
             i++)
        {
            if (roomNormal == null ||
                roomNormal.Length == 0)
                continue;

            int prefabIndex =
                Random.Range(
                    0,
                    roomNormal.Length
                );

            SpawnRoomObject(
                roomNormal[prefabIndex],
                normalRoomPositions[i],
                prefabIndex
            );
        }

        // --------------------------------------------------------
        // 보스
        // --------------------------------------------------------

        Vector2Int bossPos =
            GetFarthestEndPosition(
                normalRoomPositions,
                allOccupiedPositions
            );

        allOccupiedPositions.Add(
            bossPos
        );

        SpawnRoomObject(
            room6_Boss,
            bossPos,
            -1
        );

        // --------------------------------------------------------
        // 상자방
        // --------------------------------------------------------

        Vector2Int chestPos =
            GetChestRoomPosition(
                normalRoomPositions,
                allOccupiedPositions
            );

        allOccupiedPositions.Add(
            chestPos
        );

        SpawnRoomObject(
            room5_Chest,
            chestPos,
            -1
        );

        // --------------------------------------------------------
        // Player
        // --------------------------------------------------------

        GameObject player =
            GameObject.FindGameObjectWithTag(
                "Player"
            );

        if (player != null)
        {
            player.transform.position =
                Vector3.zero;

            Rigidbody2D rb =
                player.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.linearVelocity =
                    Vector2.zero;
            }
        }

        // --------------------------------------------------------
        // 시작방 클리어
        // --------------------------------------------------------

        if (dungeonGrid.TryGetValue(
            Vector2Int.zero,
            out RoomData startRoom))
        {
            startRoom.isCleared = false;

            OnRoomCleared(
                startRoom
            );
        }
    }

    // ============================================================
    // Spawn Room
    // ============================================================

    private void SpawnRoomObject(
        GameObject prefab,
        Vector2Int gridPos,
        int prefabIndex)
    {
        if (prefab == null)
            return;

        Vector3 worldPos =
            new Vector3(
                gridPos.x * roomSize,
                gridPos.y * roomSize,
                0f
            );

        GameObject roomObj =
            Instantiate(
                prefab,
                worldPos,
                Quaternion.identity,
                transform
            );

        RoomData roomData =
            roomObj.GetComponentInChildren<RoomData>();

        if (roomData != null)
        {
            roomData.roomPrefabIndex =
                prefabIndex;

            dungeonGrid[gridPos] =
                roomData;
        }
    }

    // ============================================================
    // Save Dungeon
    // ============================================================

    public void CreateSaveData(
        SaveData data)
    {
        if (data == null)
            return;

        data.dungeonRooms.Clear();
        data.clearedRoomPositions.Clear();

        foreach (var pair in dungeonGrid)
        {
            if (pair.Value == null)
                continue;

            RoomSaveData roomData =
                new RoomSaveData();

            roomData.gridPosition =
                pair.Key;

            roomData.roomType =
                pair.Value.roomType;

            roomData.roomPrefabIndex =
                pair.Value.roomPrefabIndex;

            roomData.isCleared =
                pair.Value.isCleared;

            data.dungeonRooms.Add(
                roomData
            );

            if (pair.Value.isCleared)
            {
                data.clearedRoomPositions.Add(
                    pair.Key
                );
            }
        }
    }

    // ============================================================
    // Restore Dungeon
    // ============================================================

    public void RestoreDungeon(
        List<RoomSaveData> savedRooms)
    {
        if (savedRooms == null ||
            savedRooms.Count == 0)
        {
            Debug.LogWarning(
                "[DungeonGenerator] 저장된 방 데이터가 없습니다."
            );

            return;
        }

        // --------------------------------------------------------
        // 기존 방 삭제
        // --------------------------------------------------------

        ClearCurrentDungeon();

        dungeonGrid.Clear();

        // --------------------------------------------------------
        // 저장된 방 그대로 생성
        // --------------------------------------------------------

        foreach (RoomSaveData savedRoom
                 in savedRooms)
        {
            GameObject prefab =
                GetRoomPrefab(
                    savedRoom
                );

            if (prefab == null)
            {
                Debug.LogError(
                    $"[DungeonGenerator] 방 프리팹을 찾을 수 없습니다. " +
                    $"Type: {savedRoom.roomType}"
                );

                continue;
            }

            SpawnRoomObject(
                prefab,
                savedRoom.gridPosition,
                savedRoom.roomPrefabIndex
            );

            if (dungeonGrid.TryGetValue(
                savedRoom.gridPosition,
                out RoomData room))
            {
                room.isCleared =
                    savedRoom.isCleared;
            }
        }

        // --------------------------------------------------------
        // 저장된 클리어 상태에 맞춰 포탈 생성
        // --------------------------------------------------------

        foreach (var pair in dungeonGrid)
        {
            if (pair.Value != null &&
                pair.Value.isCleared)
            {
                CreatePortalsOnly(
                    pair.Value
                );
            }
        }

        Debug.Log(
            $"[DungeonGenerator] 던전 복원 완료: " +
            $"{dungeonGrid.Count}개 방"
        );
    }

    // ============================================================
    // Room Prefab
    // ============================================================

    private GameObject GetRoomPrefab(
        RoomSaveData data)
    {
        if (data.roomType == 1)
        {
            return room1_Start;
        }

        if (data.roomType == 5)
        {
            return room5_Chest;
        }

        if (data.roomType == 6)
        {
            return room6_Boss;
        }

        // 일반방
        if (roomNormal == null ||
            roomNormal.Length == 0)
        {
            return null;
        }

        int index =
            data.roomPrefabIndex;

        if (index < 0 ||
            index >= roomNormal.Length)
        {
            index = 0;
        }

        return roomNormal[index];
    }

    // ============================================================
    // Clear Dungeon
    // ============================================================

    private void ClearCurrentDungeon()
    {
        // 방 안 몬스터 정리
        foreach (var pair in dungeonGrid)
        {
            if (pair.Value != null)
            {
                pair.Value.ClearCurrentEnemies();
            }
        }

        // 생성된 포탈 정리
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        dungeonGrid.Clear();
    }

    // ============================================================
    // Get Room
    // ============================================================

    public RoomData GetRoom(
        Vector2Int gridPos)
    {
        if (dungeonGrid.TryGetValue(
            gridPos,
            out RoomData room))
        {
            return room;
        }

        return null;
    }

    // ============================================================
    // Get Grid Position
    // ============================================================

    public bool TryGetGridPosition(
        RoomData room,
        out Vector2Int gridPosition)
    {
        foreach (var pair in dungeonGrid)
        {
            if (pair.Value == room)
            {
                gridPosition =
                    pair.Key;

                return true;
            }
        }

        gridPosition =
            Vector2Int.zero;

        return false;
    }

    // ============================================================
    // Clear
    // ============================================================

    public void OnRoomCleared(
        RoomData currentRoom)
    {
        if (currentRoom == null)
            return;

        GameObject player =
            GameObject.FindGameObjectWithTag(
                "Player"
            );

        if (player != null)
        {
            EntityHealth health =
                player.GetComponent<EntityHealth>();

            if (health != null)
            {
                health.Heal(30f);
            }
        }

        CreatePortalsOnly(
            currentRoom
        );
    }

    // ============================================================
    // Portal
    // ============================================================

    private void CreatePortalsOnly(
        RoomData currentRoom)
    {
        if (currentRoom == null ||
            portalPrefab == null)
            return;

        if (!TryGetGridPosition(
            currentRoom,
            out Vector2Int currentGridPos))
        {
            return;
        }

        for (int i = 0;
             i < directions.Length;
             i++)
        {
            Vector2Int neighborPos =
                currentGridPos +
                directions[i];

            if (!dungeonGrid.ContainsKey(
                neighborPos))
            {
                continue;
            }

            if (currentRoom.portalPoints == null ||
                i >= currentRoom.portalPoints.Length)
            {
                continue;
            }

            Transform point =
                currentRoom.portalPoints[i];

            if (point == null)
                continue;

            // 이미 포탈이 있으면 생성하지 않음
            if (point.childCount > 0)
                continue;

            GameObject portalObj =
                Instantiate(
                    portalPrefab,
                    point.position,
                    Quaternion.identity,
                    point
                );

            Portal portal =
                portalObj.GetComponent<Portal>();

            if (portal == null)
                continue;

            RoomData neighborRoom =
                dungeonGrid[
                    neighborPos
                ];

            int oppositeIndex =
                GetOppositeIndex(i);

            if (neighborRoom.portalPoints != null &&
                oppositeIndex <
                neighborRoom.portalPoints.Length &&
                neighborRoom.portalPoints[
                    oppositeIndex] != null)
            {
                Transform targetPoint =
                    neighborRoom.portalPoints[
                        oppositeIndex];

                Vector3 directionToCenter =
                    (
                        neighborRoom.transform.position -
                        targetPoint.position
                    ).normalized;

                float offset = 1.5f;

                portal.targetWorldPos =
                    targetPoint.position +
                    directionToCenter * offset;
            }
            else
            {
                portal.targetWorldPos =
                    neighborRoom.transform.position;
            }
        }
    }

    // ============================================================
    // Farthest Room
    // ============================================================

    private Vector2Int GetFarthestEndPosition(
        List<Vector2Int> attachable,
        List<Vector2Int> occupied)
    {
        Vector2Int bestPos =
            Vector2Int.zero;

        int maxDistance = -1;

        foreach (Vector2Int pos in attachable)
        {
            foreach (Vector2Int dir in directions)
            {
                Vector2Int candidate =
                    pos + dir;

                if (occupied.Contains(
                    candidate))
                    continue;

                int distance =
                    Mathf.Abs(candidate.x) +
                    Mathf.Abs(candidate.y);

                if (distance > maxDistance)
                {
                    maxDistance =
                        distance;

                    bestPos =
                        candidate;
                }
            }
        }

        return bestPos;
    }

    // ============================================================
    // Chest Room
    // ============================================================

    private Vector2Int GetChestRoomPosition(
        List<Vector2Int> attachable,
        List<Vector2Int> occupied)
    {
        int targetDistance = 3;

        int closestDiff = 9999;

        List<Vector2Int> candidates =
            new List<Vector2Int>();

        foreach (Vector2Int pos in attachable)
        {
            foreach (Vector2Int dir in directions)
            {
                Vector2Int candidate =
                    pos + dir;

                if (occupied.Contains(
                    candidate))
                    continue;

                int distance =
                    Mathf.Abs(candidate.x) +
                    Mathf.Abs(candidate.y);

                int diff =
                    Mathf.Abs(
                        distance -
                        targetDistance
                    );

                if (diff < closestDiff)
                {
                    closestDiff =
                        diff;

                    candidates.Clear();

                    candidates.Add(
                        candidate
                    );
                }
                else if (diff == closestDiff)
                {
                    candidates.Add(
                        candidate
                    );
                }
            }
        }

        if (candidates.Count > 0)
        {
            return candidates[
                Random.Range(
                    0,
                    candidates.Count
                )
            ];
        }

        return GetFarthestEndPosition(
            attachable,
            occupied
        );
    }

    // ============================================================
    // Opposite
    // ============================================================

    private int GetOppositeIndex(
        int index)
    {
        if (index == 0) return 1;
        if (index == 1) return 0;
        if (index == 2) return 3;
        if (index == 3) return 2;

        return 0;
    }
}