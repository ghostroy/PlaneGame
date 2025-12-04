using UnityEngine;
using System.Collections.Generic;

public class LootManager : MonoBehaviour
{
    public static LootManager Instance;

    [System.Serializable]
    public class LootPool
    {
        public int poolID;
        public List<EnemyHealth.LootItem> items; // 复用之前的权重结构
    }

    // 在 Inspector 里配置：ID 0 是杂兵池，ID 1 是精英池...
    public List<LootPool> lootPools;

    private Dictionary<int, List<EnemyHealth.LootItem>> poolDictionary;

    void Awake()
    {
        Instance = this;
        InitializePools();
    }

    void InitializePools()
    {
        poolDictionary = new Dictionary<int, List<EnemyHealth.LootItem>>();
        foreach (var pool in lootPools)
        {
            if (!poolDictionary.ContainsKey(pool.poolID))
            {
                poolDictionary.Add(pool.poolID, pool.items);
            }
        }
    }

    public List<EnemyHealth.LootItem> GetLootList(int poolID)
    {
        if (poolDictionary.ContainsKey(poolID))
            return poolDictionary[poolID];
        return null;
    }
}