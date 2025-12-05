using UnityEngine;
using UnityEngine.UI; // 必须引用 UI

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;
   
    // 护盾相关
    public GameObject shieldVisual; // 拖入一个圆形的护盾特效物体(作为子物体)
    private bool isInvincible = false;

    [Header("UI 绑定")]
    public Image healthBarFill; // 拖入刚才做的绿色 Fill 图片

     void Start()
    {
        currentHealth = maxHealth;
        if(shieldVisual != null) shieldVisual.SetActive(false);
        UpdateUI();
    }

    // === 道具调用：加血 ===
    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        UpdateUI();
        Debug.Log("玩家回血: " + amount);
    }

    // === 道具调用：开启护盾 ===
    public void ActivateShield(float duration)
    {
        StartCoroutine(ShieldRoutine(duration));
    }

    System.Collections.IEnumerator ShieldRoutine(float duration)
    {
        isInvincible = true;
        if(shieldVisual != null) shieldVisual.SetActive(true);
        
        Debug.Log("护盾开启！");
        yield return new WaitForSeconds(duration);
        
        isInvincible = false;
        if(shieldVisual != null) shieldVisual.SetActive(false);
        Debug.Log("护盾消失");
    }

    public void TakeDamage(int damage)
    {
        // 如果处于无敌状态，直接免疫伤害
        if (isInvincible) return;
        
        currentHealth -= damage;
        UpdateUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateUI()
    {
        if (healthBarFill != null)
        {
            // 计算百分比 (0.0 ~ 1.0)
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }
    }

    void Die()
    {
        // 调用 GameManager 结束游戏
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
        
        // 播放爆炸特效...
        Destroy(gameObject);
    }

    // === 【新增】处理撞击逻辑 ===
    void OnTriggerEnter2D(Collider2D other)
    {
        // 1. 撞到敌人 (保持用 Tag，因为 Enemy 这个 Tag 是 Unity 自带或者你肯定设过的)
        if (other.CompareTag("Enemy"))
        {
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            
            if (isInvincible)
            {
                // 无敌状态：秒杀敌人
                if(enemy != null) enemy.TakeDamage(9999);
                else Destroy(other.gameObject);
            }
            else
            {
                // 普通状态：互相伤害
                if(enemy != null) enemy.TakeDamage(100); 
                else Destroy(other.gameObject);

                TakeDamage(30); // 玩家扣血
            }
        }
        
        // 2. === 【修改点】撞到敌人子弹 ===
        // 不要用 other.CompareTag("EnemyBullet")，因为这需要你去编辑器设置Tag，容易忘。
        // 改用：检查它身上有没有 EnemyBullet 脚本组件
        
        EnemyBullet bullet = other.GetComponent<EnemyBullet>();

        if (bullet != null)
        {
             // 只有在无敌状态下，我们需要在这里处理子弹
             // (因为普通状态下，子弹自己的脚本会处理扣血)
             if (isInvincible)
             {
                 Destroy(other.gameObject); // 护盾挡住并销毁子弹
                 Debug.Log("护盾挡住了子弹！");
             }
        }
    }
}