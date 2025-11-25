using UnityEngine;

public class DiagonalMovement : MonoBehaviour
{
    private Vector2 moveDirection;
    private float moveSpeed;
    
    // 边界销毁 (防止飞出屏幕太远不销毁)
    private float bottomBound = -12f;
    private float sideBound = 10f; // 左右边界宽容度

    // 初始化方法：由生成器调用
    public void Initialize(Vector2 dir, float speed)
    {
        moveDirection = dir.normalized; // 归一化，保证方向长度为1
        moveSpeed = speed;

        // 【可选】让机头朝向飞行方向
        // Atan2 计算出的角度是基于X轴的，由于你的飞机贴图是头朝上的(Y轴)，所以要 +90 度
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle + 90, Vector3.forward);
    }

    void Update()
    {
        // 按照设定方向移动
        // Space.World 很重要，因为如果我们旋转了物体，Translate默认是自身坐标系
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);

        // 越界销毁检查
        if (transform.position.y < bottomBound || 
            Mathf.Abs(transform.position.x) > sideBound)
        {
            // 如果用了对象池，改为 ObjectPool.Instance.ReturnEnemy(gameObject);
            Destroy(gameObject); 
        }
    }
}