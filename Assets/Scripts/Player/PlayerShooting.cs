using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("基础设置")]
    public Transform firePoint;     // 依然需要拖入之前的 FirePoint 子物体
    public GameObject bulletPrefab; // 拖入你的子弹 Prefab (或者对象池逻辑)
    public float fireRate = 0.2f;

    private float nextFireTime = 0f;
    
    // 引用我们在上一阶段写的武器管理器
    private WeaponController weaponController;

    void Start()
    {
        // 获取同在 Player 身上的 WeaponController 组件
        weaponController = GetComponent<WeaponController>();
        
        // 容错：如果没有挂 WeaponController，就报错提示
        if (weaponController == null)
        {
            Debug.LogError("请在 Player 上挂载 WeaponController 脚本！");
        }
    }

    void Update()
    {
        // 自动射击 (或者你可以加 Input 判断)
        if (Time.time > nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        // 1. 获取当前等级 (如果没有 WeaponController，默认就是等级 1)
        int level = (weaponController != null) ? weaponController.currentPowerLevel : 1;

        // 2. 根据等级决定发射逻辑
        switch (level)
        {
            case 1:
                FireLevel1();
                break;
            case 2:
                FireLevel2();
                break;
            case 3: // 等级 3 和以上都用最强火力
            default: 
                FireLevel3();
                break;
        }
    }

    // --- 等级 1：单发直射 ---
    void FireLevel1()
    {
        // 使用对象池生成 (如果你还在用 Instantiate，就换回 Instantiate)
        CreateBullet(firePoint.position, Quaternion.identity);
    }

    // --- 等级 2：双发直射 (稍微分开一点) ---
    void FireLevel2()
    {
        Vector3 leftPos = firePoint.position + new Vector3(-0.2f, 0, 0);
        Vector3 rightPos = firePoint.position + new Vector3(0.2f, 0, 0);

        CreateBullet(leftPos, Quaternion.identity);
        CreateBullet(rightPos, Quaternion.identity);
    }

    // --- 等级 3：三发散射 (经典雷霆战机扇形) ---
    void FireLevel3()
    {
        // 中间直射
        CreateBullet(firePoint.position, Quaternion.identity);
        
        // 左斜射 (偏转 -15度)
        CreateBullet(firePoint.position, Quaternion.Euler(0, 0, 15));
        
        // 右斜射 (偏转 15度)
        CreateBullet(firePoint.position, Quaternion.Euler(0, 0, -15));
    }

    // 封装一个生成子弹的方法，方便切换对象池/Instantiate
    void CreateBullet(Vector3 pos, Quaternion rot)
    {
        // 如果你还在用对象池：
        GameObject bullet = ObjectPool.Instance.GetBullet();
        if (bullet != null) 
        {
            bullet.transform.position = pos;
            bullet.transform.rotation = rot;
        }

        // 如果你没有用对象池，用这行：
        // Instantiate(bulletPrefab, pos, rot);
    }
}