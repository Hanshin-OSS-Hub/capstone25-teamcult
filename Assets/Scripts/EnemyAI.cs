using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    // ★ 이 변수가 없어서 에러가 났던 겁니다
    public Transform target;
    private EnemyStats stats;

    void Start()
    {
        stats = GetComponent<EnemyStats>();

        // 게임 시작 시 이름이 "Player"인 오브젝트를 찾아서 타겟으로 설정
        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            target = playerObj.transform;
        }
    }

    void Update()
    {
        // 타겟(플레이어)이 없으면 움직이지 않음
        if (target == null) return;

        // 스탯에서 이동 속도 가져오기 (없으면 기본값 2f)
        float speed = (stats != null) ? stats.moveSpeed : 2f;

        // 타겟 쪽으로 이동
        transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        // (선택 사항) 적이 플레이어를 바라보게 뒤집기
        if (target.position.x < transform.position.x) transform.localScale = new Vector3(-1, 1, 1);
        else transform.localScale = new Vector3(1, 1, 1);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 플레이어와 부딪혔을 때
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth player = collision.gameObject.GetComponent<PlayerHealth>();
            if (player != null)
            {
                // 스탯에서 공격력 가져오기 (없으면 기본값 10)
                int dmg = (stats != null) ? stats.damage : 10;
                player.TakeDamage(dmg);
            }
        }
    }
}