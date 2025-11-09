using UnityEngine;
using System.Collections; // 코루틴을 위해 필요

public class PlayerMove : MonoBehaviour
{
    private TurnManage turnManager; 
    private bool isMyTurn = true; 
    
    public int moveDistance = 1; // 1칸 이동

    // === 애니메이션 변수 ===
    private Animator anim; 
    private readonly int IsMovingHash = Animator.StringToHash("IsMoving"); 
    
    [Header("게임 설정")]
    [Tooltip("달리기 애니메이션이 최소한 재생되는 시간 (예: 0.2초)")]
    public float moveAnimDuration = 0.2f; 

    void Start()
    {
        // 턴 매니저 연결
        turnManager = FindAnyObjectByType<TurnManage>();
        
        if (turnManager == null)
        {
            Debug.LogError("🚨 PlayerMove: 씬에서 TurnManage 오브젝트(스크립트)를 찾을 수 없습니다!");
        }

        // 애니메이터 찾기
        if (transform.childCount > 0)
        {
            anim = transform.GetChild(0).GetComponent<Animator>();
        }
        if (anim == null)
        {
            Debug.LogWarning(gameObject.name + ": Animator 컴포넌트를 찾을 수 없습니다!");
        }
        
        // 위치 맞춤 (정수 단위)
        transform.position = new Vector3(Mathf.RoundToInt(transform.position.x), 
                                         Mathf.RoundToInt(transform.position.y), 
                                         transform.position.z);

        // 첫 턴 시작
        StartMyTurn(); 
    }

    void Update()
    {
        // 턴이 아니면 입력 처리 무시
        if (!isMyTurn)
            return;
        
        Vector3 moveDirection = Vector3.zero;

        // WASD 및 방향키 입력 감지
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            moveDirection = Vector3.up;
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            moveDirection = Vector3.down;
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            moveDirection = Vector3.left;
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            moveDirection = Vector3.right;

        // 이동 명령이 있을 때만 처리
        if (moveDirection != Vector3.zero)
        {
            // 턴 처리를 코루틴에 맡김
            StartCoroutine(MoveAndAnimate(moveDirection));
        }
    }

    // 이동 및 애니메이션 처리 코루틴
    IEnumerator MoveAndAnimate(Vector3 moveDirection)
    {
        // 1. 턴이 시작되자마자 입력 방지
        isMyTurn = false; 

        // 2. 이동할 다음 위치 계산
        Vector3 newPos = transform.position + moveDirection * moveDistance;

        // 3. [이동!]
        
        // 3a. 달리기 애니메이션 켜기
        if (anim != null)
            anim.SetBool(IsMovingHash, true);

        // 3b. 실제 위치 이동
        transform.position = newPos;
        
        // 3c. 애니메이션이 재생될 시간을 줌
        yield return new WaitForSeconds(moveAnimDuration); 

        // 3d. 달리기 애니메이션 끄기 (Idle로 복귀)
        if (anim != null)
            anim.SetBool(IsMovingHash, false);

        // 4. 턴 종료
        EndMyTurn();
    }


    // 턴 종료 함수
    void EndMyTurn()
    {
        if (turnManager != null)
        {
            turnManager.EndPlayerTurn(); 
        }
    }
    
    // 턴 권한 받기
    public void StartMyTurn()
    {
        isMyTurn = true;
        Debug.Log("<color=green>플레이어 턴 시작!</color>"); 
    }
}