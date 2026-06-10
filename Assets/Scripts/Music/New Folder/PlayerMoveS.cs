using UnityEngine;

public class PlayerMoveS : MonoBehaviour
{
    [Header("설정")]
    [Tooltip("플레이어 이동 속도입니다.")]
    public float moveSpeed = 5f; 

    private Rigidbody2D rb;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        if (rb == null)
        {
            Debug.LogError("이 오브젝트에 Rigidbody2D 컴포넌트가 없습니다! 추가해주세요.");
        }
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        
        movement = movement.normalized; 
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }
    }
}