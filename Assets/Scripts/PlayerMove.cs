using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class playerMove : MonoBehaviour
{
   
    public float playerMoveSpeed = 5f;

    private Rigidbody2D playerRb;
    private Animator myAnim;

    private Vector2 moveInput; 

   
    private void Awake()
    {
        playerRb = GetComponent<Rigidbody2D>();
        myAnim = GetComponent<Animator>();

       
        playerRb.gravityScale = 0;
    }

   
    private void Update()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        moveInput = new Vector2(horizontalInput, verticalInput).normalized;
    }

    private void FixedUpdate()
    {
        playerRb.linearVelocity = moveInput * playerMoveSpeed;

        // 애니메이터 파라미터 설정
        if (myAnim != null)
        {
            myAnim.SetFloat("MoveX", playerRb.linearVelocity.x);
            myAnim.SetFloat("MoveY", playerRb.linearVelocity.y);
        }
    }
}