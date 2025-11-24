using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance; // 单例，方便全局访问

    [Header("配置")]
    public GameObject bulletPrefab; // 子弹预制体
    public int poolSize = 20;       // 初始池子大小

    // 核心容器：队列
    private Queue<GameObject> bulletPool = new Queue<GameObject>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 游戏开始时，先生产一堆子弹备用（且默认是关闭状态）
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(bulletPrefab);
            obj.SetActive(false); // 隐藏
            bulletPool.Enqueue(obj); // 入队
        }
    }

    // 从池子里“借”一个子弹
    public GameObject GetBullet()
    {
        if (bulletPool.Count > 0)
        {
            GameObject obj = bulletPool.Dequeue(); // 出队
            obj.SetActive(true); // 激活
            return obj;
        }
        else
        {
            // 如果池子空了，临时生成一个新的（并在稍后归还时加入池子）
            // 这叫“可扩容的对象池”
            GameObject obj = Instantiate(bulletPrefab);
            return obj;
        }
    }

    // 把子弹“还”回池子
    public void ReturnBullet(GameObject obj)
    {
        obj.SetActive(false); // 隐藏
        bulletPool.Enqueue(obj); // 入队
    }
}