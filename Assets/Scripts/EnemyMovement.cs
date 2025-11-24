using UnityEngine;

public class EnemyWaveMovement : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float deadZone = -7f;

    [Header("波动设置")]
    public float frequency = 2f; // 左右摆动的频率（速度）
    public float magnitude = 2f; // 左右摆动的幅度（宽度）

    private Vector3 startPosition;

    void Start()
    {
        // 记录初始生成的位置
        startPosition = transform.position;
    }

    void Update()
    {
        // 计算新的位置
        // Y轴：持续向下
        startPosition += Vector3.down * moveSpeed * Time.deltaTime;

        // X轴：基于 Y 轴的变化或时间生成正弦波 (Sin)
        // 这里我们用 Time.time 让它随时间摆动
        float newX = startPosition.x + Mathf.Sin(Time.time * frequency) * magnitude;

        // 应用位置
        transform.position = new Vector3(newX, startPosition.y, 0);

        // 销毁检测
        if (transform.position.y < deadZone)
        {
            Destroy(gameObject);
        }
    }
}