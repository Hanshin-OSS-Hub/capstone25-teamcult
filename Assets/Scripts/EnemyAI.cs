using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public float speed = 3f;        // 적의 이동 속도
    public float detectRange = 5f;  // 감지 거리 (이 거리 안에 오면 쫓아옴)

    private Transform target;       // 쫓아갈 대상(플레이어)

    void Start()
    {
        // 게임 시작 시 "Player"라는 태그가 달린 오브젝트를 찾아 타겟으로 설정
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            target = playerObj.transform;
        }
    }

    void Update()
    {
        // 타겟(플레이어)이 없으면 아무것도 안 함 (에러 방지)
        if (target == null) return;

        // 1. 적과 플레이어 사이의 거리 계산
        float distance = Vector2.Distance(transform.position, target.position);

        // 2. 거리가 감지 범위(detectRange)보다 작으면 쫓아감
        if (distance <= detectRange)
        {
            // MoveTowards: 현재 위치에서 목표 위치로 조금씩 이동
            transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        }
        // 거리가 멀면 else가 없으므로 자동으로 가만히 멈춰 있음
    }

    // 개발자 편의 기능: 씬(Scene) 화면에 감지 범위를 빨간 원으로 그려줌
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}