using UnityEngine;
using System.IO;
using System.Collections.Generic;
[System.Serializable]
public class OopartsSaveData
{
    public int availablePoints;
    public List<string> pickedSlotKeys = new List<string>(); // "treeIndex_row_name"
}
public class OopartsSaveManager : MonoBehaviour
{
    public static OopartsSaveManager instance;
    private string savePath => Application.persistentDataPath + "/ooparts_save.json";
    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }
    void Start()
    {
        // 게임 시작 시 항상 마석 데이터 불러오기 (런 관계없이 영구 유지)
        LoadOoparts();
    }
    public void SaveOoparts()
    {
        if (OopartsTreeManager.instance == null) return;
        OopartsSaveData data = new OopartsSaveData
        {
            availablePoints = OopartsTreeManager.instance.availablePoints
        };
        foreach (var slot in OopartsTreeManager.instance.allSlots)
        {
            if (slot == null || !slot.isPicked || slot.oopartsData == null) continue;
            string key = $"{slot.GetTree()}_{slot.GetRow()}_{slot.oopartsData.oopartsName}";
            data.pickedSlotKeys.Add(key);
        }
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log("[OopartsSave] 마석 저장 완료");
    }
    public void LoadOoparts()
    {
        if (!File.Exists(savePath)) return;
        if (OopartsTreeManager.instance == null) return;
        string json = File.ReadAllText(savePath);
        OopartsSaveData data = JsonUtility.FromJson<OopartsSaveData>(json);
        OopartsTreeManager.instance.availablePoints = data.availablePoints;
        foreach (var slot in OopartsTreeManager.instance.allSlots)
        {
            if (slot == null || slot.oopartsData == null) continue;
            string key = $"{slot.GetTree()}_{slot.GetRow()}_{slot.oopartsData.oopartsName}";
            if (data.pickedSlotKeys.Contains(key))
            {
                slot.isPicked = true;
                slot.Refresh();
                // 스탯 직접 적용 (ForcePick 대신, 중복 방지)
                if (PlayerStats.instance != null)
                {
                    var s = PlayerStats.instance;
                    var d = slot.oopartsData;
                    s.bonusAttack += d.bonusAttack;
                    s.bonusDefense += d.bonusDefense;
                    s.bonusHealth += d.bonusHealth;
                    s.moveSpeed += d.bonusMoveSpeed;
                    s.bonusAttackPercent += d.bonusAttackPercent;
                    s.bonusAttackSpeed += d.bonusAttackSpeed;
                    s.bonusAttackRange += d.bonusAttackRange;
                    s.critChance += d.bonusCritChance;
                    s.expMultiplier += d.bonusExpMultiplier / 100f;
                    s.invincibilityBonus += d.bonusInvincibility;
                    s.damageNullifyChance += d.bonusDamageNullify;
                    s.killMoveSpeedStack += d.bonusKillMoveSpeed;
                    s.killGoldChance += d.bonusKillGoldChance;
                    s.killGoldAmount += d.bonusKillGoldAmount;
                    if (d.enableBerserker) s.berserkerMode = true;
                    if (d.enableFourthAttackBonus) s.everyFourthAttackBonus = true;
                }
            }
        }
        OopartsTreeManager.instance.RefreshAllTrees();
        OopartsTreeManager.instance.UpdatePointUI();
        Debug.Log("[OopartsSave] 마석 불러오기 완료");
    }
    public void DeleteOoparts()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("[OopartsSave] 마석 데이터 삭제");
        }
    }
    // 완전 초기화 (파일 + 메모리 둘 다)
    public void ResetAll()
    {
        DeleteOoparts();

        if (OopartsTreeManager.instance != null)
        {
            OopartsTreeManager.instance.availablePoints = 0;

            foreach (var slot in OopartsTreeManager.instance.allSlots)
            {
                if (slot != null) slot.isPicked = false;
            }

            OopartsTreeManager.instance.RefreshAllTrees();
            OopartsTreeManager.instance.UpdatePointUI();
        }

        Debug.Log("[OopartsSave] 완전 초기화 완료");
    }
}