using UnityEngine;
using System.Collections.Generic;

// [필수!] IEnemyTurn 인터페이스를 받습니다.
public class EnemyMoveAI : MonoBehaviour, IEnemyTurn
{
    // === 인스펙터 설정 ===
    [Header("AI 설정")]
    public Transform player; // TurnManage가 자동으로 채워줄 것입니다.
    
    [Tooltip("플레이어를 발견하는 시야 (맨해튼 거리)")]
    public int sightRange = 8;      
    
    [Tooltip("플레이어를 잃어버리는 추격 한계 (이 거리 밖으로 가면 복귀)")]
    public int loseSightRange = 12;   
    
    [Tooltip("적의 '집' 반경 (이 안에서만 순찰)")]
    public int territoryRadius = 10; 

    // === AI 상태 ===
    // Patrolling: 순찰 / Chasing: 추격 / Returning: 복귀
    private enum AIState { Patrolling, Chasing, Returning }
    private AIState currentState = AIState.Patrolling;

    // === 내부 변수 ===
    private Vector2Int startPosition; // "집" (영역의 중심)
    private Animator anim;
    private readonly int IsMovingHash = Animator.StringToHash("IsMoving");

    void Start()
    {
        // 1. "집" 위치 기록 (현재 위치 기준)
        startPosition = new Vector2Int(Mathf.RoundToInt(transform.position.x),
                                         Mathf.RoundToInt(transform.position.y));

        // 2. 애니메이터 연결 (첫 번째 자식 오브젝트에서 찾기)
        if (transform.childCount > 0)
            anim = transform.GetChild(0).GetComponent<Animator>();
    }

    // [필수!] TurnManage가 이 함수를 호출합니다.
    public void DoEnemyTurn()
    {
        // 플레이어가 없으면 아무것도 안 함 (안전 장치)
        if (player == null)
        {
            Debug.LogWarning(gameObject.name + ": Player가 설정되지 않았습니다!");
            return;
        }

        // --- 위치 및 거리 계산 ---
        Vector2Int enemyPos = GetCurrentPos();
        Vector2Int playerPos = GetPlayerPos();
        int distanceToPlayer = GetManhattanDist(enemyPos, playerPos);
        int distanceToHome = GetManhattanDist(enemyPos, startPosition);

        bool didMove = false; // 이번 턴에 움직였는지? (애니메이션 용)

        // === AI 상태 머신 ===
        switch (currentState)
        {
            // 1. 순찰 상태 (Patrolling)
            case AIState.Patrolling:
                if (distanceToPlayer <= sightRange)
                {
                    // [상태 변경] 플레이어 발견 -> 추격 시작
                    currentState = AIState.Chasing;
                    Debug.Log(gameObject.name + ": 플레이어 발견! 추격 시작.");
                    goto case AIState.Chasing; // (이번 턴은 추격으로 바로 이어짐)
                }
                else
                {
                    // 영역 안에서 무작위로 배회
                    WanderInTerritory(enemyPos, distanceToHome);
                    didMove = true;
                }
                break;

            // 2. 추격 상태 (Chasing)
            case AIState.Chasing:
                if (distanceToPlayer > loseSightRange)
                {
                    // [상태 변경] 플레이어를 놓침 -> 복귀 시작
                    currentState = AIState.Returning;
                    Debug.Log(gameObject.name + ": 플레이어 놓침! 영역으로 복귀.");
                    goto case AIState.Returning; // (이번 턴은 복귀로 바로 이어짐)
                }
                else if (distanceToPlayer <= 1)
                {
                    // 공격! (지금은 이동 멈춤)
                    Debug.Log(gameObject.name + ": 공격!");
                    didMove = false;
                }
                else
                {
                    // 추격 (플레이어에게 1칸 이동)
                    MoveTowards(playerPos, enemyPos);
                    didMove = true;
                }
                break;

            // 3. 복귀 상태 (Returning)
            case AIState.Returning:
                if (distanceToHome <= 0) // "집"에 도착
                {
                    // [상태 변경] "집" 도착 -> 순찰 시작
                    currentState = AIState.Patrolling;
                    Debug.Log(gameObject.name + ": 영역 복귀 완료. 순찰 시작.");
                    didMove = false; // 이번 턴은 도착해서 멈춤
                }
                else
                {
                    // "집"으로 1칸 이동
                    MoveTowards(startPosition, enemyPos);
                    didMove = true;
                }
                break;
        }

        // 애니메이션 업데이트
        if (anim != null)
            anim.SetBool(IsMovingHash, didMove);
    }

    // === 이동 헬퍼 함수 ===

    // 영역 안에서 무작위 이동 (상/하/좌/우/정지)
    void WanderInTerritory(Vector2Int currentPos, int distanceToHome)
    {
        Vector2Int[] possibleMoves = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right, Vector2Int.zero };
        Vector2Int moveDir = possibleMoves[Random.Range(0, possibleMoves.Length)];
        Vector2Int newPos = currentPos + moveDir;

        // 만약 무작위 이동이 영역을 벗어나려 한다면, 대신 "집"으로 향함
        if (GetManhattanDist(newPos, startPosition) > territoryRadius && distanceToHome > 0)
        {
            MoveTowards(startPosition, currentPos); 
        }
        else
        {
            MoveTo(newPos); // 영역 안이므로 이동
        }
    }

    // 목표(targetPos)를 향해 1칸 이동
    void MoveTowards(Vector2Int targetPos, Vector2Int currentPos)
    {
        int dx = targetPos.x - currentPos.x;
        int dy = targetPos.y - currentPos.y;
        Vector2Int moveDir = Vector2Int.zero;

        if (Mathf.Abs(dx) > Mathf.Abs(dy))
            moveDir.x = (dx > 0) ? 1 : -1;
        else if (Mathf.Abs(dy) > 0)
            moveDir.y = (dy > 0) ? 1 : -1;
        
        MoveTo(currentPos + moveDir);
    }

    // === 유틸리티 함수 ===
    Vector2Int GetCurrentPos() => new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
    Vector2Int GetPlayerPos() => new Vector2Int(Mathf.RoundToInt(player.position.x), Mathf.RoundToInt(player.position.y));
    int GetManhattanDist(Vector2Int a, Vector2Int b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    void MoveTo(Vector2Int newPos) => transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
}