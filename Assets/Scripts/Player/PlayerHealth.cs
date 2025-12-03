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
}