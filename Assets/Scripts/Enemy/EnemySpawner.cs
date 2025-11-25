using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnRate = 1.5f;

    [Header("生成范围调整")]
    public float xPadding = 0.8f; // ⬅️ 新增：X轴留白距离 (值越大，生成得越靠中间)
    public float startYOffset = 1.5f; // ⬅️ 新增：在屏幕顶端上方多少距离生成

    private Vector2 screenBounds;
    private float enemyWidth;

    void Start()
    {
        // 计算屏幕边界
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
        
        // (可选) 自动根据敌人图片大小计算留白，防止手动填错
        // if(enemyPrefab.GetComponent<SpriteRenderer>()) 
        //    enemyWidth = enemyPrefab.GetComponent<SpriteRenderer>().bounds.extents.x;
        
        StartCoroutine(SpawnEnemyRoutine());
    }

    IEnumerator SpawnEnemyRoutine()
    {
        while (true)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnRate);
        }
    }

    void SpawnEnemy()
    {
        // 1. 计算 X 轴的随机范围
        // screenBounds.x 是屏幕右边缘的坐标
        // 减去 xPadding，确保敌人不会贴着边生成
        float leftLimit = -screenBounds.x + xPadding;
        float rightLimit = screenBounds.x - xPadding;

        float randomX = Random.Range(leftLimit, rightLimit);

        // 2. 计算 Y 轴生成位置 (屏幕顶端 + 偏移量)
        // 这样敌人会从屏幕外平滑地飞进来，而不是突然闪现
        float spawnY = screenBounds.y + startYOffset;

        Vector2 spawnPos = new Vector2(randomX, spawnY);
        
        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }
    void OnDrawGizmos()
    {
        // 如果还没运行游戏，screenBounds可能是0，所以这里临时算一下用于显示
        if (Application.isPlaying == false)
        {
            Vector2 bounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
            Gizmos.color = Color.green;
        
            // 画出生成线
            float left = -bounds.x + xPadding;
            float right = bounds.x - xPadding;
            float top = bounds.y + startYOffset;
        
            Gizmos.DrawLine(new Vector3(left, top, 0), new Vector3(right, top, 0));
            Gizmos.DrawWireSphere(new Vector3(left, top, 0), 0.2f); // 左端点
            Gizmos.DrawWireSphere(new Vector3(right, top, 0), 0.2f); // 右端点
        }
    }
}