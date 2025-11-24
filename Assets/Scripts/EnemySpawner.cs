using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnRate = 1.5f;
    private Vector2 screenBounds;

    void Start()
    {
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
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
        // 在屏幕顶部随机X位置生成
        float randomX = Random.Range(-screenBounds.x + 0.5f, screenBounds.x - 0.5f);
        Vector2 spawnPos = new Vector2(randomX, screenBounds.y + 1f);
        
        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }
}