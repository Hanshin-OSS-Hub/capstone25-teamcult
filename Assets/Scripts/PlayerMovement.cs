using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
   
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    //Animator anim;
    void Start()
    {
        // 스크립트가 시작될 때, Player 오브젝트의 Rigidbody2D 컴포넌트를 가져옵니다.
        rb = GetComponent<Rigidbody2D>();
        //anim = GetComponent<Animator>();
    }

    void Update()
    {
        // 1. 입력 감지 (Update에서 매 프레임 감지)
        // GetAxisRaw를 사용하면 -1, 0, 1의 값을 즉시 반환합니다.
        float moveX = Input.GetAxisRaw("Horizontal"); // A, D 키
        float moveY = Input.GetAxisRaw("Vertical");   // W, S 키
       // anim.SetFloat("Speed", moveInput.magnitude);

        moveInput = new Vector2(moveX, moveY).normalized;

        //if (moveX != 0 || moveY != 0)
        //{
        //    anim.SetFloat("LookX", moveX);
        //    anim.SetFloat("LookY", moveY);
        //}
    }

    void FixedUpdate()
    {
        
        rb.linearVelocity = moveInput * moveSpeed;
    }
}