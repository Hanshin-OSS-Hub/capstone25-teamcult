using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;

    [Header("Dash Settings")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1.0f;

    [Header("Effects")]
    public TrailRenderer trailRenderer; // 트레일 렌더러 연결용 변수 추가!

    private bool isDashing = false;
    private bool isDashCooldown = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 혹시 모르니 시작할 때 트레일 끄기
        if (trailRenderer != null)
            trailRenderer.emitting = false;
    }

    void Update()
    {
        if (isDashing) return;

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(moveX, moveY).normalized;

        // 스페이스바 대시
        if (Input.GetKeyDown(KeyCode.Space) && !isDashCooldown && moveInput != Vector2.zero)
        {
            StartCoroutine(DashRoutine());
        }
    }

    void FixedUpdate()
    {
        if (isDashing) return;
        rb.linearVelocity = moveInput * moveSpeed;
    }

    IEnumerator DashRoutine()
    {
        isDashing = true;
        isDashCooldown = true;

        // ★ 대시 시작: 궤적 그리기 ON
        if (trailRenderer != null)
            trailRenderer.emitting = true;

        rb.linearVelocity = moveInput * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        // ★ 대시 끝: 궤적 그리기 OFF
        if (trailRenderer != null)
            trailRenderer.emitting = false;

        isDashing = false;

        yield return new WaitForSeconds(dashCooldown - dashDuration);
        isDashCooldown = false;
    }
}