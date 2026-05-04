using UnityEngine;
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 movement;
    private PlayerStats stats;
    private Animator anim;
    public Vector2 lastDirection = Vector2.down;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<PlayerStats>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (GameManager.instance.isUIOpen) return;

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (movement.sqrMagnitude > 0)
        {
            lastDirection = movement.normalized;
            anim.SetFloat("DirX", movement.x);
            anim.SetFloat("DirY", movement.y);
            anim.SetBool("IsWalking", true);
        }
        else
        {
            anim.SetBool("IsWalking", false);
        }
    }

    void FixedUpdate()
    {
        if (GameManager.instance.isUIOpen) return;
        if (stats != null)
        {
            rb.MovePosition(rb.position + movement * stats.moveSpeed * Time.fixedDeltaTime);
        }
    }
}