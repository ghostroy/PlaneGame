using UnityEngine;
using UnityEngine.UI; // 依然保留，因为 Button 和 Panel 还在用原生 UI
using UnityEngine.SceneManagement;
using TMPro; // 引入 TMP 命名空间

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
        Instance = this;
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
            // TMP 的赋值方式和旧版 Text 完全一样，也是 .text
            scoreText.text = "Score: " + score;
        }
    }

    
    public void TriggerBomb()
    {
         // 1. 找到场景里所有的敌人
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
    
        foreach (GameObject enemy in enemies)
        {
        // 调用敌人的受击方法，给予巨大伤害(比如9999)
            EnemyHealth eh = enemy.GetComponent<EnemyHealth>();
            if (eh != null)
            {
                eh.TakeDamage(9999);
            }
            else
            {
            Destroy(enemy);
            }
        }
    
        // 2. 清除全屏子弹 (可选)
        // 需要给敌人的子弹加一个 Tag 叫 "EnemyBullet"
        GameObject[] bullets = GameObject.FindGameObjectsWithTag("EnemyBullet");
        foreach (var b in bullets) Destroy(b);

        // 3. 播放全屏闪白或核弹特效
        Debug.Log("核弹释放！全屏清除！");
        // CameraShake.Instance.Shake(); // 震屏
    }

    public void GameOver()
    {
        isGameOver = true;
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (finalScoreText != null)
            {
                finalScoreText.text = "Final Score: " + score;
            }
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // --- 4. 返回主菜单 ---
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f; // 记得恢复时间，否则回主菜单也是暂停的
        SceneManager.LoadScene("MainMenu"); 
    }

    // --- 5. 直接退出游戏,未来用于在PC端完全退出游戏---
    public void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }


}