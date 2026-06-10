using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DangerUIHandler : MonoBehaviour {
    private const int MAX_SKULL_COUNT = 3;

    [Header("UI Components")]
    [SerializeField] private Image[] skullImages = new Image[MAX_SKULL_COUNT];

    [Header("Sprites")]
    [SerializeField] private Sprite filledSkull;
    [SerializeField] private Sprite emptySkull;

    [Header("Settings")]
    [SerializeField] private int dangerThresholdPerSkull = 10;
    [Range(0f, 1f)]
    [SerializeField] private float emptyAlpha = 0.2f;
    [SerializeField] private Color redColor = Color.red; 

    [Header("Animation Settings")]
    [SerializeField] private float punchScale = 1.3f;
    [SerializeField] private float animationDuration = 0.5f;

    private int _currentDangerValue = 0;
    private int _currentActiveSkullCount = -1;
    private bool _wasEmergency = false; 

private void Awake() {
        Image[] children = GetComponentsInChildren<Image>();
        int index = 0;
        foreach (var child in children) {
            if (child.gameObject == this.gameObject) continue;
            if (index >= MAX_SKULL_COUNT) break;
            skullImages[index] = child;
            index++;
        }
        
    }

    
    private void Start() {
        UpdateDangerUI(0); 
    }

    public void UpdateDangerUI(int currentDanger) {
        int newCount = 0;
        if (currentDanger > 0) {
            newCount = ((currentDanger - 1) / dangerThresholdPerSkull) + 1;
        }
        newCount = Mathf.Min(newCount, MAX_SKULL_COUNT);

        bool isEmergency = currentDanger >= 10000;

        if (_currentActiveSkullCount == newCount && _wasEmergency == isEmergency) return;

        for (int i = 0; i < MAX_SKULL_COUNT; i++) {
            if (skullImages[i] == null) continue;

            if (i < newCount) {
                if (i >= _currentActiveSkullCount) {
                    StartCoroutine(PunchAnimate(skullImages[i].transform));
                }

                skullImages[i].sprite = filledSkull;

                Color targetColor = isEmergency ? redColor : Color.white;
                targetColor.a = 1.0f; 
                skullImages[i].color = targetColor;
            }
            else {
                if (emptySkull != null) {
                    skullImages[i].sprite = emptySkull;
                    skullImages[i].color = Color.white;
                    SetAlpha(skullImages[i], 1.0f);
                }
                else {
                    skullImages[i].sprite = filledSkull;
                    skullImages[i].color = Color.white;
                    SetAlpha(skullImages[i], emptyAlpha);
                }
                skullImages[i].transform.localScale = Vector3.one;
            }
        }

  
        if (BattleStateBGM.Instance != null)
        {
            BattleStateBGM.ThreatLevel targetState = BattleStateBGM.ThreatLevel.Normal;
            
            if (isEmergency) targetState = BattleStateBGM.ThreatLevel.Boss;
            else if (newCount >= 3) targetState = BattleStateBGM.ThreatLevel.Combat;  
            else if (newCount > 0) targetState = BattleStateBGM.ThreatLevel.Tension; 

            if (BattleStateBGM.Instance.currentLevel != targetState)
            {
                BattleStateBGM.Instance.SetBattleState(targetState);
            }
        }

        _currentActiveSkullCount = newCount;
        _wasEmergency = isEmergency;

        if (LogManager.Instance != null) {
            LogManager.Instance.AddLog($"위험도 갱신: {currentDanger} (해골 {newCount}개)");
        }
    }

    private IEnumerator PunchAnimate(Transform target) {
        float elapsed = 0f;
        Vector3 initialScale = Vector3.one;
        Vector3 targetScale = Vector3.one * punchScale;

        while (elapsed < animationDuration * 0.5f) {
            elapsed += Time.deltaTime;
            target.localScale = Vector3.Lerp(initialScale, targetScale, elapsed / (animationDuration * 0.5f));
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < animationDuration * 0.5f) {
            elapsed += Time.deltaTime;
            target.localScale = Vector3.Lerp(targetScale, initialScale, elapsed / (animationDuration * 0.5f));
            yield return null;
        }

        target.localScale = initialScale;
    }

    private void SetAlpha(Image img, float alpha) {
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }
}