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

    private Dictionary<Vector2Int, RoomData> dungeonGrid = new Dictionary<Vector2Int, RoomData>();
    private Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    void Awake()
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

    void Start()
    {
        GenerateIsaacDungeon();
    }

    void GenerateIsaacDungeon()
    {
        dungeonGrid.Clear();

        // 💡 핵심 분리: '일반방' 위치만 담는 리스트와 '모든 방' 위치를 담는 리스트를 분리
        List<Vector2Int> normalRoomPositions = new List<Vector2Int>();
        List<Vector2Int> allOccupiedPositions = new List<Vector2Int>();

        Vector2Int currentPos = Vector2Int.zero;

        normalRoomPositions.Add(currentPos);
        allOccupiedPositions.Add(currentPos);

        // 1. 일반방 무작위 위치 계산
        int targetNormalCount = Mathf.Max(1, totalRooms - 2);
        while (normalRoomPositions.Count < targetNormalCount)
        {
            Vector2Int nextPos = currentPos + directions[Random.Range(0, directions.Length)];
            if (!allOccupiedPositions.Contains(nextPos))
            {
                normalRoomPositions.Add(nextPos);
                allOccupiedPositions.Add(nextPos);
            }
            currentPos = nextPos;
        }

        // 2. 시작방 생성
        SpawnRoomObject(room1_Start, Vector2Int.zero);

        // 3. 일반방들 무작위 생성
        for (int i = 1; i < normalRoomPositions.Count; i++)
        {
            if (roomNormal != null && roomNormal.Length > 0)
            {
                GameObject randNormal = roomNormal[Random.Range(0, roomNormal.Length)];
                SpawnRoomObject(randNormal, normalRoomPositions[i]);
            }
        }

        // 4. 보스방(6번) 배치 - 오직 일반방(normalRoomPositions)에만 이어지도록 탐색
        Vector2Int bossPos = GetFarthestEndPosition(normalRoomPositions, allOccupiedPositions);
        allOccupiedPositions.Add(bossPos); // 보스방 위치 점유 처리 (이후 다른 방이 겹치지 않게)
        SpawnRoomObject(room6_Boss, bossPos);

        // 5. 상자방(5번) 배치 - 오직 일반방(normalRoomPositions)에만 이어지도록 탐색
        Vector2Int chestPos = GetChestRoomPosition(normalRoomPositions, allOccupiedPositions);
        allOccupiedPositions.Add(chestPos);
        SpawnRoomObject(room5_Chest, chestPos);

        // 6. 플레이어 위치 및 속도 초기화
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = Vector3.zero;
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null) playerRb.linearVelocity = Vector2.zero;
        }

        // 7. 시작방 포탈 열기
        if (dungeonGrid.TryGetValue(Vector2Int.zero, out RoomData startRoom))
        {
            OnRoomCleared(startRoom);
        }
    }

    void SpawnRoomObject(GameObject prefab, Vector2Int gridPos)
    {
        if (prefab == null) return;

        Vector3 worldPos = new Vector3(gridPos.x * roomSize, gridPos.y * roomSize, 0f);
        GameObject roomObj = Instantiate(prefab, worldPos, Quaternion.identity, transform);

        RoomData roomData = roomObj.GetComponentInChildren<RoomData>();
        if (roomData != null)
        {
            dungeonGrid[gridPos] = roomData;
        }
    }

    // 💡 변경: 이어붙일 기준(attachable)과 빈 공간 확인용(occupied) 리스트를 받아옴
    Vector2Int GetFarthestEndPosition(List<Vector2Int> attachable, List<Vector2Int> occupied)
    {
        Vector2Int bestPos = Vector2Int.zero;
        int maxDistance = -1;

        foreach (var pos in attachable)
        {
            foreach (var dir in directions)
            {
                Vector2Int candidate = pos + dir;
                if (!occupied.Contains(candidate)) // 이미 보스방/상자방/일반방이 있는 곳은 제외
                {
                    int distance = Mathf.Abs(candidate.x) + Mathf.Abs(candidate.y);
                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                        bestPos = candidate;
                    }
                }
            }
        }
        return bestPos;
    }

    // 💡 변경: 이어붙일 기준(attachable)과 빈 공간 확인용(occupied) 리스트를 받아옴
    Vector2Int GetChestRoomPosition(List<Vector2Int> attachable, List<Vector2Int> occupied)
    {
        int targetDistance = 3;
        int closestDiff = 9999;
        List<Vector2Int> candidates = new List<Vector2Int>();

        foreach (var pos in attachable)
        {
            foreach (var dir in directions)
            {
                Vector2Int candidate = pos + dir;
                if (!occupied.Contains(candidate))
                {
                    int distance = Mathf.Abs(candidate.x) + Mathf.Abs(candidate.y);
                    int diff = Mathf.Abs(distance - targetDistance);

                    if (diff < closestDiff)
                    {
                        closestDiff = diff;
                        candidates.Clear();
                        candidates.Add(candidate);
                    }
                    else if (diff == closestDiff)
                    {
                        candidates.Add(candidate);
                    }
                }
            }
        }

        if (candidates.Count > 0)
        {
            return candidates[Random.Range(0, candidates.Count)];
        }

        return GetFarthestEndPosition(attachable, occupied);
    }

    // ==== 포탈 생성 로직 (기존과 동일) ====
    public void OnRoomCleared(RoomData currentRoom)
    {
        if (currentRoom == null || portalPrefab == null) return;

        Vector2Int currentGridPos = Vector2Int.zero;
        bool foundPos = false;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // PlayerHealth 대신 현재 사용 중인 EntityHealth 컴포넌트를 가져옵니다.
            var entityHealth = player.GetComponent<EntityHealth>();
            if (entityHealth != null)
            {
                entityHealth.Heal(30f); // 30 회복
            }
        }

        foreach (var pair in dungeonGrid)
        {
            if (pair.Value == currentRoom)
            {
                currentGridPos = pair.Key;
                foundPos = true;
                break;
            }
        }

        if (!foundPos) return;

        for (int i = 0; i < directions.Length; i++)
        {
            Vector2Int neighborPos = currentGridPos + directions[i];

            if (dungeonGrid.ContainsKey(neighborPos))
            {
                if (currentRoom.portalPoints != null &&
                    i < currentRoom.portalPoints.Length &&
                    currentRoom.portalPoints[i] != null)
                {
                    Transform pPoint = currentRoom.portalPoints[i];

                    if (pPoint.childCount == 0)
                    {
                        GameObject portalObj = Instantiate(portalPrefab, pPoint.position, Quaternion.identity, pPoint);

                        Portal portal = portalObj.GetComponent<Portal>();
                        if (portal != null)
                        {
                            RoomData neighborRoom = dungeonGrid[neighborPos];
                            int oppositeIndex = GetOppositeIndex(i);

                            if (neighborRoom.portalPoints != null &&
                                oppositeIndex < neighborRoom.portalPoints.Length &&
                                neighborRoom.portalPoints[oppositeIndex] != null)
                            {
                                Transform targetPortalPoint = neighborRoom.portalPoints[oppositeIndex];
                                Vector3 directionToCenter = (neighborRoom.transform.position - targetPortalPoint.position).normalized;
                                float spawnOffset = 1.5f;
                                portal.targetWorldPos = targetPortalPoint.position + (directionToCenter * spawnOffset);
                            }
                            else
                            {
                                portal.targetWorldPos = neighborRoom.transform.position;
                            }
                        }
                    }
                }
            }
        }
    }

    private int GetOppositeIndex(int currentIndex)
    {
        if (currentIndex == 0) return 1;
        if (currentIndex == 1) return 0;
        if (currentIndex == 2) return 3;
        if (currentIndex == 3) return 2;
        return 0;
    }
}