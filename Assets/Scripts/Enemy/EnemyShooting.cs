using UnityEngine;

public class EnemyShooting : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float fireRate = 2f;
    public float startDelay = 1f;
    public bool aimAtPlayer = true; 

    private float timer;
    private Transform playerTransform;
    private Camera mainCam; // 缓存摄像机

    void Start()
    {
        fireRate = Random.Range(fireRate * 0.9f, fireRate * 1.1f);
        timer = startDelay;

        // 缓存 Main Camera，性能更好
        mainCam = Camera.main;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    void Update()
    {
        // 倒计时逻辑保持不变
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            // === 核心修改：加一层判断 ===
            if (IsOnScreen())
            {
                Fire();
            }
            
            timer = fireRate;
        }
    }

    // === 判断是否在屏幕内 ===
    bool IsOnScreen()
    {
        if (mainCam == null) return false;
        Vector3 viewPos = mainCam.WorldToViewportPoint(transform.position);

        // === 【修改点】 ===
        // 原来是 1.05f (允许稍微出界)
        // 改成 0.95f (必须完全进入屏幕内一段距离才允许开火)
        // 这样飞机必须整个身子都飞进来了，才会开始攻击
        bool onScreen = viewPos.x > 0.05f && viewPos.x < 0.95f && 
                        viewPos.y > 0.05f && viewPos.y < 0.95f;

        return onScreen;
    }

    void Fire()
    {
        // ... 原有的发射逻辑 (保持不变) ...
        if (bulletPrefab == null) return;
        Quaternion bulletRotation = Quaternion.identity;
        
        // 重新获取玩家引用(防止玩家死后重生引用丢失)
        if (playerTransform == null)
        {
             GameObject p = GameObject.FindGameObjectWithTag("Player");
             if(p != null) playerTransform = p.transform;
        }

        if (aimAtPlayer && playerTransform != null)
        {
            Vector3 direction = playerTransform.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            bulletRotation = Quaternion.Euler(0, 0, angle + 90);
        }

        Instantiate(bulletPrefab, transform.position, bulletRotation);
    }
}