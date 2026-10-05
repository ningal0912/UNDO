using System.Collections.Generic;
using UnityEngine;

public class MonsterPool : MonoBehaviour
{
    public static MonsterPool Instance;

    [Header("몬스터 프리팹 설정")]
    public GameObject meleePrefab;
    public GameObject rangedPrefab;
    public GameObject bossPrefab;

    private Queue<GameObject> meleePool =
        new Queue<GameObject>();

    private Queue<GameObject> rangedPool =
        new Queue<GameObject>();

    // ============================================================
    // Awake
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
    // Get Monster
    // ============================================================

    public GameObject GetMonster(
        int type,
        Vector3 position)
    {
        // --------------------------------------------------------
        // Boss
        // --------------------------------------------------------

        if (type == 2)
        {
            if (bossPrefab == null)
            {
                Debug.LogError(
                    "[MonsterPool] bossPrefab이 없습니다."
                );

                return null;
            }

            GameObject boss =
                Instantiate(
                    bossPrefab,
                    position,
                    Quaternion.identity
                );

            boss.SetActive(true);

            return boss;
        }

        // --------------------------------------------------------
        // 일반 몬스터
        // --------------------------------------------------------

        GameObject prefab =
            type == 0
                ? meleePrefab
                : rangedPrefab;

        Queue<GameObject> pool =
            type == 0
                ? meleePool
                : rangedPool;

        if (prefab == null)
        {
            Debug.LogError(
                $"[MonsterPool] type {type} 프리팹이 없습니다."
            );

            return null;
        }

        GameObject monster = null;

        while (pool.Count > 0 &&
               monster == null)
        {
            monster =
                pool.Dequeue();
        }

        if (monster == null)
        {
            monster =
                Instantiate(
                    prefab,
                    position,
                    Quaternion.identity
                );
        }
        else
        {
            monster.transform.position =
                position;

            monster.transform.rotation =
                Quaternion.identity;

            monster.SetActive(true);
        }

        return monster;
    }

    // ============================================================
    // Return
    // ============================================================

    public void ReturnMonster(
        int type,
        GameObject monster)
    {
        if (monster == null)
            return;

        // Boss는 풀링하지 않음
        if (type == 2)
        {
            Destroy(monster);
            return;
        }

        monster.SetActive(false);

        if (type == 0)
        {
            meleePool.Enqueue(monster);
        }
        else if (type == 1)
        {
            rangedPool.Enqueue(monster);
        }
        else
        {
            Destroy(monster);
        }
    }

    // ============================================================
    // Clear Pool
    // ============================================================

    public void ClearPool()
    {
        while (meleePool.Count > 0)
        {
            GameObject monster =
                meleePool.Dequeue();

            if (monster != null)
                Destroy(monster);
        }

        while (rangedPool.Count > 0)
        {
            GameObject monster =
                rangedPool.Dequeue();

            if (monster != null)
                Destroy(monster);
        }
    }
}