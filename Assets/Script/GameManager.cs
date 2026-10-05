using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

// ============================================================
// 몬스터 세이브 데이터
// ============================================================

[Serializable]
public class EnemySaveData
{
    public int enemyType;          // 0: 근거리 / 1: 원거리 / 2: 보스
    public Vector3 position;
    public float currentHealth;
}

// ============================================================
// 방 세이브 데이터
// ============================================================

[Serializable]
public class RoomSaveData
{
    public Vector2Int gridPosition;

    // 1: 시작방
    // 5: 상자방
    // 6: 보스방
    // 그 외: 일반방
    public int roomType;

    public int roomPrefabIndex;

    public bool isCleared;
}

// ============================================================
// 전체 세이브 데이터
// ============================================================

[Serializable]
public class SaveData
{
    // --------------------------------------------------------
    // 플레이어
    // --------------------------------------------------------

    public Vector3 playerPosition;
    public float playerHealth;

    public string mainWeaponPrefabName;
    public string subWeaponPrefabName;

    public int currentWeaponIndex;

    // --------------------------------------------------------
    // 플레이 시간
    // --------------------------------------------------------

    public float playTime;

    // --------------------------------------------------------
    // 현재 방
    // --------------------------------------------------------

    public Vector2Int currentRoomGridPos;

    // --------------------------------------------------------
    // 던전 전체 구조
    // --------------------------------------------------------

    public List<RoomSaveData> dungeonRooms =
        new List<RoomSaveData>();

    // --------------------------------------------------------
    // 클리어된 방
    // --------------------------------------------------------

    public List<Vector2Int> clearedRoomPositions =
        new List<Vector2Int>();

    // --------------------------------------------------------
    // 현재 방 몬스터
    // --------------------------------------------------------

    public List<EnemySaveData> currentRoomEnemies =
        new List<EnemySaveData>();
}

