using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float padding = 0.5f; 
    
    

    private Vector2 minBounds;
    private Vector2 maxBounds;

    void Start()
    {
        InitBounds();
    }

    void Update()
    {
        Move();
        // --- 删除掉这里的 Shoot(); ---
    }

    public void ActivateMagnet(float duration)
    {
        StartCoroutine(MagnetRoutine(duration));
    }

    System.Collections.IEnumerator MagnetRoutine(float duration)
    {
        // 修改 PowerUp 里的静态变量，通知所有道具飞过来
        PowerUp.isGlobalMagnetActive = true;
        Debug.Log("全屏磁铁激活！");

        yield return new WaitForSeconds(duration);

        PowerUp.isGlobalMagnetActive = false;
        Debug.Log("磁铁效果结束");
    }

    void InitBounds()
    {
        Camera cam = Camera.main;
        minBounds = cam.ViewportToWorldPoint(new Vector3(0, 0, 0));
        maxBounds = cam.ViewportToWorldPoint(new Vector3(1, 1, 0));
    }

    void Move()
    {
        if (Input.GetMouseButton(0)) 
        {
            Vector3 touchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            touchPos.z = 0;
            transform.position = Vector3.Lerp(transform.position, touchPos, moveSpeed * Time.deltaTime);

            float clampedX = Mathf.Clamp(transform.position.x, minBounds.x + padding, maxBounds.x - padding);
            float clampedY = Mathf.Clamp(transform.position.y, minBounds.y + padding, maxBounds.y - padding);
            transform.position = new Vector3(clampedX, clampedY, 0);
        }
    }

    
}