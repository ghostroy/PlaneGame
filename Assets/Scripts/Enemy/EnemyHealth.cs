using UnityEngine;
using System.Collections.Generic;

public class EnemyHealth : MonoBehaviour
{
    [Header("身份设置")]
    public int enemyID = 1001; // 对应 CSV 里的 ID
    
    // 这些变量在 Start 时会被 CSV 数据覆盖
    private int minGold;
    private int maxGold;
    private float extraChance;
    
    // === 【修复点1】重新加上 LootItem 的定义 ===
    [System.Serializable]
    public class LootItem
    {
        public string name;
        public GameObject prefab;
        [Range(1, 100)] 
        public int weight;
    }
    // ==========================================

    private List<LootItem> currentLootList; 

    [Header("基础引用")]
    public GameObject goldPrefab; // 记得拖拽金币预制体
    public GameObject hitEffectPrefab;
    public GameObject dieEffectPrefab;
    
    private int currentHealth = 3; 

    void Start()
    {
        InitDataFromCSV();
    }

    void InitDataFromCSV()
    {
        if (DataManager.Instance == null) return;

        // 根据 ID 查表
        var config = DataManager.Instance.GetEnemyConfig(enemyID);

        if (config != null)
        {
            this.minGold = config.minGold;
            this.maxGold = config.maxGold;
            this.extraChance = config.extraChance;

            // 根据 PoolID 去 LootManager 拿 Prefab 列表
            if (LootManager.Instance != null)
            {
                this.currentLootList = LootManager.Instance.GetLootList(config.poolID);
            }
        }
        else
        {
            Debug.LogError($"未找到 ID 为 {enemyID} 的配置数据！使用默认值。");
            minGold = 1; maxGold = 1; extraChance = 0;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (hitEffectPrefab != null) Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        if (GameManager.Instance != null) GameManager.Instance.AddScore(100);
        if (dieEffectPrefab != null) Instantiate(dieEffectPrefab, transform.position, Quaternion.identity);

        // 1. 爆金币
        DropGold();

        // 2. 掉道具
        DropExtraLoot();

        Destroy(gameObject);
    }

    void DropGold()
    {
        if (goldPrefab == null) return;
        int count = Random.Range(minGold, maxGold + 1);
        for (int i = 0; i < count; i++) Instantiate(goldPrefab, transform.position, Quaternion.identity);
    }

    void DropExtraLoot()
    {
        if (currentLootList == null || currentLootList.Count == 0) return;
        
        // 判定几率
        if (Random.value > extraChance) return; 

        // 权重随机算法
        int totalWeight = 0;
        foreach (var item in currentLootList) totalWeight += item.weight;
        
        int randomValue = Random.Range(0, totalWeight);
        
        foreach (var item in currentLootList)
        {
            if (randomValue < item.weight)
            {
                Instantiate(item.prefab, transform.position, Quaternion.identity);
                return;
            }
            randomValue -= item.weight;
        }
    }
}