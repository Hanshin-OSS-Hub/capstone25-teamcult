using UnityEngine;
using System.Collections.Generic;
using System.Linq; 

public class TurnManage : MonoBehaviour
{
    private List<IEnemyTurn> allEnemies = new List<IEnemyTurn>(); 
    private PlayerMove playerMovement; 

    void Start()
    {
        playerMovement = FindObjectOfType<PlayerMove>();
        
        // 씬에 있는 모든 IEnemyTurn 구현 스크립트를 찾음
        allEnemies = FindObjectsOfType<MonoBehaviour>()
                           .OfType<IEnemyTurn>()
                           .ToList();

        // 찾은 모든 적에게 플레이어 정보를 자동으로 할당
        if (playerMovement != null)
        {
            foreach (IEnemyTurn enemy in allEnemies)
            {
                // EnemyMoveAI 타입에만 player를 할당
                if (enemy is EnemyMoveAI)
                {
                    EnemyMoveAI enemyAI = (EnemyMoveAI)enemy;
                    if (enemyAI.player == null) // 비어있을 경우에만 설정
                    {
                        enemyAI.player = playerMovement.transform;
                    }
                }
            }
        }
        else
        {
            Debug.LogError("TurnManage: 씬에서 PlayerMove 스크립트를 찾을 수 없습니다!");
        }
        
        Debug.Log($"게임 시작. 찾은 모든 적의 수: {allEnemies.Count}");

        // 플레이어 턴으로 게임 시작
        if (playerMovement != null)
        {
            playerMovement.StartMyTurn();
        }
    }

    // 플레이어 턴이 종료되면 호출됨
    public void EndPlayerTurn()
    {
        Debug.Log("--- 플레이어 턴 종료. 적 턴 시작! ---");
        
        foreach (IEnemyTurn enemy in allEnemies)
        {
            if (enemy != null)
            {
                enemy.DoEnemyTurn();
            }
        }
        
        // 모든 적 턴이 끝나면 다시 플레이어 턴 시작
        if (playerMovement != null)
        {
            playerMovement.StartMyTurn();
        }
        
        Debug.Log("적 턴 종료. 플레이어 턴으로 돌아갑니다.");
    }
}