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

    // 【新增】防止重复死亡的锁
    private bool isDead = false;

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
        // 如果已经死透了，就不要鞭尸了，直接返回
        if (isDead) return;

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
        // 【核心修改】双重保险：如果已经标记为死亡，立刻停止
        if (isDead) return;
        
        // 标记为已死，后续的子弹打上来也不会再触发 Die() 了
        isDead = true;

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

        // 计算掉落数量
        int count = Random.Range(minGoldDrops, maxGoldDrops + 1);
        
        // === 【新增】判断是否需要大范围爆炸 ===
        // 如果掉落数量大于 10 (说明是 Boss 或精英)，就让金币飞得更快更远
        // 普通小兵掉 1-3 个，不需要炸太远
        float speedMultiplier = (count >= 6) ? 2.5f : 1.0f;

        for (int i = 0; i < count; i++)
        {
            GameObject coin = Instantiate(goldPrefab, transform.position, Quaternion.identity);
            
            // 获取金币上的 PowerUp 脚本，修改其初始速度
            PowerUp p = coin.GetComponent<PowerUp>();
            if (p != null)
            {
                // 乘上倍率，Boss 的金币会瞬间炸满全屏！
                // 并且加一点随机性 (0.8 ~ 1.2)，让它们不要排得太整齐
                p.initialSpeed *= speedMultiplier * Random.Range(0.8f, 1.2f);
            }
        }
    }

    void DropExtraLoot()
    {
        // 1. 安全检查：如果池子是空的，直接返回
        if (currentLootList == null || currentLootList.Count == 0) return;

        // 2. === 第一步：决定“是否”掉落 ===
        bool shouldDrop = false;

        if (isWaveBonusTarget)
        {
            // A. 如果是波次奖励怪：100% 掉落
            shouldDrop = true;
            Debug.Log("🎁 触发波次必掉奖励！");
        }
        else
        {
            // B. 如果是普通怪：根据表格里的概率 (ExtraChance) 判定
            if (Random.value <= extraDropChance)
            {
                shouldDrop = true;
            }
        }

        // 如果判定结果是不掉，直接结束，什么都不给
        if (!shouldDrop) return;


        // 3. === 第二步：决定“掉哪一个” (权重随机) ===
        // 这是一个“排他性”的选择，只会选中一个

        // A. 算出总权重 (分母)
        int totalWeight = 0;
        foreach (var item in currentLootList)
        {
            totalWeight += item.weight;
        }

        // B. 随机取一个值 (指针)
        int randomValue = Random.Range(0, totalWeight);

        // C. 遍历列表，看指针落在谁的区间里
        foreach (var item in currentLootList)
        {
            if (randomValue < item.weight)
            {
                // 🎯 选中了！生成这唯一的道具
                Instantiate(item.prefab, transform.position, Quaternion.identity);
                
                // 🛑 【关键】立即返回！
                // 这行 return 保证了循环结束，绝对不会再掉第二个
                return; 
            }
            
            // 没选中，减去当前权重，继续问下一个
            randomValue -= item.weight;
        }
    }
}