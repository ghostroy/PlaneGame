using System.Collections;
using System.Collections.Generic; // 引入 List
using UnityEngine;

public class SquadSpawner : MonoBehaviour
{
    [Header("敌人预制体")]
    public GameObject smallEnemyPrefab;  // 小飞机
    public GameObject mediumEnemyPrefab; // 中型飞机
    
    [Header("编队设置")]
    public int planesPerSquad = 5;      // 小队数量
    public float spawnInterval = 0.6f;  // 小队内生成间隔
    public float waveInterval = 3f;     // 两波小队之间的间隔
    public float flightSpeed = 5f; // <--- 在 Inspector 里改这个值

    [Header("中型飞机设置")]
    public float mediumSpeed = 3f;
    public float stopHeight = 3f;       // 停在 Y=3 的高度

    // 屏幕边界
    private Vector2 screenBounds;
    private float spawnY; 
    private float spawnX; 

    void Start()
    {
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        spawnY = screenBounds.y + 1.5f;
        spawnX = screenBounds.x + 1.0f; 
        
        StartCoroutine(MainLevelLoop());
    }

    // === 主关卡循环逻辑 ===
    IEnumerator MainLevelLoop()
    {
        // 这是一个无限循环，代表关卡一直进行
        while (true)
        {
            // --- 第 1 步：生成左侧小队 ---
            Debug.Log("Wave 1: 左侧突袭");
            yield return StartCoroutine(SpawnSmallSquad(true));
            
            // 等待
            yield return new WaitForSeconds(waveInterval);

            // --- 第 2 步：生成右侧小队 ---
            Debug.Log("Wave 2: 右侧突袭");
            yield return StartCoroutine(SpawnSmallSquad(false));
            
            // 等待，准备迎接精英怪
            yield return new WaitForSeconds(waveInterval);

            // --- 第 3 步：生成中型精英，并【死锁】直到它们死亡 ---
            Debug.Log("Wave 3: 精英进场 (锁定中...)");
            yield return StartCoroutine(SpawnAndWaitForElites());

            // --- 第 4 步：稍微休息一下，进入下一个大循环 ---
            Debug.Log("精英已清除，下一波准备...");
            yield return new WaitForSeconds(2f);
        }
    }

    // 生成小飞机的逻辑 (不再包含计数器)
    IEnumerator SpawnSmallSquad(bool isLeftSide)
    {
        Vector2 startPos = isLeftSide ? new Vector2(-spawnX, spawnY) : new Vector2(spawnX, spawnY);
        Vector2 flyDirection = isLeftSide ? new Vector2(1f, -0.8f) : new Vector2(-1f, -0.8f);

        for (int i = 0; i < planesPerSquad; i++)
        {
            SpawnSmallEnemy(startPos, flyDirection);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnSmallEnemy(Vector2 pos, Vector2 dir)
    {
        if (smallEnemyPrefab == null) return;
        GameObject enemy = Instantiate(smallEnemyPrefab, pos, Quaternion.identity);
        
        DiagonalMovement move = enemy.GetComponent<DiagonalMovement>();
        if (move != null) 
        {
            // === 修改点：把原来的 5f 改成 flightSpeed 变量 ===
            move.Initialize(dir, flightSpeed); 
        }
    }

    // === 核心：生成精英并等待死亡 ===
    IEnumerator SpawnAndWaitForElites()
    {
        // 1. 定义生成位置和目标位置
        Vector2 leftSpawn = new Vector2(-screenBounds.x * 0.5f, spawnY);
        Vector2 leftTarget = new Vector2(-screenBounds.x * 0.5f, stopHeight);

        Vector2 rightSpawn = new Vector2(screenBounds.x * 0.5f, spawnY);
        Vector2 rightTarget = new Vector2(screenBounds.x * 0.5f, stopHeight);

        // 2. 生成并保存引用 (Keep References)
        GameObject elite1 = Instantiate(mediumEnemyPrefab, leftSpawn, Quaternion.identity);
        GameObject elite2 = Instantiate(mediumEnemyPrefab, rightSpawn, Quaternion.identity);

        // 3. 初始化它们的移动
        if (elite1.GetComponent<StopAndShootMovement>()) 
            elite1.GetComponent<StopAndShootMovement>().Initialize(leftTarget, mediumSpeed);
        
        if (elite2.GetComponent<StopAndShootMovement>()) 
            elite2.GetComponent<StopAndShootMovement>().Initialize(rightTarget, mediumSpeed);

        // 4. 【死锁检查】只要还有一个活着，就卡在这里不往下走
        // 检查 elite1 != null 是因为当物体被 Destroy 后，Unity 会把引用变成 null
        while (elite1 != null || elite2 != null)
        {
            // 每帧检查一次
            yield return null; 
        }

        // 代码运行到这里，说明 elite1 和 elite2 都变成 null 了 (都死了)
        Debug.Log("精英已被击败！解锁下一波。");
    }
}