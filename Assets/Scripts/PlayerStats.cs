using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("통합 스탯 관리소")]

    [Header("1. 이동 속도")]
    public float moveSpeed = 5f;

    [Header("2. 최대 체력")]
    public int maxHealth = 100;

    [Header("3. 공격력")]
    public float attackMultiplier = 1.0f;
    public int bonusDamage = 0;

    // ★ [NEW] 4. 방어력 추가!
    [Header("4. 방어력")]
    public int defense = 0; // 기본은 0 (안 아픔)

    // (아이템 먹으면 방어력 증가)
    public void AddDefense(int amount)
    {
        defense += amount;
        Debug.Log("방어력 증가! 현재 방어력: " + defense);
    }
}