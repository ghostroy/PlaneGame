using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // 引用 TMP

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI 设置")]
    public TMP_Text scoreText;          
    public GameObject gameOverPanel;
    public TMP_Text finalScoreText;     

    private int score = 0;
    private bool isGameOver = false;

    void Awake()
    {
        // === 1. 单例初始化 ===
        // 覆盖 Instance，确保每个场景都有新的 GameManager
        Instance = this;

        // === 2. 强制恢复时间 ===
        // 防止上一局暂停导致新游戏卡住
        Time.timeScale = 1f;
        isGameOver = false;
    }

    void Start()
    {
        if(gameOverPanel != null) gameOverPanel.SetActive(false);
        UpdateScoreUI();
    }

    public void AddScore(int amount)
    {
        if (isGameOver) return;
        score += amount;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    // === 3. 游戏结束逻辑 ===
    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        
        Debug.Log("游戏结束！");

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (finalScoreText != null)
            {
                finalScoreText.text = "Final Score: " + score;
            }
        }
    }

    // === 4. 重启逻辑 ===
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    // === 5. 返回主菜单 ===
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    // === 6. 【补回】核弹清屏逻辑 ===
    public void TriggerBomb()
    {
        // A. 清除所有敌人
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            EnemyHealth eh = enemy.GetComponent<EnemyHealth>();
            if (eh != null)
            {
                // 造成巨额伤害
                eh.TakeDamage(9999);
            }
            else
            {
                Destroy(enemy);
            }
        }

        // B. 清除所有子弹 (使用类型查找，不依赖 Tag，防止报错)
        EnemyBullet[] bullets = FindObjectsOfType<EnemyBullet>();
        foreach (EnemyBullet b in bullets)
        {
            // 这里可以加一个子弹消失特效
            Destroy(b.gameObject);
        }

        Debug.Log("核弹释放！全屏清除！");
        
        // 震屏效果 (如果以后加了相机震动脚本，可以在这里调用)
        // CameraShake.Instance.Shake(); 
    }
}