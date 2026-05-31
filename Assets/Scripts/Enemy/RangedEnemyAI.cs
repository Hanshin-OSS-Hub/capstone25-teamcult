using UnityEngine;

public class RangedEnemy : MonoBehaviour
{
    [Header("기본 설정")]
    public float detectRange = 7f;   // 감지 범위 (플레이어를 인식하는 거리)
    public float stopDistance = 3f;  // 정지 거리 (이 거리 안에서는 이동을 멈춤)
    public float moveSpeed = 1.5f;   // 이동 속도

    public GameObject bulletPrefab;  // 발사할 적 탄환 프리팹을 여기에 연결하세요
    public float attackCooldown = 2f; // 공격 간격(초)
    protected float lastAttackTime;

    protected Transform player;

    protected bool hasSpotted = false;


    protected virtual void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        lastAttackTime = Time.time - attackCooldown + Mathf.Min(attackCooldown, 1.5f);
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // 감지 범위 안에 플레이어가 들어왔는지 확인
        if (distance <= detectRange)
        {
            
            if (!hasSpotted)
            {
                hasSpotted = true;
                if (SFXManager.Instance != null) 
                 SFXManager.Instance.PlaySFX(SFXType.EnemyEncounter);
            }
            
            
            // 공격 쿨타임 체크 후 발사
            if (Time.time > lastAttackTime + attackCooldown)
            {
                Shoot();
                lastAttackTime = Time.time;
            }

            // 정지 거리 밖이면 플레이어 쪽으로 이동, 안쪽이면 정지
            if (distance > stopDistance)
            {
                transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
            }
        }
    }

    protected virtual void Shoot()
    {
        if (bulletPrefab == null) return;

        // 탄환 생성
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

        // 생성한 탄환에 플레이어 방향 벡터 전달
        EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
        if (bulletScript != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            bulletScript.SetDirection(direction);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}
