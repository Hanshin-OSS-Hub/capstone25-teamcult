using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [Header("Basic Info")]
    public string enemyName;    
    public int level = 1;       

    [Header("Combat Stats")]
    public int maxHealth = 30;
    public int damage = 5;      // 공격력
    public float moveSpeed = 2f;// 이동 속도
    public int defense = 0;     // 방어력 (제산식 적용)
    public int danger = 5;     // 위험도 



    public void ScaleStats(int stageLevel)
    {
        maxHealth += (int)(maxHealth * 0.1f * stageLevel);
        damage += stageLevel;
        level = stageLevel;
    }
}