// ============================================================
// GameManager
// ============================================================

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("플레이 타임 상태")]
    public float currentPlayTime = 0f;
    public bool isTimerRunning = false;

    private string saveFilePath;

    // ============================================================
    // Singleton
    // ============================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        saveFilePath =
            Path.Combine(
                Application.persistentDataPath,
                "saveData.json"
            );
    }

    private void Start()
    {
        StartTimer();
    }

    private void Update()
    {
        if (isTimerRunning)
        {
            currentPlayTime += Time.deltaTime;
        }

        // 저장
        if (Input.GetKeyDown(KeyCode.P))
        {
            SaveGame();
        }

        // 로드
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadGame();
        }
    }

    // ============================================================
    // Timer
    // ============================================================

    public void StartTimer()
    {
        isTimerRunning = true;
    }

    public void StopTimer()
    {
        isTimerRunning = false;
    }

    public void ResetTimer()
    {
        currentPlayTime = 0f;
    }

    // ============================================================
    // SAVE
    // ============================================================

    public void SaveGame()
    {
        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogWarning(
                "[GameManager] Player를 찾을 수 없습니다."
            );

            return;
        }

        SaveData data =
            new SaveData();

        // --------------------------------------------------------
        // 플레이 시간
        // --------------------------------------------------------

        data.playTime =
            currentPlayTime;

        // --------------------------------------------------------
        // 플레이어 위치
        // --------------------------------------------------------

        data.playerPosition =
            player.transform.position;

        EntityHealth playerHealth =
            player.GetComponent<EntityHealth>();

        if (playerHealth != null)
        {
            data.playerHealth =
                playerHealth.currentHealth;
        }

        // --------------------------------------------------------
        // 무기
        // --------------------------------------------------------

        WeaponManager weaponManager =
            player.GetComponent<WeaponManager>();

        if (weaponManager != null)
        {
            data.currentWeaponIndex =
                weaponManager.currentWeaponIndex;

            if (weaponManager.weapons != null)
            {
                if (weaponManager.weapons.Length > 0 &&
                    weaponManager.weapons[0] != null)
                {
                    data.mainWeaponPrefabName =
                        weaponManager.weapons[0]
                            .name
                            .Replace("(Clone)", "");
                }

                if (weaponManager.weapons.Length > 1 &&
                    weaponManager.weapons[1] != null)
                {
                    data.subWeaponPrefabName =
                        weaponManager.weapons[1]
                            .name
                            .Replace("(Clone)", "");
                }
            }
        }

        // --------------------------------------------------------
        // 현재 방
        // --------------------------------------------------------

        RoomData currentRoom =
            FindActiveRoom(
                player.transform.position
            );

        if (currentRoom != null)
        {
            if (DungeonGenerator.Instance != null)
            {
                if (DungeonGenerator.Instance.TryGetGridPosition(
                    currentRoom,
                    out Vector2Int currentGridPos))
                {
                    data.currentRoomGridPos =
                        currentGridPos;
                }
            }

            // ----------------------------------------------------
            // 현재 방 몬스터
            // ----------------------------------------------------

            List<GameObject> enemies =
                currentRoom.GetActiveEnemies();

            foreach (GameObject enemy in enemies)
            {
                if (enemy == null)
                    continue;

                if (!enemy.activeInHierarchy)
                    continue;

                EntityHealth health =
                    enemy.GetComponent<EntityHealth>();

                if (health == null)
                    continue;

                if (health.currentHealth <= 0f)
                    continue;

                int enemyType =
                    GetEnemyType(enemy);

                if (enemyType == -1)
                    continue;

                EnemySaveData enemyData =
                    new EnemySaveData();

                enemyData.enemyType =
                    enemyType;

                enemyData.position =
                    enemy.transform.position;

                enemyData.currentHealth =
                    health.currentHealth;

                data.currentRoomEnemies.Add(
                    enemyData
                );
            }
        }

        // --------------------------------------------------------
        // 던전 전체 저장
        // --------------------------------------------------------

        if (DungeonGenerator.Instance != null)
        {
            DungeonGenerator.Instance
                .CreateSaveData(data);
        }

        // --------------------------------------------------------
        // JSON 저장
        // --------------------------------------------------------

        string json =
            JsonUtility.ToJson(
                data,
                true
            );

        try
        {
            File.WriteAllText(
                saveFilePath,
                json
            );

            Debug.Log(
                "[GameManager] ========================\n" +
                "세이브 완료\n" +
                $"플레이 시간 : {FormatTime(currentPlayTime)}\n" +
                $"현재 방 몬스터 : {data.currentRoomEnemies.Count}\n" +
                $"던전 방 개수 : {data.dungeonRooms.Count}\n" +
                $"경로 : {saveFilePath}\n" +
                "========================"
            );
        }
        catch (Exception e)
        {
            Debug.LogError(
                $"[GameManager] 세이브 실패: {e.Message}"
            );
        }
    }

    // ============================================================
    // LOAD
    // ============================================================

    public void LoadGame()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.LogWarning(
                "[GameManager] 저장 파일이 없습니다."
            );

            return;
        }

        string json;

        try
        {
            json =
                File.ReadAllText(
                    saveFilePath
                );
        }
        catch (Exception e)
        {
            Debug.LogError(
                $"[GameManager] 파일 읽기 실패: {e.Message}"
            );

            return;
        }

        SaveData data;

        try
        {
            data =
                JsonUtility.FromJson<SaveData>(
                    json
                );
        }
        catch (Exception e)
        {
            Debug.LogError(
                $"[GameManager] JSON 로드 실패: {e.Message}"
            );

            return;
        }

        if (data == null)
        {
            Debug.LogError(
                "[GameManager] SaveData가 null입니다."
            );

            return;
        }

        // --------------------------------------------------------
        // 플레이 시간
        // --------------------------------------------------------

        currentPlayTime =
            data.playTime;

        // --------------------------------------------------------
        // 던전 복원
        // --------------------------------------------------------

        if (DungeonGenerator.Instance != null &&
            data.dungeonRooms != null &&
            data.dungeonRooms.Count > 0)
        {
            DungeonGenerator.Instance
                .RestoreDungeon(
                    data.dungeonRooms
                );
        }

        // --------------------------------------------------------
        // Player
        // --------------------------------------------------------

        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogWarning(
                "[GameManager] Player를 찾을 수 없습니다."
            );

            return;
        }

        player.transform.position =
            data.playerPosition;

        Rigidbody2D playerRb =
            player.GetComponent<Rigidbody2D>();

        if (playerRb != null)
        {
            playerRb.linearVelocity =
                Vector2.zero;
        }

        // --------------------------------------------------------
        // Player HP
        // --------------------------------------------------------

        EntityHealth playerHealth =
            player.GetComponent<EntityHealth>();

        if (playerHealth != null)
        {
            playerHealth.currentHealth =
                data.playerHealth;
        }

        // --------------------------------------------------------
        // Weapon
        // --------------------------------------------------------

        WeaponManager weaponManager =
            player.GetComponent<WeaponManager>();

        if (weaponManager != null)
        {
            weaponManager.RestoreWeapons(
                data.mainWeaponPrefabName,
                data.subWeaponPrefabName,
                data.currentWeaponIndex
            );
        }

        // --------------------------------------------------------
        // 현재 방 찾기
        // --------------------------------------------------------

        RoomData currentRoom = null;

        if (DungeonGenerator.Instance != null)
        {
            currentRoom =
                DungeonGenerator.Instance
                    .GetRoom(
                        data.currentRoomGridPos
                    );
        }

        // --------------------------------------------------------
        // 현재 방 몬스터 복원
        // --------------------------------------------------------

        if (currentRoom != null)
        {
            currentRoom.PrepareForLoad();

            currentRoom.isCleared =
                data.dungeonRooms.Find(
                    r =>
                        r.gridPosition ==
                        data.currentRoomGridPos
                )?.isCleared ?? false;

            RestoreEnemies(
                currentRoom,
                data.currentRoomEnemies
            );

            currentRoom.FinishLoadingRoom();
        }

        Debug.Log(
            "[GameManager] ========================\n" +
            "로드 완료\n" +
            $"플레이 시간 : {FormatTime(currentPlayTime)}\n" +
            $"현재 방 : {data.currentRoomGridPos}\n" +
            $"복원 몬스터 : {data.currentRoomEnemies.Count}\n" +
            "========================"
        );
    }

    // ============================================================
    // 몬스터 복원
    // ============================================================

    private void RestoreEnemies(
        RoomData room,
        List<EnemySaveData> enemies)
    {
        if (room == null)
            return;

        room.ClearCurrentEnemies();

        if (enemies == null)
            return;

        if (MonsterPool.Instance == null)
        {
            Debug.LogError(
                "[GameManager] MonsterPool.Instance가 없습니다."
            );

            return;
        }

        foreach (EnemySaveData data in enemies)
        {
            GameObject monster =
                MonsterPool.Instance.GetMonster(
                    data.enemyType,
                    data.position
                );

            if (monster == null)
                continue;

            EntityHealth health =
                monster.GetComponent<EntityHealth>();

            if (health != null)
            {
                health.currentHealth =
                    data.currentHealth;
            }

            room.RegisterRestoredMonster(
                monster
            );
        }
    }

    // ============================================================
    // 몬스터 타입
    // ============================================================

    private int GetEnemyType(
        GameObject enemy)
    {
        if (enemy.GetComponent<EnemyMelee>() != null)
            return 0;

        if (enemy.GetComponent<EnemyRanged>() != null)
            return 1;

        if (enemy.GetComponent<EnemyBoss>() != null)
            return 2;

        return -1;
    }

    // ============================================================
    // 현재 방 찾기
    // ============================================================

    private RoomData FindActiveRoom(
        Vector3 position)
    {
        RoomData[] rooms =
            FindObjectsOfType<RoomData>();

        foreach (RoomData room in rooms)
        {
            Collider2D collider =
                room.GetComponent<Collider2D>();

            if (collider != null &&
                collider.OverlapPoint(position))
            {
                return room;
            }
        }

        return null;
    }

    // ============================================================
    // Save File
    // ============================================================

    public bool HasSaveFile()
    {
        return File.Exists(saveFilePath);
    }

    public void DeleteSaveFile()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);

            Debug.Log(
                "[GameManager] 세이브 파일 삭제 완료"
            );
        }
    }

    // ============================================================
    // Time Format
    // ============================================================

    public string FormatTime(
        float timeInSeconds)
    {
        int hours =
            Mathf.FloorToInt(
                timeInSeconds / 3600f
            );

        int minutes =
            Mathf.FloorToInt(
                (timeInSeconds % 3600f) / 60f
            );

        int seconds =
            Mathf.FloorToInt(
                timeInSeconds % 60f
            );

        if (hours > 0)
        {
            return string.Format(
                "{0:D2}:{1:D2}:{2:D2}",
                hours,
                minutes,
                seconds
            );
        }

        return string.Format(
            "{0:D2}:{1:D2}",
            minutes,
            seconds
        );
    }
}