using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 7f;
    public int damage = 10; // 给玩家造成的伤害

    void Update()
    {
        // 1. 向下移动 (Vector2.down)
        transform.Translate(Vector2.down * speed * Time.deltaTime);

        // 2. 超出屏幕下方销毁 (假设屏幕底部是 -8)
        if (transform.position.y < -8f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 3. 撞到玩家
        if (other.CompareTag("Player"))
        {
            // 获取玩家血量脚本
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            // 撞击后销毁子弹自己
            Destroy(gameObject);
        }
    }
}