using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // 引用 Button
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("战斗 UI 绑定")]
    public TMP_Text scoreText;      // 左上角分数
    public TMP_Text goldText;       // 【新增】右上角金币
    public TMP_Text bombText;       // 【新增】核弹数量
    public Button bombButton;       // 【新增】核弹按钮

    [Header("结算面板绑定")]
    public GameObject gameOverPanel;
    public TMP_Text finalScoreText;     // 失败面板的分数
    public TMP_Text finalGoldText;      // 【新增】失败面板的金币

    public GameObject victoryPanel;
    public TMP_Text victoryScoreText;   // 【新增】胜利面板的分数
    public TMP_Text victoryGoldText;    // 【新增】胜利面板的金币

    // --- 内部数据 ---
    private int score = 0;
    private int levelGold = 0;  // 本关赚的金币
    private int bombCount = 0;  // 当前核弹数 (每关清零)
    
    private bool isGameOver = false;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
        isGameOver = false;
        
        // 【需求实现】每关清零核弹
        bombCount = 0; 
        levelGold = 0;
    }

    void Start()
    {
        // 初始化 UI
        if(gameOverPanel != null) gameOverPanel.SetActive(false);
        if(victoryPanel != null) victoryPanel.SetActive(false);
        
        // 绑定核弹按钮事件
        if (bombButton != null)
        {
            bombButton.onClick.AddListener(UseBomb);
        }

        RefreshUI();
    }

    // --- 1. 数据更新方法 ---

    public void AddScore(int amount)
    {
        if (isGameOver) return;
        score += amount;
        RefreshUI();
    }

    public void AddGold(int amount)
    {
        if (isGameOver) return;
        
        // 1. 增加本关统计
        levelGold += amount;
        
        // 2. 同时增加 DataManager 总金币 (存档用)
        if (DataManager.Instance != null)
        {
            DataManager.Instance.gold += amount;
        }
        
        RefreshUI();
    }

    public void AddBomb()
    {
        if (isGameOver) return;
        bombCount++;
        Debug.Log("获得核弹！当前数量: " + bombCount);
        RefreshUI();
    }

    // === 核心：刷新战斗 UI ===
    void RefreshUI()
    {
        // 分数通常还是保留 "Score:" 前缀比较好看，如果你也想去掉，就改成 score.ToString()
        if (scoreText != null) scoreText.text = "Score: " + score;
        
        // 【修改】只显示数字
        if (goldText != null) goldText.text = levelGold.ToString();
        
        // 【修改】只显示数字
        if (bombText != null) bombText.text = bombCount.ToString();
        
        // 控制按钮是否可点击
        if (bombButton != null) 
        {
            bombButton.interactable = (bombCount > 0);
        }
    }

    // --- 3. 核弹技能逻辑 ---
    public void UseBomb()
    {
        if (isGameOver) return;

        if (bombCount > 0)
        {
            bombCount--; // 扣除数量
            RefreshUI(); // 刷新界面

            TriggerBombEffect(); // 释放大招
        }
        else
        {
            Debug.Log("没有核弹了！");
        }
    }

    // 执行全屏杀伤
    void TriggerBombEffect()
    {
        // A. 清除敌人
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            EnemyHealth eh = enemy.GetComponent<EnemyHealth>();
            if (eh != null) eh.TakeDamage(9999);
            else Destroy(enemy);
        }

        // B. 清除子弹
        EnemyBullet[] bullets = FindObjectsOfType<EnemyBullet>();
        foreach (EnemyBullet b in bullets) Destroy(b.gameObject);

        Debug.Log("💥 核弹引爆！");
        
        // TODO: 在这里播放全屏闪白或爆炸特效
        // Instantiate(bigExplosionPrefab, Vector3.zero, Quaternion.identity);
    }

    // --- 4. 结算逻辑 ---

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        
        // 【修复】弹出面板前，强制同步数据到结算 UI
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (finalScoreText != null) finalScoreText.text = "Score: " + score;
            if (finalGoldText != null) finalGoldText.text = "Get Gold: " + levelGold;
        }
    }

    public void LevelComplete()
    {
        if (isGameOver) return;
        
        // 胜利时不立刻停止，等 Boss 掉落吸完
        Invoke("ShowVictoryPanel", 2f);
    }

    void ShowVictoryPanel()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            
            // 【修复】同步胜利面板数据
            if (victoryScoreText != null) victoryScoreText.text = "Score: " + score;
            if (victoryGoldText != null) victoryGoldText.text = "Get Gold: " + levelGold;

            // 自动吸附战利品
            PowerUp.isGlobalMagnetActive = true;
            
            // 胜利时，禁用玩家操作和核弹按钮
            if (bombButton != null) bombButton.gameObject.SetActive(false);
            DisablePlayerControl();
        }
    }

    void DisablePlayerControl()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            if(player.GetComponent<PlayerController>()) player.GetComponent<PlayerController>().enabled = false;
            if(player.GetComponent<PlayerShooting>()) player.GetComponent<PlayerShooting>().enabled = false;
            if(player.GetComponent<PlayerHealth>()) player.GetComponent<PlayerHealth>().ActivateShield(999f, false);
        }
    }
    
    // ... Restart, LoadNextLevel 等保持不变 ...
    public void RestartGame() { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    public void LoadNextLevel() { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    public void ReturnToMainMenu() { Time.timeScale = 1f; SceneManager.LoadScene("MainMenu"); }
}