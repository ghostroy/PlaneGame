using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    public float scrollSpeed = 2f;
    public float bgHeight = 10.8f; // 务必确保这个值等于图片的Unity单位高度

    void Update()
    {
        // 1. 向下移动
        transform.Translate(Vector2.down * scrollSpeed * Time.deltaTime);

        // 2. 修正后的复位逻辑
        // 不管它是图1还是图2，只要Y轴坐标低于 -bgHeight，说明它已经完全离开屏幕下方了
        // (假设摄像机在 0,0，且屏幕下边缘大概在 -5 到 -8 之间，-10 肯定出去了)
        if (transform.position.y <= -bgHeight)
        {
            RepositionBackground();
        }
    }

    void RepositionBackground()
    {
        // 计算补偿量：当前位置 + 2倍的高度
        // 为什么是 position 而不是直接设坐标？为了保留移动时的平滑小数位，防止画面抖动
        Vector2 newPos = transform.position;
        newPos.y += bgHeight * 2f; 
        transform.position = newPos;
    }
}