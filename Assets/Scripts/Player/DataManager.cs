using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    [Header("存档数据")]
    public int weaponUpgradeLevel = 1; // 场外升级等级 (Lv.1 ~ Lv.99)
    public int gold = 0;

    void Awake()
    {
        // 简单的单例模式，且切换场景不销毁
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- 核心公式 ---
    // 根据等级计算基础伤害。例如：Lv1=10, Lv2=12, Lv3=14...
    public int GetCurrentBaseDamage()
    {
        int baseDmg = 10;
        int growthPerLevel = 2;
        return baseDmg + (weaponUpgradeLevel - 1) * growthPerLevel;
    }

    // --- 升级逻辑 (供主界面 UI 调用) ---
    public void UpgradeWeapon()
    {
        int cost = weaponUpgradeLevel * 100; // 升级价格公式
        if (gold >= cost)
        {
            gold -= cost;
            weaponUpgradeLevel++;
            SaveData();
            Debug.Log("武器升级成功！当前等级: " + weaponUpgradeLevel);
        }
    }

    // --- 简单的存档读写 ---
    void SaveData()
    {
        PlayerPrefs.SetInt("WeaponLevel", weaponUpgradeLevel);
        PlayerPrefs.SetInt("Gold", gold);
        PlayerPrefs.Save();
    }

    void LoadData()
    {
        weaponUpgradeLevel = PlayerPrefs.GetInt("WeaponLevel", 1);
        gold = PlayerPrefs.GetInt("Gold", 0);
    }
}