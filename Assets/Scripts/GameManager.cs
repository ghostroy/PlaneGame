using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI 设置")]
    public TMP_Text scoreText;          
    public GameObject gameOverPanel;
    public TMP_Text finalScoreText;
    
    // === 【新增】胜利面板 ===
    public GameObject victoryPanel; 

    private int score = 0;
    private bool isGameOver = false;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
        isGameOver = false;
    }

    void Start()
    {
        // === 【新增】安全重置 ===
        // 防止上一局游戏胜利后把磁铁打开了，导致新游戏一开始道具就乱飞
        PowerUp.isGlobalMagnetActive = false;

        if(gameOverPanel != null) gameOverPanel.SetActive(false);
        if(victoryPanel != null) victoryPanel.SetActive(false); // 隐藏胜利面板
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
        if (scoreText != null) scoreText.text = "Score: " + score;
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

    // === 【新增】关卡胜利逻辑 ===
    public void LevelComplete()
    {
        if (isGameOver) return;
        
        Debug.Log("🎉 关卡胜利！BOSS 已被击败！");
        
        // 稍微延迟一下显示面板，体验更好
        Invoke("ShowVictoryPanel", 1f);
    }

    void ShowVictoryPanel()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            
            // 1. 清除全屏危险 (敌人/子弹)
            TriggerBomb(); 

            // 2. 禁止玩家操作 (防止胜利后乱跑)
            DisablePlayerControl();

            // 3. === 【核心修改】开启全屏磁铁 ===
            // 此时 Boss 掉落的物品已经散落在地上了
            // 这行代码会让它们全部自动飞向玩家
            PowerUp.isGlobalMagnetActive = true;
            Debug.Log("🧲 胜利结算：自动吸附所有战利品！");
        }
    }

    // 新增一个方法来禁用玩家
    void DisablePlayerControl()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // 1. 禁止移动
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null) pc.enabled = false; 

            // 2. 禁止射击
            PlayerShooting ps = player.GetComponent<PlayerShooting>();
            if (ps != null) ps.enabled = false;

            // 3. (可选) 让玩家进入无敌状态，防止意外死亡
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            // === 【修改点 2】传入 false，开启无敌但不显示特效 ===
            if (ph != null) ph.ActivateShield(999f, false);
        }
    }

    // === 【新增】进入下一关 ===
    public void LoadNextLevel()
    {
        Time.timeScale = 1f;
        
        // 更新 DataManager 里的关卡数 (如果有的话)
        if (DataManager.Instance != null)
        {
            // 这里我们暂时还没有 level 变量，但可以先留个位置
            // DataManager.Instance.currentLevelIndex++; 
        }

        // 因为你说“重复玩一遍”，所以我们重新加载当前场景
        // 未来设计了 Level2, Level3 后，这里可以改成 LoadScene(currentLevel + 1)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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