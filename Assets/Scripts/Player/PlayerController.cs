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