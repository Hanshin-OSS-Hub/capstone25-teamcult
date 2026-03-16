using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OopartsTooltip : MonoBehaviour
{
    public static OopartsTooltip instance;

    [Header("툴팁 UI")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public TextMeshProUGUI statText;
    public Image iconImage;

    private RectTransform tooltipRect;
    private Canvas parentCanvas;

    private void Awake()
    {
        instance = this;
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
            tooltipRect = tooltipPanel.GetComponent<RectTransform>();
            parentCanvas = tooltipPanel.GetComponentInParent<Canvas>();
        }
    }

    private void Update()
    {
        if (tooltipPanel == null || !tooltipPanel.activeSelf || tooltipRect == null) return;

        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.GetComponent<RectTransform>(),
            Input.mousePosition,
            parentCanvas.worldCamera,
            out pos
        );

        float w = tooltipRect.rect.width;
        float h = tooltipRect.rect.height;

        RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();
        float canvasW = canvasRect.rect.width;
        float canvasH = canvasRect.rect.height;

        // 기본: 오른쪽에 표시
        float offsetX = w + 10f;
        float offsetY = 0f;

        // 오른쪽 벗어나면 왼쪽에 표시
        if (pos.x + offsetX + w > canvasW / 2f)
            offsetX = -w - 10f;

        // 아래쪽 벗어나면 위로 올림
        if (pos.y + offsetY - h < -canvasH / 2f)
            offsetY = h;

        tooltipRect.anchoredPosition = pos + new Vector2(offsetX, offsetY);
    }

    public void Show(OopartsData data)
    {
        if (data == null || tooltipPanel == null) return;

        tooltipPanel.SetActive(true);

        if (nameText != null) nameText.text = data.oopartsName;
        if (descText != null) descText.text = data.description;
        if (iconImage != null && data.icon != null) iconImage.sprite = data.icon;

        string stats = "";
        if (data.bonusAttack != 0) stats += $"ATK +{data.bonusAttack}\n";
        if (data.bonusDefense != 0) stats += $"DEF +{data.bonusDefense}\n";
        if (data.bonusHealth != 0) stats += $"HP +{data.bonusHealth}\n";
        if (data.bonusMoveSpeed != 0) stats += $"SPD +{data.bonusMoveSpeed}\n";
        if (data.bonusAttackPercent != 0) stats += $"ATK% +{data.bonusAttackPercent}%\n";
        if (data.bonusAttackSpeed != 0) stats += $"ASPD +{data.bonusAttackSpeed}%\n";
        if (data.bonusAttackRange != 0) stats += $"Range +{data.bonusAttackRange}\n";
        if (data.bonusCritChance != 0) stats += $"Crit +{data.bonusCritChance}%\n";
        if (data.bonusExpMultiplier != 0) stats += $"EXP +{data.bonusExpMultiplier}%\n";
        if (data.bonusInvincibility != 0) stats += $"Invincibility +{data.bonusInvincibility}s\n";
        if (data.bonusDamageNullify != 0) stats += $"Nullify {data.bonusDamageNullify}%\n";
        if (data.bonusKillMoveSpeed != 0) stats += $"Kill SPD +{data.bonusKillMoveSpeed}\n";
        if (data.bonusKillGoldChance != 0) stats += $"Gold Chance {data.bonusKillGoldChance}%\n";
        if (data.bonusKillGoldAmount != 0) stats += $"Gold +{data.bonusKillGoldAmount}\n";
        if (data.enableBerserker) stats += $"Berserker Mode\n";
        if (data.enableFourthAttackBonus) stats += $"4th Attack x2\n";

        if (statText != null) statText.text = stats.TrimEnd('\n');
    }

    public void Hide()
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }
}