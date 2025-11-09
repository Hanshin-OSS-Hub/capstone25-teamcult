using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMoveAI : MonoBehaviour
{
    public float moveSpeed = 3f;      // 적의 이동 속도
    public float sightRange = 8f;     // 플레이어를 발견하는 시야 (원)

    private Transform player;           
    private Rigidbody2D rb;
    private Animator myAnim;

    private Vector2 moveDirection;      // 적이 움직일 방향

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        myAnim = GetComponent<Animator>();

        // 중력 0으로 설정
        rb.gravityScale = 0;

        // 씬에서 'playerMove' 스크립트를 가진 오브젝트를 찾아 player
        playerMove playerObject = FindObjectOfType<playerMove>();
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError(gameObject.name + ": !");
            enabled = false; 
        }
    }

    void Update()
    {
        if (player == null) return; 

        // 1. 플레이어와의 거리 계산
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // 2. 시야안에 플레이어가 있는지 확인
        if (distanceToPlayer <= sightRange)
        {
            // 플레이어가 범위 안에 있음 - 플레이어 방향으로 이동
            // (목표 위치 - 현재 위치) = 방향
            moveDirection = (player.position - transform.position).normalized;
        }
        else
        {
            // 플레이어가 범위 밖에 있음 - 멈춤
            moveDirection = Vector2.zero;
        }
    }

    void FixedUpdate()
    {
        //실제 이동 (속도 설정)
        rb.linearVelocity = moveDirection * moveSpeed;

       
        if (myAnim != null)
        {
            myAnim.SetFloat("MoveX", rb.linearVelocity.x);
            myAnim.SetFloat("MoveY", rb.linearVelocity.y);
        }
    }

   
    
}