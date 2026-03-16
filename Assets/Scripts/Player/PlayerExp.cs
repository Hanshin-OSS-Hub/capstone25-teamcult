using UnityEngine;

public class PlayerExp : MonoBehaviour
{
    [Header("경험치 시스템")]
    public int level = 1;
    public int currentExp = 0;
    public int maxExp = 100;

    public void GetExp(int amount)
    {
        currentExp += amount;
        Debug.Log($"[경험치 획득] +{amount} (현재: {currentExp} / {maxExp})");

        while (currentExp >= maxExp)
        {
            LevelUp();
        }
    }

    void LevelUp()
    {
        level++;
        currentExp = currentExp - maxExp;
        maxExp += 50;

        // 기존 - bonusDamage 증가
        PlayerStats stats = GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.bonusDamage += 2;
        }

        // ? 추가 - 오파츠 포인트 1 지급 (레벨업마다)
        if (OopartsTreeManager.instance != null)
        {
            OopartsTreeManager.instance.AddPoint(1);
        }

        Debug.Log($"레벨 업!! 현재 레벨: {level} | 오파츠 포인트 +1 지급!");
    }
}