using UnityEngine;

public class PlayerMoveS : MonoBehaviour
{
    [Header("설정")]
    [Tooltip("플레이어 이동 속도입니다.")]
    public float moveSpeed = 5f; // 다시 부활시켰습니다! 여기서 속도 조절하세요.

    private Rigidbody2D rb;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // 혹시 Rigidbody2D가 없을까봐 안전장치 추가
        if (rb == null)
        {
            Debug.LogError("이 오브젝트에 Rigidbody2D 컴포넌트가 없습니다! 추가해주세요.");
        }
    }

    void Update()
    {
        // WASD 또는 방향키 입력 받기 (-1 ~ 1)
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        
        // 대각선 이동 시 속도가 빨라지는 것 방지 (정규화)
        movement = movement.normalized; 
    }

    void FixedUpdate()
    {
        // 물리 연산을 통한 이동
        if (rb != null)
        {
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }
    }
}