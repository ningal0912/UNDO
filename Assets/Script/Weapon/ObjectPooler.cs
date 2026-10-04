using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;

    private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();

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

    // 기본 SpawnFromPool (4개 인자)
    public GameObject SpawnFromPool(string poolTag, GameObject prefab, Vector3 position, Quaternion rotation)
    {
        string key = poolTag + "_" + prefab.name;

        if (!poolDictionary.ContainsKey(key))
        {
            poolDictionary[key] = new Queue<GameObject>();
        }

        if (poolDictionary[key].Count > 0)
        {
            GameObject obj = poolDictionary[key].Dequeue();
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
            return obj;
        }
        else
        {
            GameObject obj = Instantiate(prefab, position, rotation);
            obj.name = prefab.name;
            return obj;
        }
    }

    // 💡 안전장치 오버로딩: rotation을 안 적었을 때 기본 회전값(Quaternion.identity) 적용
    public GameObject SpawnFromPool(string poolTag, GameObject prefab, Vector3 position)
    {
        return SpawnFromPool(poolTag, prefab, position, Quaternion.identity);
    }

    public void ReturnToPool(string poolTag, GameObject obj)
    {
        obj.SetActive(false);
        string key = poolTag + "_" + obj.name;

        if (!poolDictionary.ContainsKey(key))
        {
            poolDictionary[key] = new Queue<GameObject>();
        }

        poolDictionary[key].Enqueue(obj);
    }
}