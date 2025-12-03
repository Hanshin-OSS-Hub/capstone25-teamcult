using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float sightRange = 5f; // 5칸으로 설정된 상태
    public float attackRange = 1.5f;

    [SerializeField] private Transform player;
    private Rigidbody2D rb;
    private Animator myAnim;

    private Vector2 moveDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        myAnim = GetComponent<Animator>();

        rb.gravityScale = 0; // 2D이므로 중력 0으로 설정

        // 씬에서 'PlayerController2D' 스크립트를 가진 오브젝트를 찾아 player로 설정
        
        // ------------------------------
        if (player == null) {
            PlayerController2D playerObject = FindFirstObjectByType<PlayerController2D>();
            player = playerObject.transform;
        }

        if (player == null)  {
            Debug.LogError(gameObject.name + ": 씬에서 플레이어(Player)를 찾을 수 없습니다!");
            enabled = false;
        }
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        Debug.Log($"Object: {transform.position}, Player: {player.position}, Distance: {Vector2.Distance(transform.position, player.position)}");

        if (distanceToPlayer <= sightRange)
        {
            if (distanceToPlayer > attackRange)
            {
                moveDirection = (player.position - transform.position).normalized;
            }
            else
            {
                moveDirection = Vector2.zero; // 공격 범위 안 -> 멈춤
            }
        }
        else
        {
            moveDirection = Vector2.zero; // 시야 밖 -> 멈춤
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveDirection * moveSpeed;

        if (myAnim != null)
        {
            myAnim.SetFloat("MoveX", rb.linearVelocity.x);
            myAnim.SetFloat("MoveY", rb.linearVelocity.y);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}