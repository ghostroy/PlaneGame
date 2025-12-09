using UnityEngine;
using UnityEngine.UI; 
using System.Collections.Generic;

public class EnemyHealth : MonoBehaviour
{
    // ==========================================
    // 1. 需要在 Inspector 配置的 (只剩引用了)
    // ==========================================
    [Header("配置与身份")]
    public int enemyID = 1001; // 这是唯一需要填数字的地方，用于查表

    [Header("UI 与 特效引用")]
    public Image healthBarFill;       
    public GameObject healthBarCanvas;
    public GameObject hitEffectPrefab;
    public GameObject dieEffectPrefab;

    [Header("必掉金币引用")]
    public GameObject goldPrefab; // 只拖 Prefab，数量由表格决定

    // ==========================================
    // 2. 运行时数据 (全部私有化，面板不可见)
    // ==========================================
    private int maxHealth;
    private int currentHealth;
    private int scoreValue;
    private int minGoldDrops;
    private int maxGoldDrops;
    private float extraDropChance;
    
    private List<LootItem> currentLootList; 

    // 波次奖励标记
    private bool isWaveBonusTarget = false;

    // 内部类定义
    [System.Serializable]
    public class LootItem
    {
        public string name;
        public GameObject prefab;
        [Range(1, 100)] public int weight;
    }

    // ==========================================
    // 3. 逻辑代码
    // ==========================================

    void Start()
    {
        // 1. 从 CSV 读取所有数值
        InitDataFromCSV(); 

        // 2. 初始化血条
        UpdateHealthBar();
        if (healthBarCanvas != null) healthBarCanvas.SetActive(true);
    }

    void InitDataFromCSV()
    {
        if (DataManager.Instance == null) return;
        var config = DataManager.Instance.GetEnemyConfig(enemyID);

        if (config != null)
        {
            // === 全权由表格接管 ===
            this.maxHealth = config.hp;
            this.currentHealth = config.hp; // 满血初始化
            this.scoreValue = config.score; // 分数
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
            Debug.LogError($"❌ 严重错误：找不到 ID {enemyID} 的配置！敌人将无法正常运作。");
            // 只有出错时给个保底值，防止除以0报错
            maxHealth = 1; currentHealth = 1; 
        }
    }

    void Update()
    {
        // 锁定血条旋转
        if (healthBarCanvas != null && healthBarCanvas.activeSelf)
        {
            healthBarCanvas.transform.rotation = Quaternion.identity;
        }
    }

    public void SetWaveBonusTarget(bool isBonus)
    {
        this.isWaveBonusTarget = isBonus;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (healthBarCanvas != null) healthBarCanvas.SetActive(true);
        UpdateHealthBar();

        if (hitEffectPrefab != null) 
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHealthBar()
    {
        if (healthBarFill != null && maxHealth > 0)
        {
            float fillAmount = (float)currentHealth / maxHealth;
            healthBarFill.fillAmount = fillAmount;
        }
    }

    void Die()
    {
        // 使用表格读来的分数
        if (GameManager.Instance != null) 
            GameManager.Instance.AddScore(scoreValue);

        if (dieEffectPrefab != null) 
            Instantiate(dieEffectPrefab, transform.position, Quaternion.identity);

        DropGold();
        DropExtraLoot();

        Destroy(gameObject);
    }

    void DropGold()
    {
        if (goldPrefab == null) return;
        // 使用表格读来的数量
        int count = Random.Range(minGoldDrops, maxGoldDrops + 1);
        for (int i = 0; i < count; i++) Instantiate(goldPrefab, transform.position, Quaternion.identity);
    }

    void DropExtraLoot()
    {
        if (currentLootList == null || currentLootList.Count == 0) return;

        bool shouldDrop = false;

        // 判定：是波次奖励怪？ OR 随机判定通过？
        if (isWaveBonusTarget)
        {
            shouldDrop = true;
            Debug.Log("🎁 波次奖励触发！");
        }
        else
        {
            // 使用表格读来的概率
            if (Random.value <= extraDropChance) shouldDrop = true;
        }

        if (shouldDrop)
        {
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
}