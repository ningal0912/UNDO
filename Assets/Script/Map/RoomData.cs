using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomData : MonoBehaviour
{
    [Header("방 기본 정보")]
    public int roomType; // 1: 시작방, 2: 일반방, 5: 상자방, 6: 보스방
    public Transform[] spawnPoints;  // 몬스터 스폰 위치들
    public Transform[] portalPoints; // [0]: 상, [1]: 하, [2]: 좌, [3]: 우

    [HideInInspector] public int totalEnemies;
    [HideInInspector] public int killedEnemies;
    [HideInInspector] public bool isCleared = false;

    private bool isPlayerEntered = false;
    public List<GameObject> activeMonsters = new List<GameObject>();

    void Start()
    {
        // 시작방(1번)인 경우 게임 시작과 동시에 클리어 처리하여 포탈 생성
        if (roomType == 1)
        {
            SetRoomCleared();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 플레이어 최초 진입 감지
        if (!isPlayerEntered && collision.CompareTag("Player"))
        {
            isPlayerEntered = true;
            OnPlayerEnterRoom();
        }
    }

    private void OnPlayerEnterRoom()
    {
        // 시작방(1)이나 상자방(5)은 진입 즉시 클리어 처리
        if (roomType == 1 || roomType == 5)
        {
            SetRoomCleared();
            return;
        }

        if (isCleared) return;

        // 몬스터 스폰 로직
        if (roomType == 6) // 보스방
        {
            totalEnemies = 1;
            Vector3 spawnPos = (spawnPoints != null && spawnPoints.Length > 0) ? spawnPoints[0].position : transform.position;
            GameObject boss = MonsterPool.Instance.GetMonster(2, spawnPos);
            RegisterMonster(boss, 2);
        }
        else // 일반방
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                SetRoomCleared();
                return;
            }

            totalEnemies = Random.Range(2, spawnPoints.Length + 1);
            List<Transform> points = new List<Transform>(spawnPoints);

            for (int i = 0; i < totalEnemies; i++)
            {
                if (points.Count == 0) break;
                int randIndex = Random.Range(0, points.Count);
                int mobType = Random.Range(0, 2); // 0: 근거리, 1: 원거리

                GameObject mob = MonsterPool.Instance.GetMonster(mobType, points[randIndex].position);
                points.RemoveAt(randIndex);
                RegisterMonster(mob, mobType);
            }
        }
    }

    private void RegisterMonster(GameObject mob, int mobType)
    {
        if (mob == null) return;

        activeMonsters.Add(mob);

        EntityHealth health = mob.GetComponent<EntityHealth>();
        if (health != null)
        {
            // 사망 시 콜백 연결
            health.onDeath += () => OnEnemyKilled(mob, mobType);
        }
    }

    public void OnEnemyKilled(GameObject mob, int mobType)
    {
        killedEnemies++;
        MonsterPool.Instance.ReturnMonster(mobType, mob);

        // 모든 적 처치 시 방 클리어
        if (killedEnemies >= totalEnemies)
        {
            SetRoomCleared();
        }
    }

    private void SetRoomCleared()
    {
        isCleared = true;
        if (DungeonGenerator.Instance != null)
        {
            DungeonGenerator.Instance.OnRoomCleared(this);
        }
    }
}