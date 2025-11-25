using UnityEngine;

public class AutoDestroyEffect : MonoBehaviour
{
    public float delay = 1f; // 设定特效播放大概需要多久

    void OnEnable()
    {
        // 如果你以后打算给特效也做对象池，这里就不能用 Destroy，而要写回收逻辑
        // 现阶段为了简单，我们直接延迟销毁
        Destroy(gameObject, delay);
    }
}