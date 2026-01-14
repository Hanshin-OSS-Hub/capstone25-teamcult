using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 3f;        // 이동 속도
    public float detectRange = 5f;  // 감지 거리 (이 거리 안이면 쫓아옴)

    [Header("Attack")]
    public float attackRange = 1.5f;    // 공격 거리 (이 거리 안이면 멈추고 공격) [추가됨]
    public float attackCooldown = 2.0f; // 공격 쿨타임 [추가됨]
    private float lastAttackTime = 0f;  // 마지막 공격 시간 저장용 [추가됨]

    private Transform target;       // 플레이어

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            target = playerObj.transform;
        }
    }

    void Update()
    {
        if (target == null) return;

        // 1. 현재 플레이어와 적 사이의 거리 계산
        float distance = Vector2.Distance(transform.position, target.position);

        // [변경점] 거리 체크 로직 분기

        // A. 공격 사거리 안에 들어왔을 때 (공격)
        if (distance <= attackRange)
        {
            // 이동하지 않음 (MoveTowards 코드를 실행 안 하면 멈춤)

            // 쿨타임 체크 후 공격
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                Attack(); // 공격 함수 실행
                lastAttackTime = Time.time; // 시간 갱신
            }
        }
        // B. 공격 범위는 아니지만, 감지 범위 안일 때 (추적)
        else if (distance <= detectRange)
        {
            // MoveTowards: 현재 위치에서 목표 위치로 야금야금 이동
            transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        }

        // C. 둘 다 아니면 가만히 있음 (else 생략 가능)
    }

    // [추가됨] 실제 공격 행동을 하는 함수
    void Attack()
    {
        Debug.Log("적군이 공격합니다!");
        // 여기에 나중에 공격 애니메이션 실행이나 데미지 주는 코드를 넣으면 됨
    }

    void OnDrawGizmos()
    {
        // 감지 범위 (빨간색)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        // 공격 범위 (노란색) [추가됨] - 에디터에서 구별하기 쉽게
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}