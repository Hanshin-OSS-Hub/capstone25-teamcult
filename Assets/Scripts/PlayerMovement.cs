using UnityEngine;
using System.Collections; // [중요] 코루틴을 쓰기 위해 꼭 필요함

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Dash Settings")]
    public float dashSpeed = 20f;      // 대시 속도 (20)
    public float dashDuration = 0.25f; // 대시 시간 (0.25초)
    public float dashCooldown = 10f;   // 쿨타임 (10초)

    // 내부 상태 변수
    private bool isDashing = false;
    private bool canDash = true;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private TrailRenderer trail; // 잔상 효과

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        trail = GetComponent<TrailRenderer>(); // 없으면 null
    }

    void Update()
    {
        // 1. 대시 중일 때는 방향키 입력을 무시
        if (isDashing) return;

        // 2. 평소 이동 입력
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(moveX, moveY).normalized;

        // 3. 스페이스바 + 쿨타임 끝남 -> 대시 발동
        if (Input.GetKeyDown(KeyCode.Space) && canDash)
        {
            StartCoroutine(DashRoutine());
        }
    }

    void FixedUpdate()
    {
        // 4. 대시 중일 때는 물리 이동 업데이트 안 함 (대시 속도 유지)
        if (isDashing) return;

        rb.linearVelocity = moveInput * moveSpeed;
    }

    // 대시 로직 (코루틴)
    IEnumerator DashRoutine()
    {
        canDash = false; // 쿨타임 시작
        isDashing = true; // 조작 불가능 상태

        // 대시 방향 결정 (멈춰있으면 오른쪽으로)
        Vector2 dashDir = moveInput;
        if (dashDir == Vector2.zero)
        {
            dashDir = new Vector2(1, 0);
        }

        // 슉! 하고 힘 주기
        rb.linearVelocity = dashDir * dashSpeed;

        // 잔상 켜기
        if (trail != null) trail.emitting = true;

        // 0.25초 유지
        yield return new WaitForSeconds(dashDuration);

        // 대시 끝
        if (trail != null) trail.emitting = false;
        rb.linearVelocity = Vector2.zero; // 멈춤
        isDashing = false; // 다시 조작 가능

        // 10초 쿨타임 대기
        yield return new WaitForSeconds(dashCooldown);
        canDash = true; // 쿨타임 끝
    }
}