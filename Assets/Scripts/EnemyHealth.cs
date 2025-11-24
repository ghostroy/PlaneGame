using UnityEngine;
using UnityEngine.UI; // 引入 UI

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public int scoreValue = 100;
    
    [Header("UI 设置")]
    // 这里拖入敌人头顶那个 Canvas 里的 绿色/红色前景图(Image)
    public Image healthBarFill; 
    public GameObject healthBarCanvas; // 整个血条对象(用于满血隐藏)

    // ... 特效变量 ...
    public GameObject hitEffectPrefab; 
    public GameObject dieEffectPrefab; 

    private int currentHealth;

    void OnEnable()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
        
        // 满血时可以选择隐藏血条，受伤再显示
        if (healthBarCanvas != null) healthBarCanvas.SetActive(false);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        
        // 受伤了，显示血条
        if (healthBarCanvas != null) healthBarCanvas.SetActive(true);
        
        UpdateHealthBar();
        
        // ... 受击特效逻辑 ...
        if (hitEffectPrefab != null) Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);

        if (currentHealth <= 0) Die();
    }

    void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            // 必须强转 float，否则整数相除会变成 0
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }
    }

    void Die()
    {
        if (GameManager.Instance != null) GameManager.Instance.AddScore(scoreValue);
        if (dieEffectPrefab != null) Instantiate(dieEffectPrefab, transform.position, Quaternion.identity);
        
        // 如果使用了对象池，血条会跟着物体一起隐藏，所以不需要额外销毁逻辑
        Destroy(gameObject); 
        // 记得如果用对象池改成: gameObject.SetActive(false);
    }
}