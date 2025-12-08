using UnityEngine;
using System.Collections; // 引用协程

public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float padding = 0.5f; // 屏幕边缘留白
    
    private Vector2 minBounds;
    private Vector2 maxBounds;
    
    // 移动手感变量
    private Vector3 offset;
    private bool isDragging = false;

    void Start()
    {
        InitBounds();
    }

    void Update()
    {
        HandleMovement();
    }

    void InitBounds()
    {
        Camera cam = Camera.main;
        minBounds = cam.ViewportToWorldPoint(new Vector3(0, 0, 0));
        maxBounds = cam.ViewportToWorldPoint(new Vector3(1, 1, 0));
    }

    // === 1. 丝滑移动逻辑 (跟手 + 无遮挡) ===
    void HandleMovement()
    {
        if (Time.timeScale == 0) return; // 暂停时禁止移动

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0; 

        // 按下瞬间：记录偏移
        if (Input.GetMouseButtonDown(0))
        {
            offset = transform.position - mousePos;
            isDragging = true;
        }

        // 拖动过程：位置 = 手指 + 偏移
        if (Input.GetMouseButton(0) && isDragging)
        {
            Vector3 targetPos = mousePos + offset;

            // 限制在屏幕内
            float clampedX = Mathf.Clamp(targetPos.x, minBounds.x + padding, maxBounds.x - padding);
            float clampedY = Mathf.Clamp(targetPos.y, minBounds.y + padding, maxBounds.y - padding);
            
            transform.position = new Vector3(clampedX, clampedY, 0);
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    // === 2. 【补回】磁铁技能逻辑 ===
    // 供 PowerUp.cs 调用
    public void ActivateMagnet(float duration)
    {
        // 如果已经在磁铁状态，可以先停止旧的协程(可选)，这里简单处理直接开新的
        StartCoroutine(MagnetRoutine(duration));
    }

    IEnumerator MagnetRoutine(float duration)
    {
        // 开启全屏吸附
        PowerUp.isGlobalMagnetActive = true;
        Debug.Log("🧲 全屏磁铁已激活！");

        // 等待持续时间
        yield return new WaitForSeconds(duration);

        // 关闭全屏吸附
        PowerUp.isGlobalMagnetActive = false;
        Debug.Log("🧲 磁铁效果结束");
    }
}