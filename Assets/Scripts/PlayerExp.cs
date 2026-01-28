using UnityEngine;

public class PlayerExp : MonoBehaviour
{
    [Header("레벨 시스템")]
    public int level = 1;
    public int currentExp = 0;
    public int maxExp = 100; // 레벨업에 필요한 경험치

    // 적이 죽을 때 이 함수를 부릅니다
    public void GetExp(int amount)
    {
        currentExp += amount;

        // $ 표시 추가 완료 (숫자로 잘 보임)
        Debug.Log($"[경험치 획득] +{amount} (현재: {currentExp} / {maxExp})");

        // 경험치가 꽉 찼으면 레벨업! (한 번에 왕창 먹었을 때를 대비해 while문 사용)
        while (currentExp >= maxExp)
        {
            LevelUp();
        }
    }

    void LevelUp()
    {
        level++; // 레벨 1 증가

        currentExp = currentExp - maxExp; // 남은 경험치는 다음 레벨로 이월
        maxExp += 50; // 다음 레벨 난이도 증가 (50씩 더 필요해짐)

        // ★ [추가] 레벨업 시 공격력(bonusDamage) 증가 로직
        PlayerStats stats = GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.bonusDamage += 2; // 레벨업마다 추가 데미지 +2 (원하는 만큼 수정 가능)
            Debug.Log($" 성장 완료! 공격력이 강해졌습니다. (현재 추가뎀: {stats.bonusDamage})");
        }

        Debug.Log($" 레벨 업!! 현재 레벨: {level} (다음 레벨까지: {maxExp})");
    }
}