using UnityEngine;

public class StopAndShootMovement : MonoBehaviour
{
    private float speed;
    private Vector2 targetPos; // 目标悬停位置
    private bool hasReachedTarget = false;

    // 初始化方法：由生成器调用，告诉它去哪里
    public void Initialize(Vector2 target, float moveSpeed)
    {
        this.targetPos = target;
        this.speed = moveSpeed;
    }

    void Update()
    {
        if (!hasReachedTarget)
        {
            // === 阶段 1: 飞向目标点 ===
            // 使用 MoveTowards 平滑移动到目标
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

            // 检查距离，如果非常接近了，就认为到达了
            if (Vector2.Distance(transform.position, targetPos) < 0.01f)
            {
                hasReachedTarget = true;
            }
        }
        else
        {
            // === 阶段 2: 到达后悬停 ===
            // 这里可以让它完全静止，也可以加一点轻微的“呼吸感”上下浮动
            // 简单的悬停浮动效果：
            float hoverY = Mathf.Sin(Time.time * 2f) * 0.1f; // 上下轻微摆动
            transform.position = new Vector3(targetPos.x, targetPos.y + hoverY, 0);
        }
    }
}