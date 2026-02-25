using UnityEngine;

public class BattleMusicSensor : MonoBehaviour
{
    [Header("Detection Settings")]
    public Transform player;
    public float detectionRadius = 15.0f; 

    private Collider2D[] hitBuffer = new Collider2D[20];
    private ContactFilter2D contactFilter;

    void Start()
    {
        contactFilter = new ContactFilter2D();
        contactFilter.NoFilter();
    }

    void Update()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
            else return; 
        }

        int count = Physics2D.OverlapCircle(player.position, detectionRadius, contactFilter, hitBuffer);
        
        bool foundBoss = false;
        bool foundCombat = false;

        for (int i = 0; i < count; i++)
        {
            if (hitBuffer[i] != null)
            {
                // ★ 1. 콜라이더가 잡힌 오브젝트 중에 "Enemy" 태그를 가진 놈이 있는지 확인
                if (hitBuffer[i].CompareTag("Enemy"))
                {
                    string objName = hitBuffer[i].gameObject.name;
                    
                    // 🚨 여기에 찍히는 이름을 정확히 봐야 합니다!
                    Debug.Log($"👀 [레이더] Enemy 태그 감지됨! 실제 이름: {objName}");

                    // C#은 대소문자와 띄어쓰기를 엄격하게 구분합니다.
                    if (objName.Contains("RangedEnemy"))
                    {
                        foundBoss = true;
                        break; 
                    }
                    else if (objName.Contains("EnemyAI"))
                    {
                        foundCombat = true;
                    }
                }
            }
        }

        if (BattleStateBGM.Instance != null)
        {
            RoomState targetState = RoomState.Normal;
            if (foundBoss) targetState = RoomState.Boss;
            else if (foundCombat) targetState = RoomState.Combat;

            if (BattleStateBGM.Instance.currentState != targetState)
            {
                // 로그 메시지를 명확하게 수정했습니다.
                if (targetState == RoomState.Normal) 
                    Debug.Log("🕊️ [음악 변경] 주변에 적이 없습니다. Normal 모드로 전환!");
                else 
                    Debug.Log($"🔥 [음악 변경] {targetState} 모드로 전환!");

                BattleStateBGM.Instance.SetBattleState(targetState);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawSphere(player.position, detectionRadius);
        }
    }
}