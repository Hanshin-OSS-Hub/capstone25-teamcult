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
            if (hitBuffer[i] != null && hitBuffer[i].CompareTag("Enemy"))
            {
                string objName = hitBuffer[i].gameObject.name;

                // ★ 1. 보스 인식 (이름에 Boss나 Devil 2가 포함될 때)
                if (objName.Contains("Boss") || objName.Contains("Devil 2"))
                {
                    foundBoss = true;
                    break; 
                }
                // ★ 2. 일반 전투 인식 (EnemyAI, RangedEnemy, Devil 등)
                else if (objName.Contains("EnemyAI") || objName.Contains("RangedEnemy") || objName.Contains("Devil"))
                {
                    foundCombat = true;
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
                Debug.Log($"🔥 [레이더] 상태를 {targetState} (으)로 변경합니다.");
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