using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [Header("运动设置")]
    public float moveSpeed = 3f;      // 下落速度
    public float waveFrequency = 2f;  // 左右摆动频率
    public float waveMagnitude = 1.5f;// 左右摆动幅度

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // 1. 向下持续移动 (模拟重力)
        startPos += Vector3.down * moveSpeed * Time.deltaTime;

        // 2. 加上左右摆动 (Sin函数)
        // Time.time 决定波动的进度
        float waveOffset = Mathf.Sin(Time.time * waveFrequency) * waveMagnitude;

        // 3. 应用位置
        transform.position = startPos + new Vector3(waveOffset, 0, 0);

        // 4. 超出屏幕销毁
        if (transform.position.y < -10f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            WeaponController wc = other.GetComponent<WeaponController>();
            if (wc != null)
            {
                wc.IncreasePowerLevel();
                Destroy(gameObject);
            }
        }
    }
}