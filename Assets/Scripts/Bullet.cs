using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public int damage = 1; // 子弹伤害

    void OnEnable() { /* 重置逻辑 */ }

    void Update()
    {
        transform.Translate(Vector2.up * speed * Time.deltaTime);

        // 回收越界子弹 (复用之前的逻辑)
        if (transform.position.y > 6)
        {
            ObjectPool.Instance.ReturnBullet(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 碰到敌人
        if (other.CompareTag("Enemy"))
        {
            // 1. 尝试获取敌人身上的血量脚本
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();

            // 2. 如果敌人有血条，就扣血
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }
            else
            {
                // 容错：如果敌人忘了挂血量脚本，直接销毁，防止报错
                Destroy(other.gameObject);
            }

            // 3. 子弹任务完成，回收自己
            ObjectPool.Instance.ReturnBullet(gameObject);
        }
    }
}