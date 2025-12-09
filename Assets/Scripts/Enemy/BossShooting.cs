using UnityEngine;

public class BossShooting : MonoBehaviour
{
    [Header("基础设置")]
    public GameObject bulletPrefab; // 拖入 EnemyBullet (红色子弹)
    public float fireRate = 2f;     // 发射间隔 (2秒)
    public float startDelay = 2f;   // Boss进场停稳后再开枪

    [Header("弹幕形态")]
    public int bulletCount = 10;    // 一次发射多少发 (10发)
    
    [Header("进阶设置 (可选)")]
    public bool spinPattern = false; // 是否开启螺旋效果
    public float spinAngleStep = 10f; // 每次发射整体旋转多少度

    private float timer;
    private float currentAngleOffset = 0f; // 用于记录旋转偏移

    void Start()
    {
        timer = startDelay;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            FireNova();
            timer = fireRate;
        }
    }

    // === 核心：发射环形弹幕 ===
    void FireNova()
    {
        if (bulletPrefab == null) return;

        // 计算每颗子弹的角度间隔 (360 / 10 = 36度)
        float angleStep = 360f / bulletCount;

        for (int i = 0; i < bulletCount; i++)
        {
            // 1. 计算当前这颗子弹的角度
            // 基础角度 (i * 36) + 额外的螺旋偏移
            float currentAngle = (i * angleStep) + currentAngleOffset;

            // 2. 将角度转换为四元数 (Unity的旋转格式)
            // 绕 Z 轴旋转
            Quaternion rotation = Quaternion.Euler(0, 0, currentAngle);

            // 3. 生成子弹
            Instantiate(bulletPrefab, transform.position, rotation);
        }

        // 播放音效 (如果有)
        // AudioManager.Instance.PlayBossShoot();

        // 进阶：如果开启了螺旋，下一次发射时整体转一点点
        if (spinPattern)
        {
            currentAngleOffset += spinAngleStep;
        }
    }
}