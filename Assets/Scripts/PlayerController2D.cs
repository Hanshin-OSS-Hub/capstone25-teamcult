using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    // 인스펙터 창에서 속도를 조절할 수 있도록 public으로 선언합니다.
    public float moveSpeed = 5f;

    // Player의 Rigidbody2D 컴포넌트를 담을 변수
    private Rigidbody2D rb;
    
    // Player의 이동 입력을 저장할 변수 (2D이므로 Vector2 사용)
    private Vector2 moveInput;

    void Start()
    {
        // 스크립트가 시작될 때, Player 오브젝트의 Rigidbody2D 컴포넌트를 가져옵니다.
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 1. 입력 감지 (Update에서 매 프레임 감지)
        // GetAxisRaw를 사용하면 -1, 0, 1의 값을 즉시 반환합니다.
        float moveX = Input.GetAxisRaw("Horizontal"); // A, D 키
        float moveY = Input.GetAxisRaw("Vertical");   // W, S 키

        // 2. 이동 방향 벡터 생성 및 정규화
        // 3D와 달리 X와 Y축을 사용합니다.
        // .normalized를 통해 대각선으로 이동할 때 속도가 더 빨라지는 것을 방지합니다.
        moveInput = new Vector2(moveX, moveY).normalized;
    }

    void FixedUpdate()
    {
        // 3. 물리 엔진을 통해 이동 처리 (FixedUpdate에서 고정된 주기로 처리)
        
        // Rigidbody2D의 속도(velocity)를 직접 제어합니다.
        // 3D 코드와 달리 Z축이 없으며, Y축이 바로 상하 이동에 사용됩니다.
        rb.linearVelocity = moveInput * moveSpeed;
    }
}