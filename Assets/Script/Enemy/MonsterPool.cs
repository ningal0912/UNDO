using System.Collections.Generic;
using UnityEngine;

public class MonsterPool : MonoBehaviour
{
    public static MonsterPool Instance;

    [Header("몬스터 프리팹 설정")]
    public GameObject meleePrefab;  // 0: 근거리 몬스터
    public GameObject rangedPrefab; // 1: 원거리 몬스터
    public GameObject bossPrefab;   // 2: 보스 몬스터

    private Queue<GameObject> meleePool = new Queue<GameObject>();
    private Queue<GameObject> rangedPool = new Queue<GameObject>();

    void Awake()
    {
        // 싱글톤 초기화
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public GameObject GetMonster(int type, Vector3 position)
    {
        GameObject monster = null;

        // 보스는 풀링 없이 단일 생성
        if (type == 2)
        {
            if (bossPrefab == null) return null;
            return Instantiate(bossPrefab, position, Quaternion.identity);
        }

        Queue<GameObject> targetPool = (type == 0) ? meleePool : rangedPool;
        GameObject prefab = (type == 0) ? meleePrefab : rangedPrefab;

        if (prefab == null)
        {
            Debug.LogError($"MonsterPool: type {type}에 해당하는 프리팹이 등록되지 않았습니다.");
            return null;
        }

        // 풀에 재활용 가능한 오브젝트가 남아있는 경우
        if (targetPool.Count > 0)
        {
            monster = targetPool.Dequeue();
            monster.transform.position = position;
            monster.SetActive(true);
        }
        else // 풀이 비어있으면 새롭게 생성
        {
            monster = Instantiate(prefab, position, Quaternion.identity);
        }

        return monster;
    }

    public void ReturnMonster(int type, GameObject monster)
    {
        if (monster == null) return;

        monster.SetActive(false);

        if (type == 0) meleePool.Enqueue(monster);
        else if (type == 1) rangedPool.Enqueue(monster);
        else if (type == 2) Destroy(monster); // 보스는 풀에 넣지 않고 파괴
    }
}