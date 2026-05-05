using UnityEngine;
using System.Collections.Generic; // ★ 이 줄이 추가되었습니다! (HashSet을 쓰기 위한 필수 선언)

public class BattleMusicSensor : MonoBehaviour
{
    [Header("탐지 설정 (범위)")]
    public Transform player;
    public float detectionRadius = 25.0f; 

    void Update()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
            else return; 
        }

        // 범위 내의 모든 콜라이더 스캔
        Collider2D[] hits = Physics2D.OverlapCircleAll(player.position, detectionRadius);
        
        bool foundBoss = false;
        
        // ★ [핵심 수정] 콜라이더가 아니라 '고유한 적 오브젝트' 자체를 담는 주머니 생성
        HashSet<GameObject> uniqueEnemies = new HashSet<GameObject>();

        foreach (Collider2D hit in hits)
        {
            if (hit != null && hit.CompareTag("Enemy"))
            {
                // (선택 사항) 만약 적 체력 스크립트가 있다면, 죽은 시체는 세지 않도록 방어
                // EnemyHealth hp = hit.GetComponentInParent<EnemyHealth>();
                // if (hp != null && hp.currentHealth <= 0) continue; 

                string objName = hit.gameObject.name;

                if (objName.Contains("Boss") || objName.Contains("Devil 2"))
                {
                    foundBoss = true;
                    break; 
                }
                else if (objName.Contains("EnemyAI") || objName.Contains("RangedEnemy") || objName.Contains("Devil"))
                {
                    // 오브젝트 자체를 넣으므로, 한 놈의 콜라이더가 5개가 걸려도 주머니엔 1개만 들어감
                    uniqueEnemies.Add(hit.gameObject); 
                }
            }
        }

        // 주머니에 들어있는 진짜 적의 마릿수
        int trueEnemyCount = uniqueEnemies.Count;

        // 위협도 판정 및 브금 변경
        if (BattleStateBGM.Instance != null)
        {
            BattleStateBGM.ThreatLevel targetState = BattleStateBGM.ThreatLevel.Normal;
            
            if (foundBoss) targetState = BattleStateBGM.ThreatLevel.Boss;
            else if (trueEnemyCount >= 3) targetState = BattleStateBGM.ThreatLevel.Combat;  
            else if (trueEnemyCount > 0) targetState = BattleStateBGM.ThreatLevel.Tension; 

            if (BattleStateBGM.Instance.currentLevel != targetState)
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