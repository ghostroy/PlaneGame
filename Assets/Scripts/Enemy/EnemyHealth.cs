using UnityEngine;
using UnityEngine.UI; // 必须引用 UI 命名空间
using System.Collections.Generic;

public class EnemyHealth : MonoBehaviour
{
    [Header("配置与身份")]
    public int enemyID = 1001; 

    [Header("基础属性")] 
    public int maxHealth = 3;  
    public int scoreValue = 100;

    // === 【修复点 1】加回 UI 变量定义 ===
    [Header("UI 血条设置")]
    public Image healthBarFill;       // 拖入那个红色的前景 Image
    public GameObject healthBarCanvas;// 拖入整个血条 Canvas (用于满血隐藏/旋转锁定)

    [Header("特效设置")]
    public GameObject hitEffectPrefab;
    public GameObject dieEffectPrefab;

    [Header("掉落设置")]
    public GameObject goldPrefab; 
    public int minGoldDrops = 1;  
    public int maxGoldDrops = 3;
    [Range(0f, 1f)]
    public float extraDropChance = 0.2f; 

    [System.Serializable]
    public class LootItem
    {
        public string name;
        public GameObject prefab;
        [Range(1, 100)] 
        public int weight;
    }

    private List<LootItem> currentLootList; 
    private int currentHealth;

    void Start()
    {
        // 1. 读取 CSV 数据
        // 这一步会把 maxHealth 和 currentHealth 都覆盖为表格里的数值
        // 所以 Inspector 里填多少都已经不重要了
        InitDataFromCSV();

        // 2. 强制更新一次 UI
        // 确保进游戏时，血条长度是根据表格数据计算的满血状态
        UpdateHealthBar();
        
        // 3. 【核心修改】强制显示血条
        // 之前可能有代码写了 SetActive(false)，请删掉或改成 true
        if (healthBarCanvas != null) 
        {
            healthBarCanvas.SetActive(true);
        }
    }

    void InitDataFromCSV()
    {
        if (DataManager.Instance == null) return;
        var config = DataManager.Instance.GetEnemyConfig(enemyID);

        if (config != null)
        {
            // === 数据覆盖逻辑 ===
            // 这里从 CSV 读出的 hp 会直接覆盖掉 Inspector 里的设置
            this.maxHealth = config.hp; 
            
            // 关键：进游戏时不仅上限要改，当前血量也要填满！
            this.currentHealth = config.hp; 

            this.minGoldDrops = config.minGold;
            this.maxGoldDrops = config.maxGold;
            this.extraDropChance = config.extraChance;

            if (LootManager.Instance != null)
            {
                this.currentLootList = LootManager.Instance.GetLootList(config.poolID);
            }
        }
        else
        {
            Debug.LogError($"未找到 ID {enemyID} 的配置，使用 Inspector 默认值。");
        }
    }

    void Update()
    {
        // 【修复点 3】锁定血条旋转
        // 防止血条跟着飞机乱转，让它始终水平显示
        if (healthBarCanvas != null && healthBarCanvas.activeSelf)
        {
            healthBarCanvas.transform.rotation = Quaternion.identity;
        }
    }


    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // 【修复点 4】受伤时显示血条并更新长度
        if (healthBarCanvas != null) healthBarCanvas.SetActive(true);
        UpdateHealthBar();

        if (hitEffectPrefab != null) 
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // === 【修复点 5】核心 UI 更新逻辑 ===
    void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            // 注意：必须强转 (float)，否则 2/3 会变成 0
            float fillAmount = (float)currentHealth / maxHealth;
            healthBarFill.fillAmount = fillAmount;
        }
    }

    void Die()
    {
        if (GameManager.Instance != null) GameManager.Instance.AddScore(scoreValue);
        if (dieEffectPrefab != null) Instantiate(dieEffectPrefab, transform.position, Quaternion.identity);

        DropGold();
        DropExtraLoot();

        Destroy(gameObject);
    }

    // ... (掉落逻辑保持不变) ...
    void DropGold() 
    {
        if (goldPrefab == null) return;
        int count = Random.Range(minGoldDrops, maxGoldDrops + 1);
        for (int i = 0; i < count; i++) Instantiate(goldPrefab, transform.position, Quaternion.identity);
    }

    void DropExtraLoot()
    {
        if (currentLootList == null || currentLootList.Count == 0) return;
        if (Random.value > extraDropChance) return;
        
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