using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("配置")]
    public Transform firePoint;     // 发射点
    public float fireRate = 0.2f;   // 射速
    private float nextFireTime = 0f;
    
    // 引用
    private WeaponController weaponController;

    void Start()
    {
        weaponController = GetComponent<WeaponController>();
    }

    void Update()
    {
        if (Time.time > nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        if (weaponController == null) return;

        // 1. 获取场内等级 (决定形态)
        int scaleLevel = weaponController.currentScaleLevel;
        // 2. 获取场外伤害 (决定数值)
        int dmg = weaponController.finalDamageValue;

        // 偏移量设置 (根据飞机的宽度适当调整)
        float innerOffset = 0.15f; // 内侧子弹间距
        float outerOffset = 0.35f; // 外侧子弹间距

        switch (scaleLevel)
        {
            case 1:
                // === Lv.1: 单发直射 ===
                CreateBullet(firePoint.position, Quaternion.identity, dmg);
                break;

            case 2:
                // === Lv.2: 双发直射 ===
                // 左右各偏移一点
                CreateBullet(firePoint.position + new Vector3(-innerOffset, 0, 0), Quaternion.identity, dmg);
                CreateBullet(firePoint.position + new Vector3(innerOffset, 0, 0), Quaternion.identity, dmg);
                break;

            case 3:
                // === Lv.3: 三发散射 (1直 + 2斜) ===
                // (这是之前的逻辑，保留作为过渡)
                CreateBullet(firePoint.position, Quaternion.identity, dmg);
                CreateBullet(firePoint.position, Quaternion.Euler(0, 0, 15), dmg); // 左斜 15度
                CreateBullet(firePoint.position, Quaternion.Euler(0, 0, -15), dmg);// 右斜 15度
                break;

            case 4:
                // === Lv.4: 双发直线 + 两个斜线 (共4发) ===
                // 1. 两发直线 (并排)
                CreateBullet(firePoint.position + new Vector3(-innerOffset, 0, 0), Quaternion.identity, dmg);
                CreateBullet(firePoint.position + new Vector3(innerOffset, 0, 0), Quaternion.identity, dmg);
                
                // 2. 两发斜线 (角度稍微大一点，比如20度)
                // 位置可以稍微靠外一点，或者从中心发
                CreateBullet(firePoint.position, Quaternion.Euler(0, 0, 20), dmg);  // 左斜
                CreateBullet(firePoint.position, Quaternion.Euler(0, 0, -20), dmg); // 右斜
                break;

            case 5:
            default: 
                // === Lv.5: 三直线 + 两个斜线 ===
                // 1. 中间直射
                CreateBullet(firePoint.position, Quaternion.identity, dmg); 
                
                // 2. 左右直射 (【修改点】这里使用 outerOffset，之前是手写的数值)
                CreateBullet(firePoint.position + new Vector3(-outerOffset, 0, 0), Quaternion.identity, dmg); 
                CreateBullet(firePoint.position + new Vector3(outerOffset, 0, 0), Quaternion.identity, dmg);  
                
                // 3. 两个大角度斜线
                CreateBullet(firePoint.position, Quaternion.Euler(0, 0, 30), dmg);
                CreateBullet(firePoint.position, Quaternion.Euler(0, 0, -30), dmg);
                break;
        }
    }

    // 辅助方法：生成并初始化子弹
    void CreateBullet(Vector3 pos, Quaternion rot, int damageValue)
    {
        // 假设你使用了 ObjectPool (如果没有，请改回 Instantiate)
        GameObject obj = ObjectPool.Instance.GetBullet();
        
        if (obj != null)
        {
            obj.transform.position = pos;
            obj.transform.rotation = rot;
            
            // 注入伤害
            Bullet bulletScript = obj.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.InitStats(damageValue);
            }
        }
    }
}