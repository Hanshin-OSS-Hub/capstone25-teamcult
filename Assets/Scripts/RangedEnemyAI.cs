using UnityEngine;

public class RangedEnemyAI : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 2.5f;      // 근거리보다 조금 느리게
    public float detectRange = 8f;  // 감지는 멀리서
    public float keepDistance = 5f; // ★핵심: 플레이어와 이만큼 거리를 둠 (멈춤)

    [Header("Attack")]
    public GameObject bulletPrefab; // 아까 만든 총알 프리팹 넣기
    public float attackCooldown = 2.0f;
    private float lastAttackTime = 0f;

    private Transform target;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) target = player.transform;
    }

    void Update()
    {
        if (target == null) return;

        float distance = Vector2.Distance(transform.position, target.position);

        // 1. 공격 사거리(유지 거리) 안에 들어왔을 때 -> 멈춰서 쏨
        if (distance <= keepDistance)
        {
            // 이동 멈춤 (추적 안 함)

            // 쿨타임 체크 후 발사
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                Shoot();
                lastAttackTime = Time.time;
            }
        }
        // 2. 감지 범위 안이지만 아직 멀 때 -> 추적
        else if (distance <= detectRange)
        {
            transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null) return;

        Debug.Log("탕! (원거리 공격)");

        // 1. 총알 생성 (적 위치에서)
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

        // 2. 플레이어 방향 계산
        Vector2 direction = (target.position - transform.position).normalized;

        // 3. 총알에게 방향 전달
        bullet.GetComponent<EnemyBullet>().SetDirection(direction);
    }

    // 범위 확인용 기즈모
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange); // 감지
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, keepDistance); // 사격 거리
    }
}