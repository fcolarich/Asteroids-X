using System.Collections.Generic;
using Unity.Transforms;
using UnityEngine;

public class Pooler : MonoBehaviour
{
    
    [System.Serializable]
    public class Pool
    {
        public GameObject prefab;
        public int size;
    }
    
    public static Pooler Instance;
    private void Awake()
    {
        Instance = this;
    }

    [SerializeField] private List<Pool> poolList;
    private Dictionary<string, Queue<GameObject>> poolDictionary;
    
    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (var pool in poolList)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            for (int i = 0; i < pool.size; i++)
            {
                var newObject = Instantiate(pool.prefab, this.transform);
                newObject.SetActive(false);
                objectPool.Enqueue(newObject);
            }
            poolDictionary.Add(pool.prefab.name, objectPool);
        }
    }

    public GameObject Spawn(GameObject gameObject, Vector3 position, Quaternion rotation)
    {
        var tag = gameObject.name;
        if (poolDictionary.ContainsKey(tag))
        {
            GameObject objectToSpawn = poolDictionary[tag].Dequeue();
            objectToSpawn.SetActive(false);
            objectToSpawn.SetActive(true);
            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;
            poolDictionary[tag].Enqueue(objectToSpawn);
            return objectToSpawn;
        }
        else
        {
            return null;    
        }
    }

    public GameObject Spawn(GameObject gameObject)
    {
        var tag = gameObject.name;
        if (poolDictionary.ContainsKey(tag))
        {
            GameObject objectToSpawn = poolDictionary[tag].Dequeue();
            objectToSpawn.SetActive(false);
            objectToSpawn.SetActive(true);
            poolDictionary[tag].Enqueue(objectToSpawn);
            return objectToSpawn;
        }
        else
        {
            return null;    
        }
    }
    
    public void DeSpawn(GameObject gameObject)
    {
        gameObject.SetActive(false);
    }
}


