using System.Collections.Generic;
using UnityEngine;

public class RoomData : MonoBehaviour
{
    [Header("방 정보 설정")]
    public int roomType = 1;
    public bool isCleared = false;
    private bool isPlayerEntered = false;

    [Header("방 연결 및 스폰 포인트")]
    public Transform[] portalPoints;
    public Transform[] spawnPoints;

    // 👇 [추가된 부분] 랜덤으로 띄울 무기 프리팹들을 담을 배열
    [Header("보상(무기) 설정")]
    public GameObject[] weaponPrefabs;

    private int totalEnemies = 0;
    private int currentKilledEnemies = 0;
    private List<GameObject> activeEnemies = new List<GameObject>();

    void Awake()
    {
        // 시작방(1번)인 경우 방 오브젝트가 생성되자마자 즉시 클리어 처리
        if (roomType == 1 || roomType == 5)
        {
            isCleared = true;
            SpawnRandomWeapon(); // 방이 생성될 때 정중앙에 무기 스폰
        }
    }
    private void SpawnRandomWeapon()
    {
        // 1단계: 함수 호출 여부 확인
        Debug.Log($"[WeaponDebug] {gameObject.name} (RoomType: {roomType})에서 무기 스폰 함수가 호출되었습니다.");

        // 2단계: 배열 할당 여부 확인
        if (weaponPrefabs == null || weaponPrefabs.Length == 0)
        {
            Debug.LogError($"[WeaponDebug] ❌ {gameObject.name}의 Weapon Prefabs 배열이 비어있습니다! 인스펙터를 확인하세요.");
            return;
        }

        int randIndex = Random.Range(0, weaponPrefabs.Length);
        GameObject selectedWeapon = weaponPrefabs[randIndex];

        // 3단계: 배열 내부의 프리팹 누락 확인
        if (selectedWeapon == null)
        {
            Debug.LogError($"[WeaponDebug] ❌ 배열의 {randIndex}번째 칸이 None(비어있음) 상태입니다.");
            return;
        }

        // 4단계: 스폰 위치 계산 및 확인
        Vector3 spawnPos = transform.position;
        if (roomType == 1)
        {
            spawnPos += new Vector3(0f, 1.5f, 0f);
        }
        spawnPos.z = 0f; // Z축 문제 방지용 강제 0 설정

        Debug.Log($"[WeaponDebug] 무기({selectedWeapon.name})를 {spawnPos} 위치에 생성을 시도합니다...");

        // 5단계: 실제 생성 및 하이어라키(Hierarchy) 등록 확인
        GameObject spawnedWeapon = Instantiate(selectedWeapon, spawnPos, Quaternion.identity, transform);

        if (spawnedWeapon != null)
        {
            Debug.Log($"[WeaponDebug] ✅ 무기 생성 성공! 생성된 오브젝트: {spawnedWeapon.name}, 활성화 상태: {spawnedWeapon.activeInHierarchy}");
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isPlayerEntered && collision.CompareTag("Player"))
        {
            isPlayerEntered = true;
            OnPlayerEnterRoom();
        }
    }

    private void OnPlayerEnterRoom()
    {
        // 시작방(1)이나 상자방(5)은 몬스터 생성 없이 클리어
        if (roomType == 1 || roomType == 5)
        {
            SetRoomCleared();
            return;
        }

        if (isCleared) return;

        if (MonsterPool.Instance == null)
        {
            Debug.LogError("[RoomData] 씬에 MonsterPool 인스턴스가 존재하지 않습니다!");
            return;
        }

        // 일반방/보스방 몬스터 스폰 로직
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            SetRoomCleared();
            return;
        }

        List<Transform> validPoints = new List<Transform>();
        foreach (var p in spawnPoints)
        {
            if (p != null) validPoints.Add(p);
        }

        if (validPoints.Count == 0)
        {
            SetRoomCleared();
            return;
        }

        if (roomType == 6) // 보스방
        {
            totalEnemies = 1;
            GameObject boss = MonsterPool.Instance.GetMonster(2, validPoints[0].position);
            RegisterMonster(boss);
        }
        else // 일반방
        {
            totalEnemies = Random.Range(2, validPoints.Count + 1);
            for (int i = 0; i < totalEnemies; i++)
            {
                if (validPoints.Count == 0) break;
                int randIndex = Random.Range(0, validPoints.Count);
                int mobType = Random.Range(0, 2);

                GameObject mob = MonsterPool.Instance.GetMonster(mobType, validPoints[randIndex].position);
                validPoints.RemoveAt(randIndex);
                RegisterMonster(mob);
            }
        }
    }

    private void RegisterMonster(GameObject mob)
    {
        if (mob == null) return;

        activeEnemies.Add(mob);
        EntityHealth health = mob.GetComponent<EntityHealth>();
        if (health != null)
        {
            // System.Action 이벤트 구독 해제 후 재등록 (-=, += 연산자 사용)
            health.onDeath -= OnEnemyDied;
            health.onDeath += OnEnemyDied;
        }
    }

    private void OnEnemyDied()
    {
        currentKilledEnemies++;
        if (currentKilledEnemies >= totalEnemies)
        {
            SetRoomCleared();
        }
    }

    public void SetRoomCleared()
    {
        isCleared = true;

        // DungeonGenerator에 포탈 생성 요청
        if (DungeonGenerator.Instance != null)
        {
            DungeonGenerator.Instance.OnRoomCleared(this);
        }
        else
        {
            // 최신 Unity 권장 방식인 FindFirstObjectByType 또는 FindAnyObjectByType 사용
            DungeonGenerator generator = FindFirstObjectByType<DungeonGenerator>();
            if (generator != null)
            {
                generator.OnRoomCleared(this);
            }
            else
            {
                Debug.LogError("[RoomData] DungeonGenerator 인스턴스를 찾을 수 없습니다!");
            }
        }
    }
}