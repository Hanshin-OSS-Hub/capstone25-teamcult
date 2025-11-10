using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))] 
public class Player : MonoBehaviour
{
    public float moveSpeed = 5f;


    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 moveInput;

    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // 중력 0으로 설정
        rb.gravityScale = 0;
        // 회전 고정
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Update()
    {
        // 1. WASD 및 방향키 입력
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        // 2. 8방향 이동을 위해 입력 벡터를 저장하고 정규화
        moveInput = new Vector2(horizontalInput, verticalInput).normalized;
    }

    private void FixedUpdate()
    {
        // 계산된 입력 방향(moveInput)과 속도를 곱하여 이동
       
        rb.linearVelocity = moveInput * moveSpeed;

        // 애니메이터 파라미터 설정 (애니메이션이 있다면)
        if (anim != null)
        {
            // 움직임 속도를 파라미터에 전달
            anim.SetFloat("MoveX", rb.linearVelocity.x);
            anim.SetFloat("MoveY", rb.linearVelocity.y);
        }
    }
}