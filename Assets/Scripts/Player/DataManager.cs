using UnityEngine;
using System.Collections.Generic;
using System;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    [Header("配置数据 (请拖入 Assets/Data/EnemyDropData)")]
    public TextAsset enemyDropCsvFile; // 掉落表
    public TextAsset itemDataCsvFile; // 物品列表

    [Header("玩家存档")]
    public int weaponUpgradeLevel = 1; // 场外武器等级
    public int gold = 0;               // 金币数量

    // --- 数据表存储 (Key: EnemyID, Value: 配置) ---
    public Dictionary<int, EnemyDropConfig> dropTableMap = new Dictionary<int, EnemyDropConfig>();

    // 数据结构类
    public class EnemyDropConfig
    {
        public int id;            // 敌人ID (1001)
        public string name;       // 名字
        public int hp;            // 敌人血量
        public int score;         // 敌人分数
        public int minGold;       // 最少金币
        public int maxGold;       // 最多金币
        public float extraChance; // 额外掉落率 (0.0 ~ 1.0)
        public int poolID;        // 掉落池ID
    }

     public class ItemConfig
    {
        public int id;
        public string name;
        public ItemType type; // 枚举类型
        public int value;     // 数值
    }

    // 道具数据字典
    public Dictionary<int, ItemConfig> itemTableMap = new Dictionary<int, ItemConfig>();

    void Awake()
    {
        // 单例模式初始化
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 切换场景不销毁
            
            LoadData();      // 1. 读取玩家金币和等级
            LoadCSVTable();  // 2. 读取 CSV 配置表
            LoadItemTable(); // 3. 读取道具表
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // =========================================================
    // 1. CSV 表格读取逻辑 (增强版：修复格式错误)
    // =========================================================
    void LoadCSVTable()
    {
        if (enemyDropCsvFile == null)
        {
            Debug.LogError("❌ CSV 文件未绑定！请在 GameManager 物体上的 DataManager 组件中拖入 CSV 文件。");
            return;
        }

        string csvContent = enemyDropCsvFile.text;

        // 兼容 Windows(\r\n) 和 Mac/Linux(\n) 的换行符
        string[] lines = csvContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        // 从第 1 行开始循环 (跳过第 0 行的表头: ID,Name...)
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];

            // 跳过空行
            if (string.IsNullOrWhiteSpace(line)) continue;

            // 清理可能存在的 BOM 字符 (防止第1行解析失败)
            if (i == 1) line = line.TrimStart('\uFEFF');

            // 按逗号分割
            string[] values = line.Split(',');

            // 安全检查：如果列数少于6，说明格式不对
            if (values.Length < 6)
            {
                Debug.LogWarning($"⚠️ 第 {i} 行格式错误 (列数不足): '{line}'");
                continue;
            }

            try
            {
                EnemyDropConfig config = new EnemyDropConfig();

                config.id = int.Parse(values[0].Trim()); 
                config.name = values[1].Trim();
                config.hp = int.Parse(values[2].Trim()); 
                
                // 【新增】读取第 3 列 Score
                config.score = int.Parse(values[3].Trim()); 

                // 【注意】后面的索引全部 +1
                config.minGold = int.Parse(values[4].Trim());
                config.maxGold = int.Parse(values[5].Trim());
                config.extraChance = float.Parse(values[6].Trim());
                config.poolID = int.Parse(values[7].Trim());

                if (!dropTableMap.ContainsKey(config.id))
                {
                    dropTableMap.Add(config.id, config);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ 解析失败 [第{i}行]: 原始内容='{line}'\n具体错误: {e.Message}");
            }
        }

        Debug.Log($"✅ CSV 配置表加载完毕！共加载 {dropTableMap.Count} 条敌人数据。");
    }

     void LoadItemTable()
    {
        if (itemDataCsvFile == null)
        {
            Debug.LogError("❌ 道具 CSV 未绑定！请在 DataManager 面板拖入 ItemData.csv");
            return;
        }

        string[] lines = itemDataCsvFile.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (i == 1) line = line.TrimStart('\uFEFF'); // 去除BOM

            string[] values = line.Split(',');

            try
            {
                ItemConfig config = new ItemConfig();
                config.id = int.Parse(values[0].Trim());
                config.name = values[1].Trim();
                
                // 【核心】将字符串转为枚举 (大小写必须匹配)
                string typeStr = values[2].Trim();
                config.type = (ItemType)Enum.Parse(typeof(ItemType), typeStr);

                config.value = int.Parse(values[3].Trim());

                if (!itemTableMap.ContainsKey(config.id))
                {
                    itemTableMap.Add(config.id, config);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"道具表解析错误 [第{i}行]: {e.Message}");
            }
        }
        Debug.Log($"✅ 道具表加载完毕，共 {itemTableMap.Count} 条数据。");
    }

    // 获取道具配置的接口
    public ItemConfig GetItemConfig(int itemID)
    {
        if (itemTableMap.ContainsKey(itemID))
            return itemTableMap[itemID];
        return null;
    }

    // 提供给 EnemyHealth 调用的查询方法
    public EnemyDropConfig GetEnemyConfig(int enemyID)
    {
        if (dropTableMap.ContainsKey(enemyID))
        {
            return dropTableMap[enemyID];
        }
        else
        {
            Debug.LogWarning($"⚠️ 试图查询不存在的 EnemyID: {enemyID}");
            return null;
        }
    }

    // =========================================================
    // 2. 武器伤害计算逻辑 (场外升级)
    // =========================================================
    
    // 获取当前基础伤害 (公式：基础10 + 等级加成)
    public int GetCurrentBaseDamage()
    {
        int baseDmg = 10;
        int growthPerLevel = 2; // 每级加2点攻击
        return baseDmg + (weaponUpgradeLevel - 1) * growthPerLevel;
    }

    // 升级武器 (UI按钮调用)
    public void UpgradeWeapon()
    {
        int cost = weaponUpgradeLevel * 100; // 价格公式
        if (gold >= cost)
        {
            gold -= cost;
            weaponUpgradeLevel++;
            SaveData(); // 升级后立即保存
            Debug.Log($"武器升级成功！Lv.{weaponUpgradeLevel}, 剩余金币: {gold}");
        }
        else
        {
            Debug.Log("金币不足！");
        }
    }

    // =========================================================
    // 3. 存档读写逻辑 (PlayerPrefs)
    // =========================================================
    void SaveData()
    {
        PlayerPrefs.SetInt("WeaponLevel", weaponUpgradeLevel);
        PlayerPrefs.SetInt("Gold", gold);
        PlayerPrefs.Save();
    }

    void LoadData()
    {
        weaponUpgradeLevel = PlayerPrefs.GetInt("WeaponLevel", 1); // 默认 Lv1
        gold = PlayerPrefs.GetInt("Gold", 0); // 默认 0金币
    }
}