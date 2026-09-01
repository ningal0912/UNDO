using System.Collections.Generic;
using UnityEngine;

public class MonsterPool : MonoBehaviour
{
    public static MonsterPool Instance;

    public GameObject meleePrefab;
    public GameObject rangedPrefab;
    public GameObject bossPrefab;

    private Queue<GameObject> meleePool = new Queue<GameObject>();
    private Queue<GameObject> rangedPool = new Queue<GameObject>();

    void Awake()
    {
        Instance = this;
    }

    public GameObject GetMonster(int type, Vector3 position)
    {
        GameObject monster = null;
        Queue<GameObject> targetPool = (type == 0) ? meleePool : rangedPool;
        GameObject prefab = (type == 0) ? meleePrefab : (type == 1 ? rangedPrefab : bossPrefab);

        if (type == 2) // 보스는 풀링 없이 단일 생성
        {
            return Instantiate(bossPrefab, position, Quaternion.identity);
        }

        if (targetPool.Count > 0)
        {
            monster = targetPool.Dequeue();
            monster.transform.position = position;
            monster.SetActive(true);
        }
        else
        {
            monster = Instantiate(prefab, position, Quaternion.identity);
        }

        return monster;
    }

    public void ReturnMonster(int type, GameObject monster)
    {
        monster.SetActive(false);
        if (type == 0) meleePool.Enqueue(monster);
        else if (type == 1) rangedPool.Enqueue(monster);
    }
}