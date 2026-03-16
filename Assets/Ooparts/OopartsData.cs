using UnityEngine;

[CreateAssetMenu(fileName = "NewOoparts", menuName = "Ooparts/OopartsData")]
public class OopartsData : ScriptableObject
{
    [Header("기본 정보")]
    public string oopartsName = "Ooparts Name";
    public Sprite icon;
    [TextArea] public string description = "Description";

    [Header("고정 스탯 보너스")]
    public int bonusAttack = 0;
    public int bonusDefense = 0;
    public int bonusHealth = 0;
    public float bonusMoveSpeed = 0f;

    [Header("퍼센트 스탯 보너스")]
    public float bonusAttackPercent = 0f;
    public float bonusAttackSpeed = 0f;

    [Header("특수 스탯")]
    public float bonusAttackRange = 0f;         // 사거리 보너스
    public float bonusCritChance = 0f;          // 치명타 확률 (%)
    public float bonusExpMultiplier = 0f;       // 경험치 획득 배율 (%)
    public float bonusInvincibility = 0f;       // 피격 무적시간 보너스
    public float bonusDamageNullify = 0f;       // 데미지 무효 확률 (%)
    public float bonusKillMoveSpeed = 0f;       // 적 처치 시 이동속도 중첩
    public float bonusKillGoldChance = 0f;      // 적 처치 시 골드 획득 확률 (%)
    public int bonusKillGoldAmount = 0;         // 적 처치 시 골드 획득량
    public bool enableBerserker = false;        // 광전사 모드
    public bool enableFourthAttackBonus = false; // 4번째 공격 데미지 2배

    [Header("트리 설정")]
    public int treeIndex = 0;
    public int row = 0;
}