using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("场内状态 (火力规模)")]
    public int maxScaleLevel = 5; // 场内吃道具最多升到几级
    public int currentScaleLevel = 1; // 当前火力规模 (决定发几颗子弹)

    [Header("场外属性 (基础伤害)")]
    public int finalDamageValue; // 最终传给子弹的伤害值 (由 DataManager 决定)

    void Start()
    {
        // 1. 初始化火力规模 (每次进关卡重置为 1)
        currentScaleLevel = 1;

        // 2. 从 DataManager 获取场外养成的伤害值
        // 如果 DataManager 还没做或场景里没有，就给个默认值 10
        if (DataManager.Instance != null)
        {
            finalDamageValue = DataManager.Instance.GetCurrentBaseDamage();
        }
        else
        {
            finalDamageValue = 10; 
        }
        
        Debug.Log($"游戏开始：火力规模 Lv.{currentScaleLevel}, 单发伤害 {finalDamageValue}");
    }

    // === 【核心修复点】 ===
    // 方法名必须叫 IncreasePowerLevel，因为您的 PowerUp.cs 是这样调用的
    public void IncreasePowerLevel()
    {
        if (currentScaleLevel < maxScaleLevel)
        {
            currentScaleLevel++;
            Debug.Log("吃到道具！火力规模提升至 Lv." + currentScaleLevel);
            // 这里可以播放 "Power Up" 音效
        }
        else
        {
            Debug.Log("火力已达上限！");
            // 满级后吃到道具，通常设计为加分
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(500);
            }
        }
    }

    // 降级逻辑 (供 PlayerHealth 玩家受伤时调用)
    public void DowngradeWeapon()
    {
        if (currentScaleLevel > 1)
        {
            currentScaleLevel--;
            Debug.Log("玩家受伤！火力规模降至 Lv." + currentScaleLevel);
        }
    }
}