using System.Collections.Generic;
using UnityEngine;

public class RoomData : MonoBehaviour
{
    [Header("방 정보 설정")]
    public int roomType = 1;

    // DungeonGenerator가 일반방 프리팹의 몇 번째인지 기록
    public int roomPrefabIndex = -1;

    public bool isCleared = false;

    private bool isPlayerEntered = false;
    private bool isLoadingRoom = false;

    [Header("방 연결 및 스폰 포인트")]
    public Transform[] portalPoints;
    public Transform[] spawnPoints;

    [Header("보상(무기) 설정")]
    public GameObject[] weaponPrefabs;

    private int totalEnemies = 0;
    private int currentKilledEnemies = 0;

    private List<GameObject> activeEnemies =
        new List<GameObject>();

    // ============================================================
    // Awake
    // ============================================================

    private void Awake()
    {
        if (roomType == 1 ||
            roomType == 5)
        {
            isCleared = true;

            SpawnRandomWeapon();
        }
    }

    // ============================================================
    // Weapon Reward
    // ============================================================

    private void SpawnRandomWeapon()
    {
        if (weaponPrefabs == null ||
            weaponPrefabs.Length == 0)
        {
            Debug.LogWarning(
                $"[RoomData] {name}의 weaponPrefabs가 비어있습니다."
            );

            return;
        }

        int index =
            Random.Range(
                0,
                weaponPrefabs.Length
            );

        GameObject selected =
            weaponPrefabs[index];

        if (selected == null)
            return;

        Vector3 spawnPos =
            transform.position;

        if (roomType == 1)
        {
            spawnPos +=
                new Vector3(
                    0f,
                    1.5f,
                    0f
                );
        }

        spawnPos.z = 0f;

        Instantiate(
            selected,
            spawnPos,
            Quaternion.identity,
            transform
        );
    }

    // ============================================================
    // Player Enter
    // ============================================================

    private void OnTriggerEnter2D(
        Collider2D collision)
    {
        if (!isPlayerEntered &&
            collision.CompareTag("Player"))
        {
            isPlayerEntered = true;

            OnPlayerEnterRoom();
        }
    }

    // ============================================================
    // Spawn
    // ============================================================

    private void OnPlayerEnterRoom()
    {
        // 로드 중이면 자동 생성 금지
        if (isLoadingRoom)
            return;

        // 시작방 / 상자방
        if (roomType == 1 ||
            roomType == 5)
        {
            SetRoomCleared();
            return;
        }

        // 이미 클리어한 방
        if (isCleared)
            return;

        if (MonsterPool.Instance == null)
        {
            Debug.LogError(
                "[RoomData] MonsterPool이 없습니다."
            );

            return;
        }

        if (spawnPoints == null ||
            spawnPoints.Length == 0)
        {
            SetRoomCleared();
            return;
        }

        List<Transform> validPoints =
            new List<Transform>();

        foreach (Transform point in spawnPoints)
        {
            if (point != null)
                validPoints.Add(point);
        }

        if (validPoints.Count == 0)
        {
            SetRoomCleared();
            return;
        }

        // --------------------------------------------------------
        // 보스방
        // --------------------------------------------------------

        if (roomType == 6)
        {
            totalEnemies = 1;
            currentKilledEnemies = 0;

            GameObject boss =
                MonsterPool.Instance.GetMonster(
                    2,
                    validPoints[0].position
                );

            RegisterMonster(boss);
        }

        // --------------------------------------------------------
        // 일반방
        // --------------------------------------------------------

        else
        {
            totalEnemies =
                Random.Range(
                    2,
                    validPoints.Count + 1
                );

            currentKilledEnemies = 0;

            for (int i = 0;
                 i < totalEnemies;
                 i++)
            {
                if (validPoints.Count == 0)
                    break;

                int randomIndex =
                    Random.Range(
                        0,
                        validPoints.Count
                    );

                int monsterType =
                    Random.Range(0, 2);

                GameObject monster =
                    MonsterPool.Instance.GetMonster(
                        monsterType,
                        validPoints[randomIndex].position
                    );

                validPoints.RemoveAt(
                    randomIndex
                );

                RegisterMonster(monster);
            }
        }
    }

    // ============================================================
    // Register
    // ============================================================

    private void RegisterMonster(
        GameObject monster)
    {
        if (monster == null)
            return;

        if (!activeEnemies.Contains(monster))
        {
            activeEnemies.Add(monster);
        }

        EntityHealth health =
            monster.GetComponent<EntityHealth>();

        if (health != null)
        {
            health.onDeath -= OnEnemyDied;
            health.onDeath += OnEnemyDied;
        }
    }

    public void RegisterRestoredMonster(
        GameObject monster)
    {
        RegisterMonster(monster);
    }

    // ============================================================
    // Death
    // ============================================================

    private void OnEnemyDied()
    {
        currentKilledEnemies++;

        CleanupDeadEnemies();

        if (currentKilledEnemies >= totalEnemies)
        {
            SetRoomCleared();
        }
    }

    private void CleanupDeadEnemies()
    {
        for (int i = activeEnemies.Count - 1;
             i >= 0;
             i--)
        {
            GameObject monster =
                activeEnemies[i];

            if (monster == null ||
                !monster.activeInHierarchy)
            {
                activeEnemies.RemoveAt(i);
            }
        }
    }

    // ============================================================
    // Get Active Enemies
    // ============================================================

    public List<GameObject> GetActiveEnemies()
    {
        CleanupDeadEnemies();

        return activeEnemies;
    }

    // ============================================================
    // Clear Current Enemies
    // ============================================================

    public void ClearCurrentEnemies()
    {
        for (int i = activeEnemies.Count - 1;
             i >= 0;
             i--)
        {
            GameObject monster =
                activeEnemies[i];

            if (monster == null)
                continue;

            int type =
                GetMonsterType(monster);

            if (MonsterPool.Instance != null)
            {
                MonsterPool.Instance.ReturnMonster(
                    type,
                    monster
                );
            }
            else
            {
                Destroy(monster);
            }
        }

        activeEnemies.Clear();

        totalEnemies = 0;
        currentKilledEnemies = 0;
    }

    // ============================================================
    // Monster Type
    // ============================================================

    private int GetMonsterType(
        GameObject monster)
    {
        if (monster == null)
            return -1;

        if (monster.GetComponent<EnemyMelee>() != null)
            return 0;

        if (monster.GetComponent<EnemyRanged>() != null)
            return 1;

        if (monster.GetComponent<EnemyBoss>() != null)
            return 2;

        return -1;
    }

    // ============================================================
    // Load
    // ============================================================

    public void PrepareForLoad()
    {
        isLoadingRoom = true;

        ClearCurrentEnemies();

        isPlayerEntered = true;
    }

    public void FinishLoadingRoom()
    {
        isLoadingRoom = false;
    }

    // ============================================================
    // Clear Room
    // ============================================================

    public void SetRoomCleared()
    {
        if (isCleared)
            return;

        isCleared = true;

        totalEnemies = 0;

        if (DungeonGenerator.Instance != null)
        {
            DungeonGenerator.Instance.OnRoomCleared(
                this
            );
        }
    }
}