using System.Collections;
using UnityEngine;

public class SquadSpawner : MonoBehaviour
{
    [Header("1. 敌人预制体配置")]
    public GameObject smallEnemyPrefab;  // 小飞机 (挂 DiagonalMovement)
    public GameObject mediumEnemyPrefab; // 中型飞机 (挂 StopAndShootMovement)
    public GameObject bossPrefab;        // Boss (挂 StopAndShootMovement)
    
    [Header("2. 小飞机波次设置")]
    public int smallSquadWaves = 2;     // 小飞机总共来几波 (比如: 左-右-左-右 是4波)
    public int planesPerSquad = 5;      // 每一波有几架
    public float spawnInterval = 0.6f;  // 队内生成间隔
    public float waveInterval = 3f;     // 波次间隔时间
    public float flightSpeed = 5f;      // 小飞机的飞行速度 (在编辑器里调整)

    [Header("3. 精英怪设置")]
    public float mediumSpeed = 3f;      // 精英怪进场速度
    public float stopHeight = 3f;       // 精英怪悬停高度

    [Header("4. Boss设置")]
    public float bossEntrySpeed = 2f;   // Boss 进场速度
    public float bossHoverHeight = 3.5f;// Boss 悬停高度

    // 屏幕边界缓存
    private Vector2 screenBounds;
    private float spawnY; 
    private float spawnX; 

    void Start()
    {
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        
        // === 【修改点】出生高度 ===
        // 原来是 + 2f，改成 + 1f 或者 + 0.5f
        // 这样飞机就在屏幕边缘刚好看不见的地方生成，能更快飞进画面
        spawnY = screenBounds.y + 1.0f; 
        
        spawnX = screenBounds.x + 1f; 
        
        StartCoroutine(LevelFlowRoutine());
    }

    // === 关卡总流程控制 ===
    IEnumerator LevelFlowRoutine()
    {
        // --- 阶段一：小飞机波次循环 ---
        for (int i = 0; i < smallSquadWaves; i++)
        {
            // 偶数波(0, 2...)从左边出，奇数波(1, 3...)从右边出
            bool isLeft = (i % 2 == 0);
            string sideName = isLeft ? "左侧" : "右侧";
            
            Debug.Log($"Wave {i + 1}: 小飞机{sideName}突袭");
            
            yield return StartCoroutine(SpawnSmallSquad(isLeft));
            
            // 等待下一波
            yield return new WaitForSeconds(waveInterval);
        }

        // --- 阶段二：精英怪进场 (死锁等待) ---
        Debug.Log("⚠️ 警告：精英敌机进场！");
        yield return StartCoroutine(SpawnAndWaitForElites());
        
        // 稍微给玩家喘息时间，捡捡掉落物
        yield return new WaitForSeconds(3f);

        // --- 阶段三：BOSS 战 (死锁等待) ---
        Debug.Log("☠️ 警告：BOSS 降临！");
        yield return StartCoroutine(SpawnAndWaitForBoss());

        // --- 阶段四：关卡胜利 ---
        Debug.Log("🎉 关卡胜利！");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LevelComplete();
        }
    }

    // 生成一波小飞机
    IEnumerator SpawnSmallSquad(bool isLeftSide)
    {
        Vector2 startPos = isLeftSide ? new Vector2(-spawnX, spawnY) : new Vector2(spawnX, spawnY);
        Vector2 flyDirection = isLeftSide ? new Vector2(1f, -0.8f) : new Vector2(-1f, -0.8f);

        // 循环生成
        for (int i = 0; i < planesPerSquad; i++)
        {
            if (smallEnemyPrefab != null)
            {
                GameObject enemy = Instantiate(smallEnemyPrefab, startPos, Quaternion.identity);
                
                // 初始化移动
                DiagonalMovement move = enemy.GetComponent<DiagonalMovement>();
                if (move != null) move.Initialize(flyDirection, flightSpeed);

                // === 【核心修改】标记最后一只为“掉落奖励怪” ===
                // i 是从 0 开始的，所以最后一只的索引是 count - 1
                if (i == planesPerSquad - 1)
                {
                    EnemyHealth health = enemy.GetComponent<EnemyHealth>();
                    if (health != null)
                    {
                        health.SetWaveBonusTarget(true); // 标记它！
                    }
                }
            }
            
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    // 生成精英怪并等待它们全部死亡
    IEnumerator SpawnAndWaitForElites()
    {
        // 定义两架飞机的目标位置 (左上和右上)
        Vector2 leftSpawn = new Vector2(-screenBounds.x * 0.5f, spawnY);
        Vector2 leftTarget = new Vector2(-screenBounds.x * 0.5f, stopHeight);

        Vector2 rightSpawn = new Vector2(screenBounds.x * 0.5f, spawnY);
        Vector2 rightTarget = new Vector2(screenBounds.x * 0.5f, stopHeight);

        // 生成
        GameObject elite1 = Instantiate(mediumEnemyPrefab, leftSpawn, Quaternion.identity);
        GameObject elite2 = Instantiate(mediumEnemyPrefab, rightSpawn, Quaternion.identity);

        // 初始化移动 (飞入并悬停)
        if (elite1.GetComponent<StopAndShootMovement>()) 
            elite1.GetComponent<StopAndShootMovement>().Initialize(leftTarget, mediumSpeed);
        
        if (elite2.GetComponent<StopAndShootMovement>()) 
            elite2.GetComponent<StopAndShootMovement>().Initialize(rightTarget, mediumSpeed);

        // === 死锁循环 ===
        // 只要还有任意一个活着，就卡在这里不往下走
        while (elite1 != null || elite2 != null)
        {
            yield return null; // 等待下一帧
        }
    }

    // 生成 Boss 并等待死亡
    IEnumerator SpawnAndWaitForBoss()
    {
        if (bossPrefab == null) yield break;

        // Boss 从正上方生成
        Vector2 spawnPos = new Vector2(0, spawnY);
        Vector2 targetPos = new Vector2(0, bossHoverHeight);

        GameObject boss = Instantiate(bossPrefab, spawnPos, Quaternion.identity);

        // 初始化移动
        StopAndShootMovement move = boss.GetComponent<StopAndShootMovement>();
        if (move != null)
        {
            move.Initialize(targetPos, bossEntrySpeed);
        }

        // === 死锁循环 ===
        // 只要 Boss 活着，就一直卡住，直到它被 Destroy
        while (boss != null)
        {
            yield return null;
        }
    }
}