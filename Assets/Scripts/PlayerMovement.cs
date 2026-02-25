using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // public float moveSpeed = 5f; // ← [삭제됨] 이제 이거 안 씁니다!

    private Rigidbody2D rb;
    private Vector2 movement;
    private PlayerStats stats; //  [추가] 스탯 관리소 연결
    private Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<PlayerStats>(); // 내 몸에 있는 스탯 스크립트 찾기
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // 2. 방향 파라미터 전달 (이동 중일 때만 갱신해서 방향 기억)
        // sqrMagnitude는 벡터 길이의 제곱입니다. 0보다 크면 움직이는 중이라는 뜻!
        if (movement.sqrMagnitude > 0)
        {
            anim.SetFloat("DirX", movement.x);
            anim.SetFloat("DirY", movement.y);

            // [추가] 움직이니까 걷기 모션으로 전환!
            anim.SetBool("IsWalking", true);
        }
        else
        {
            // [추가] 멈췄으니 다시 Idle 모션으로!
            anim.SetBool("IsWalking", false);
        }
        // [꿀팁] 나중에 걷기(Run) 애니메이션과 연결할 때를 대비해, 
        // 캐릭터가 움직이고 있는지(속도)를 애니메이터에 전달해주면 편합니다.
        // 주석을 풀고 Animator에 'Speed'라는 Float 파라미터를 추가해서 쓸 수 있어요.
        // anim.SetFloat("Speed", movement.sqrMagnitude);
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