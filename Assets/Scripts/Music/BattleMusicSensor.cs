using UnityEngine;
using System.Collections.Generic; 

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

        Collider2D[] hits = Physics2D.OverlapCircleAll(player.position, detectionRadius);
        
        bool foundBoss = false;
        HashSet<GameObject> uniqueEnemies = new HashSet<GameObject>();

        foreach (Collider2D hit in hits)
        {
            if (hit != null && hit.CompareTag("Enemy"))
            {
                string objName = hit.gameObject.name;

                if (objName.Contains("Boss") || objName.Contains("Devil 2"))
                {
                    foundBoss = true;
                    break; 
                }
                else 
                {
                    uniqueEnemies.Add(hit.gameObject); 
                }
            }
        }

        int trueEnemyCount = uniqueEnemies.Count;

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