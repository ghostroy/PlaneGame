using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    
    // 【修改】现在这个变量只是一个默认值，实际伤害由发射者决定
    public int damage = 1; 

    // 【新增】提供一个初始化方法，让 PlayerShooting 调用
    public void InitStats(int dmg)
    {
        this.damage = dmg;
    }

    void Update()
    {
        transform.Translate(Vector2.up * speed * Time.deltaTime);
        // ... 回收逻辑 ...
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                // 此时造成的伤害是经过场外升级加成后的数值
                enemy.TakeDamage(damage);
            }
            
            // 回收子弹
            ObjectPool.Instance.ReturnBullet(gameObject);
        }
    }
}