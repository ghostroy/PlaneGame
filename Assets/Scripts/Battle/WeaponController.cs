using UnityEngine;

public class WeaponController : MonoBehaviour
{
    // 永久升级的上限，方便在编辑器中调整
    [Header("武器属性")]
    public int maxPowerLevel = 7; 

    // 存储当前关卡内的武器等级 (0-7)
    [Tooltip("关卡内通过拾取道具获得的临时等级")]
    public int currentPowerLevel = 1; 

    // 存储玩家选择的主武器类型 (用于未来的多武器切换)
    // 初始设定为 "Laser"，后续可以从 DataManager 中加载
    public string currentWeaponType = "Laser"; 

    void Start()
    {
        // 确保等级不会低于 1
        if (currentPowerLevel < 1)
        {
            currentPowerLevel = 1;
        }
    }

    // === 由 PowerUp.cs 调用 ===
    public void IncreasePowerLevel()
    {
        if (currentPowerLevel < maxPowerLevel)
        {
            currentPowerLevel++;
            Debug.Log("武器等级提升至: " + currentPowerLevel);
            // 未来可以在这里播放升级音效或特效
        }
        else
        {
            Debug.Log("已达最高等级！");
        }
    }

    // === 由 PlayerHealth.cs 或 GameManager 调用 ===
    public void DowngradeWeapon()
    {
        if (currentPowerLevel > 1)
        {
            currentPowerLevel--;
            Debug.Log("武器等级降至: " + currentPowerLevel);
        }
        else
        {
            // 如果已经是 Level 1，不降级，可能直接触发死亡逻辑或闪烁无敌
        }
    }

    // 未来添加：LoadWeaponStatsFromData(), ChangeWeaponType(), etc.
}