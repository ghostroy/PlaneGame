using UnityEngine;
using UnityEngine.UI; // 必须引用 UI

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    [Header("UI 绑定")]
    public Image healthBarFill; // 拖入刚才做的绿色 Fill 图片

    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
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