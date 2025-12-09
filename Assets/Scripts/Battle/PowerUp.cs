using UnityEngine;

// 定义道具类型枚举
public enum ItemType
{
    WeaponUpgrade,  // 武器升级
    Health,         // 生命回复
    Shield,         // 护盾/无敌
    Bomb,           // 核弹/清屏
    Gold,           // 资源/金币
    Magnet          // 磁铁 (全屏吸附)
}

public class PowerUp : MonoBehaviour
{
    [Header("配置")]
    public int itemID = 2001; // 只填 ID，其他从表格读
    
    [Header("道具类型设置")]
    private ItemType itemType;
    private int amount = 1; // 通用数值：加多少血/加多少分/护盾持续几秒

    [Header("运动设置")]
    public float initialSpeed = 5f;
    public float friction = 2f;
    public float minSpeed = 1f;

    [Header("磁力设置")]
    public float baseMagnetRange = 3f; // 基础吸附距离
    public float magnetSpeed = 15f;

    private Vector2 velocity;
    private Vector2 screenBounds;
    private Transform playerTransform;
    
    // 静态标记：全屏磁铁是否激活
    public static bool isGlobalMagnetActive = false;

    void Start()
    {
        // === 1. 初始化数据 (从 CSV 读取) ===
        InitDataFromCSV();

        // 2. 运动初始化
        InitMovement();
    }

    void InitDataFromCSV()
    {
        if (DataManager.Instance == null) return;
        var config = DataManager.Instance.GetItemConfig(itemID);

        if (config != null)
        {
            // 数据注入
            this.itemType = config.type;
            this.amount = config.value;
            // Debug.Log($"道具 {itemID} 初始化: 类型={itemType}, 数值={amount}");
        }
        else
        {
            Debug.LogError($"❌ 找不到 ID 为 {itemID} 的道具配置！");
        }
    }

    // 为了代码整洁，把运动初始化封装一下
    void InitMovement()
    {
        float padding = 0.5f;
        Camera cam = Camera.main;
        if(cam == null) return;
        float height = cam.orthographicSize;
        float width = height * cam.aspect;
        screenBounds = new Vector2(width - padding, height - padding);

        float randomAngle = Random.Range(0f, 360f);
        velocity = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)) * initialSpeed;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    void Update()
    {
        // === 1. 磁力逻辑 (升级版) ===
        if (playerTransform != null)
        {
            float dist = Vector2.Distance(transform.position, playerTransform.position);
            
            // 判断条件：距离够近 OR 全屏磁铁已激活
            if (dist < baseMagnetRange || isGlobalMagnetActive)
            {
                transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, magnetSpeed * Time.deltaTime);
                return; // 吸附时忽略漂浮逻辑
            }
        }

        // === 2. 漂浮逻辑 (保持不变) ===
        if (velocity.magnitude > minSpeed)
            velocity -= velocity.normalized * friction * Time.deltaTime;
        
        transform.Translate(velocity * Time.deltaTime);
        CheckBounds();
    }

    void CheckBounds()
    {
        // ... (保持之前的反弹逻辑不变，省略以节省篇幅) ...
        Vector3 pos = transform.position;
        if (pos.x > screenBounds.x) { pos.x = screenBounds.x; velocity.x = -Mathf.Abs(velocity.x); }
        else if (pos.x < -screenBounds.x) { pos.x = -screenBounds.x; velocity.x = Mathf.Abs(velocity.x); }
        if (pos.y > screenBounds.y) { pos.y = screenBounds.y; velocity.y = -Mathf.Abs(velocity.y); }
        else if (pos.y < -screenBounds.y) { pos.y = -screenBounds.y; velocity.y = Mathf.Abs(velocity.y); }
        transform.position = pos;
    }

    // === 核心：拾取分发逻辑 ===
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ApplyEffect(other.gameObject);
            Destroy(gameObject);
        }
    }

    void ApplyEffect(GameObject player)
    {
        switch (itemType)
        {
            case ItemType.WeaponUpgrade:
                // 武器升级
                WeaponController wc = player.GetComponent<WeaponController>();
                if (wc != null) wc.IncreasePowerLevel();
                break;

            case ItemType.Health:
                // 生命回复
                PlayerHealth ph = player.GetComponent<PlayerHealth>();
                if (ph != null) ph.Heal(amount); // amount 例如 20
                break;

            case ItemType.Shield:
                // 护盾/无敌
                PlayerHealth phShield = player.GetComponent<PlayerHealth>();
                if (phShield != null) phShield.ActivateShield(amount); // amount 是持续时间(秒)
                break;

            case ItemType.Bomb:
                // 核弹/清屏
                GameManager.Instance.TriggerBomb();
                break;

            case ItemType.Gold:
                // 金币/资源
                if (DataManager.Instance != null)
                {
                    DataManager.Instance.gold += amount;
                    // TODO: 播放金币音效，更新UI
                    Debug.Log("获得金币: " + amount);
                }
                break;

            case ItemType.Magnet:
                // 磁铁 (激活全屏吸附)
                // 这里我们调用一个协程来处理持续时间，建议写在 Player 身上的脚本里
                // 为了简单，我们直接在 PlayerController 里加个方法
                PlayerController pc = player.GetComponent<PlayerController>();
                if (pc != null) pc.ActivateMagnet(amount); // amount 是持续时间
                break;
        }
    }
}