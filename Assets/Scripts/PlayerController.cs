using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float padding = 0.5f; // 屏幕边缘留白
    
    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.2f;
    private float nextFireTime = 0f;

    private Vector2 minBounds;
    private Vector2 maxBounds;

    void Start()
    {
        InitBounds();
    }

    void Update()
    {
        Move();
        Shoot();
    }

    void InitBounds()
    {
        Camera cam = Camera.main;
        minBounds = cam.ViewportToWorldPoint(new Vector3(0, 0, 0));
        maxBounds = cam.ViewportToWorldPoint(new Vector3(1, 1, 0));
    }

    void Move()
    {
        // 兼容鼠标(编辑器)和触摸(手机)
        if (Input.GetMouseButton(0)) 
        {
            Vector3 touchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            touchPos.z = 0;

            // 平滑移动到手指位置
            transform.position = Vector3.Lerp(transform.position, touchPos, moveSpeed * Time.deltaTime);

            // 限制在屏幕范围内
            float clampedX = Mathf.Clamp(transform.position.x, minBounds.x + padding, maxBounds.x - padding);
            float clampedY = Mathf.Clamp(transform.position.y, minBounds.y + padding, maxBounds.y - padding);
            transform.position = new Vector3(clampedX, clampedY, 0);
        }
    }

    void Shoot()
    {
        if (Time.time > nextFireTime)
        {
            // 简单的实例化 (后期建议优化为对象池)
            GameObject bullet = ObjectPool.Instance.GetBullet();
            bullet.transform.position = firePoint.position;
            bullet.transform.rotation = Quaternion.identity;
        
            nextFireTime = Time.time + fireRate;
        }
    }
    // 添加 OnTriggerEnter2D 方法
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // 1. 敌人撞到玩家，敌人直接死（或者也扣血，看你设计）
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if(enemy != null) enemy.TakeDamage(100); // 假设撞击伤害很高

            // 2. 玩家扣血 (例如扣 20 血)
            PlayerHealth myHealth = GetComponent<PlayerHealth>();
            if (myHealth != null)
            {
                myHealth.TakeDamage(20); 
            }
            
            // 注意：不要在这里直接 Destroy(gameObject) 了，由 PlayerHealth 控制死亡
        }
    }

}