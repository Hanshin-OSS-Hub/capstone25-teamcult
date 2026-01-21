using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // public float moveSpeed = 5f; // ← [삭제됨] 이제 이거 안 씁니다!

    private Rigidbody2D rb;
    private Vector2 movement;
    private PlayerStats stats; //  [추가] 스탯 관리소 연결

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<PlayerStats>(); // 내 몸에 있는 스탯 스크립트 찾기
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
    }

    void FixedUpdate()
    {
        //  [변경] stats.moveSpeed를 가져와서 움직임
        if (stats != null)
        {
            rb.MovePosition(rb.position + movement * stats.moveSpeed * Time.fixedDeltaTime);
        }
    }
}