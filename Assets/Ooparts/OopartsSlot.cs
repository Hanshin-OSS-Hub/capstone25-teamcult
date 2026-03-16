using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class OopartsSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("오파츠 데이터")]
    public OopartsData oopartsData;

    [Header("UI 참조")]
    public Image backgroundImage;
    public Image iconImage;
    public GameObject selectedOutline;
    public GameObject lockOverlay;

    [Header("발광 효과 색상")]
    public Color glowColor = new Color(1.0f, 0.85f, 0.20f, 0.8f);

    [HideInInspector] public bool isPicked = false;
    [HideInInspector] public bool isUnlocked = false;

    private Shadow[] shadows;

    private void Awake()
    {
        shadows = GetComponents<Shadow>();
        if (shadows.Length == 0)
        {
            for (int i = 0; i < 3; i++)
                gameObject.AddComponent<Shadow>();
            shadows = GetComponents<Shadow>();
        }
        SetGlow(false);
    }

    private void Start()
    {
        Refresh();
    }

    void SetGlow(bool on)
    {
        if (shadows == null) return;
        Color c = on ? glowColor : new Color(0, 0, 0, 0);
        float[] distances = { 4f, 7f, 10f };
        for (int i = 0; i < shadows.Length && i < distances.Length; i++)
        {
            shadows[i].effectColor = c;
            shadows[i].effectDistance = new Vector2(distances[i], -distances[i]);
        }
    }

    public void SetUnlocked(bool unlocked)
    {
        if (!unlocked && isPicked)
            ForceUnpick();
        isUnlocked = unlocked;
        Refresh();
    }

    public void Refresh()
    {
        if (backgroundImage != null)
            // 찍은 슬롯만 밝게, 나머지는 어둡게
            backgroundImage.color = isPicked ? Color.white : new Color(0.3f, 0.3f, 0.3f, 1f);

        if (iconImage != null)
        {
            iconImage.sprite = oopartsData != null ? oopartsData.icon : null;
            iconImage.enabled = oopartsData != null && oopartsData.icon != null;
            iconImage.color = Color.white;
        }

        if (lockOverlay != null) lockOverlay.SetActive(!isUnlocked);
        if (selectedOutline != null) selectedOutline.SetActive(isPicked && isUnlocked);

        SetGlow(isPicked && isUnlocked);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isUnlocked || oopartsData == null) return;
        if (isPicked) return;
        OopartsTreeManager.instance.TryPick(this);
    }

    public void ForcePick()
    {
        if (isPicked || oopartsData == null) return;
        isPicked = true;

        if (PlayerStats.instance != null)
        {
            var s = PlayerStats.instance;
            s.bonusAttack += oopartsData.bonusAttack;
            s.bonusDefense += oopartsData.bonusDefense;
            s.bonusHealth += oopartsData.bonusHealth;
            s.moveSpeed += oopartsData.bonusMoveSpeed;
            s.bonusAttackPercent += oopartsData.bonusAttackPercent;
            s.bonusAttackSpeed += oopartsData.bonusAttackSpeed;
            s.bonusAttackRange += oopartsData.bonusAttackRange;
            s.critChance += oopartsData.bonusCritChance;
            s.expMultiplier += oopartsData.bonusExpMultiplier / 100f;
            s.invincibilityBonus += oopartsData.bonusInvincibility;
            s.damageNullifyChance += oopartsData.bonusDamageNullify;
            s.killMoveSpeedStack += oopartsData.bonusKillMoveSpeed;
            s.killGoldChance += oopartsData.bonusKillGoldChance;
            s.killGoldAmount += oopartsData.bonusKillGoldAmount;
            if (oopartsData.enableBerserker) s.berserkerMode = true;
            if (oopartsData.enableFourthAttackBonus) s.everyFourthAttackBonus = true;
            s.UpdateStatUI();
        }
        Refresh();
    }

    public void ForceUnpick()
    {
        if (!isPicked || oopartsData == null) return;
        isPicked = false;

        if (PlayerStats.instance != null)
        {
            var s = PlayerStats.instance;
            s.bonusAttack -= oopartsData.bonusAttack;
            s.bonusDefense -= oopartsData.bonusDefense;
            s.bonusHealth -= oopartsData.bonusHealth;
            s.moveSpeed -= oopartsData.bonusMoveSpeed;
            s.bonusAttackPercent -= oopartsData.bonusAttackPercent;
            s.bonusAttackSpeed -= oopartsData.bonusAttackSpeed;
            s.bonusAttackRange -= oopartsData.bonusAttackRange;
            s.critChance -= oopartsData.bonusCritChance;
            s.expMultiplier -= oopartsData.bonusExpMultiplier / 100f;
            s.invincibilityBonus -= oopartsData.bonusInvincibility;
            s.damageNullifyChance -= oopartsData.bonusDamageNullify;
            s.killMoveSpeedStack -= oopartsData.bonusKillMoveSpeed;
            s.killGoldChance -= oopartsData.bonusKillGoldChance;
            s.killGoldAmount -= oopartsData.bonusKillGoldAmount;
            if (oopartsData.enableBerserker) s.berserkerMode = false;
            if (oopartsData.enableFourthAttackBonus) s.everyFourthAttackBonus = false;
            s.UpdateStatUI();
        }
        Refresh();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (oopartsData == null) return; // isUnlocked 조건 제거
        OopartsTooltip.instance?.Show(oopartsData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OopartsTooltip.instance?.Hide();
    }

    public int GetTree() => oopartsData != null ? oopartsData.treeIndex : -1;
    public int GetRow() => oopartsData != null ? oopartsData.row : -1;
}