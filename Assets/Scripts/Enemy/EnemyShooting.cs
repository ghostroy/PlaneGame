using UnityEngine;

public class EnemyShooting : MonoBehaviour
{
    public GameObject bulletPrefab; // 拖入刚才做的红色子弹 Prefab
    public float fireRate = 2f;     // 每几秒开一枪
    public float startDelay = 1f;   // 生成后延迟多久开始射击(防止刚出来就开枪)

    private float timer;

    void Start()
    {
        // 稍微随机化一下发射间隔，这样如果有多个敌人，它们不会像机器人一样完全同步开枪
        // 比如 fireRate 是 2，那就在 1.8 ~ 2.2 之间随机
        fireRate = Random.Range(fireRate * 0.9f, fireRate * 1.1f);
        
        timer = startDelay;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            Fire();
            timer = fireRate; // 重置计时器
        }
    }

    void Fire()
    {
        if (bulletPrefab != null)
        {
            // 在当前位置生成子弹
            Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            
            // 如果你有发射音效，可以在这里播放
            // AudioSource.PlayClipAtPoint(shootSound, transform.position);
        }
    }
}