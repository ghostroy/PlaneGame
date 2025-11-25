using System.Collections;
using UnityEngine;

public class SquadSpawner : MonoBehaviour
{
    [Header("核心设置")]
    public GameObject enemyPrefab;
    
    [Header("编队设置")]
    public int planesPerSquad = 5;      // 一队有几架飞机 (5架)
    public float spawnInterval = 0.6f;  // 队内每架飞机的生成间隔
    public float waveInterval = 5f;     // 两队之间的等待时间 (5秒)

    [Header("飞机属性控制")]
    public float flightSpeed = 5f;      // 控制飞机飞行速度
    public float fireRate = 1.5f;       // 控制飞机开火频率

    // 屏幕边界缓存
    private Vector2 screenBounds;
    private float spawnY; // 生成高度
    private float spawnX; // 生成的X轴偏移量

    void Start()
    {
        // 计算屏幕边界，确保生成点在屏幕外
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        spawnY = screenBounds.y + 1.5f; // 屏幕顶端上方
        spawnX = screenBounds.x + 1.0f; // 屏幕侧边外侧

        StartCoroutine(SquadRoutine());
    }

    IEnumerator SquadRoutine()
    {
        while (true)
        {
            // --- 第一阶段：左侧编队 (往右下飞) ---
            yield return StartCoroutine(SpawnSquad(true));

            // 等待波次间隔
            yield return new WaitForSeconds(waveInterval);

            // --- 第二阶段：右侧编队 (往左下飞) ---
            yield return StartCoroutine(SpawnSquad(false));

            // 等待波次间隔，然后循环
            yield return new WaitForSeconds(waveInterval);
        }
    }

    // 生成一整队飞机的逻辑
    IEnumerator SpawnSquad(bool isLeftSide)
    {
        // 设定起始点和飞行方向
        Vector2 startPos;
        Vector2 flyDirection;

        if (isLeftSide)
        {
            // 左上角生成，向右下角飞 (X=1, Y=-1)
            startPos = new Vector2(-spawnX, spawnY);
            flyDirection = new Vector2(0.5f, -0.8f); // -0.8f 决定了下落的陡峭程度
        }
        else
        {
            // 右上角生成，向左下角飞 (X=-1, Y=-1)
            startPos = new Vector2(spawnX, spawnY);
            flyDirection = new Vector2(-0.5f, -0.8f);
        }

        // 循环生成指定数量的飞机
        for (int i = 0; i < planesPerSquad; i++)
        {
            SpawnSingleEnemy(startPos, flyDirection);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnSingleEnemy(Vector2 pos, Vector2 dir)
    {
        GameObject enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);

        // 1. 初始化移动逻辑
        DiagonalMovement movement = enemy.GetComponent<DiagonalMovement>();
        if (movement != null)
        {
            movement.Initialize(dir, flightSpeed);
        }

        // 2. 初始化开火逻辑 (覆盖 Prefab 里的默认设置)
        EnemyShooting shooting = enemy.GetComponent<EnemyShooting>();
        if (shooting != null)
        {
            shooting.fireRate = fireRate;
            // 每次修改fireRate后，最好重置一下它的计时器，但这取决于你的Shooting脚本写法
            // 简单的做法是直接赋值即可，Shooting脚本下次射击会采用新频率
        }
    }
}