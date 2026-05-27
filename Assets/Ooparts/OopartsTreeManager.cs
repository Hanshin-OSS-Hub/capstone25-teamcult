using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class OopartsTreeManager : MonoBehaviour
{
    public static OopartsTreeManager instance;

    [Header("전체 슬롯 (트리/행 순서대로 할당)")]
    public List<OopartsSlot> allSlots = new List<OopartsSlot>();

    [Header("포인트 UI")]
    public TextMeshProUGUI pointText;

    [Header("현재 포인트")]
    public int availablePoints = 0;

    private void Awake() { instance = this; }

    private void Start()
    {
        RefreshAllTrees();
        UpdatePointUI();
    }

    public void AddPoint(int amount = 1)
    {
        availablePoints += amount;
        UpdatePointUI();
        // 마석 획득 시 자동 저장
        if (OopartsSaveManager.instance != null) OopartsSaveManager.instance.SaveOoparts();
        Debug.Log($"[Ooparts] Point +{amount} (Total: {availablePoints})");
    }

    public void UpdatePointUI()
    {
        if (pointText != null)
            pointText.text = $"Point: {availablePoints}";
    }

    public void TryPick(OopartsSlot target)
    {
        if (availablePoints <= 0)
        {
            Debug.Log("[Ooparts] Not enough points!");
            return;
        }

        int tree = target.GetTree();
        int row = target.GetRow();

        foreach (var slot in allSlots)
        {
            if (slot == null) continue;
            if (slot.GetTree() == tree && slot.GetRow() == row && slot.isPicked)
            {
                Debug.Log("[Ooparts] Already picked in this row! Cannot change.");
                return;
            }
        }

        availablePoints--;
        target.ForcePick();
        UpdatePointUI();
        RefreshAllTrees();

        // 능력 선택 시 자동 저장
        if (OopartsSaveManager.instance != null) OopartsSaveManager.instance.SaveOoparts();

        Debug.Log($"[Ooparts] {target.oopartsData.oopartsName} picked! (Points left: {availablePoints})");
    }

    public void RefreshAllTrees()
    {
        Dictionary<int, Dictionary<int, List<OopartsSlot>>> treeRowMap
            = new Dictionary<int, Dictionary<int, List<OopartsSlot>>>();

        foreach (var slot in allSlots)
        {
            if (slot == null) continue;
            int t = slot.GetTree();
            int r = slot.GetRow();

            if (!treeRowMap.ContainsKey(t))
                treeRowMap[t] = new Dictionary<int, List<OopartsSlot>>();
            if (!treeRowMap[t].ContainsKey(r))
                treeRowMap[t][r] = new List<OopartsSlot>();

            treeRowMap[t][r].Add(slot);
        }

        foreach (var treePair in treeRowMap)
        {
            var rowMap = treePair.Value;

            int maxRow = 0;
            foreach (var key in rowMap.Keys)
                if (key > maxRow) maxRow = key;

            for (int r = 0; r <= maxRow; r++)
            {
                if (!rowMap.ContainsKey(r)) continue;

                bool unlocked = true;
                if (r > 0)
                {
                    unlocked = false;
                    if (rowMap.ContainsKey(r - 1))
                    {
                        foreach (var slot in rowMap[r - 1])
                        {
                            if (slot.isPicked) { unlocked = true; break; }
                        }
                    }
                }

                foreach (var slot in rowMap[r])
                    slot.SetUnlocked(unlocked);
            }
        }
    }
}