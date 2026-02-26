using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [Header("Basic Info")]
    public string enemyName;    // 이름 (고블린, 오크 등)
    public int level = 1;       // 몬스터 레벨 (나중에 스케일링용)

    [Header("Combat Stats")]
    public int maxHealth = 30;
    public int damage = 5;      // 공격력
    public float moveSpeed = 2f;// 이동 속도
    public int defense = 0;     // 방어력 (제산식 적용)

    [Header("Rewards")]
    public int expReward = 10;  // 처치 시 주는 경험치

    // 나중에 시간이 지나면 적을 강화시키는 함수
    public void ScaleStats(int stageLevel)
    {
        // 예: 스테이지가 오를 때마다 체력 10%, 공격력 1씩 증가
        maxHealth += (int)(maxHealth * 0.1f * stageLevel);
        damage += stageLevel;
        level = stageLevel;
    }
}