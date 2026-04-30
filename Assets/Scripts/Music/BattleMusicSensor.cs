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
        int enemyCount = 0; // ★ 적 마릿수 카운트

        for (int i = 0; i < count; i++)
        {
            if (hitBuffer[i] != null && hitBuffer[i].CompareTag("Enemy"))
            {
                string objName = hitBuffer[i].gameObject.name;

                if (objName.Contains("Boss") || objName.Contains("Devil 2"))
                {
                    foundBoss = true;
                    break; 
                }
                else if (objName.Contains("EnemyAI") || objName.Contains("RangedEnemy") || objName.Contains("Devil"))
                {
                    enemyCount++; // 발견된 일반 적 마릿수 누적
                }
            }
        }

        if (BattleStateBGM.Instance != null)
        {
            RoomState targetState = RoomState.Normal;
            
            if (foundBoss) targetState = RoomState.Boss;
            else if (enemyCount >= 3) targetState = RoomState.Combat;  // 3마리 이상: Combat (전투)
            else if (enemyCount > 0) targetState = RoomState.Tension; // 1~2마리: Tension (긴장)

            if (BattleStateBGM.Instance.currentState != targetState)
            {
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