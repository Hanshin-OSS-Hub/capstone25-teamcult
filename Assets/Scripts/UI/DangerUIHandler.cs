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

    private int _currentDangerValue = 0; // 위험도
    private int _currentActiveSkullCount = -1;// 해골 개수, 처음에 초기화해야되서 -1로 설정, Awake에서 0이 됨

    private void Awake() {
        Image[] children = GetComponentsInChildren<Image>();
        int index = 0;
        foreach (var child in children) {
            if (child.gameObject == this.gameObject) continue;
            if (index >= MAX_SKULL_COUNT) break;
            skullImages[index] = child;
            index++;
        }

        // 초기 UI 상태를 0개로 설정
        UpdateDangerUI(0);
    }

    private void Update() {
        // 테스트 입력 로직
        // 숫자 5: 위험도 랜덤 감소 (1~5)
        if (Input.GetKeyDown(KeyCode.Alpha5)) {
            int randomDecrease = Random.Range(1, 6); // 1~5 사이의 정수
            ModifyDanger(-randomDecrease);
        }

        // 숫자 6: 위험도 랜덤 증가 (1~5)
        if (Input.GetKeyDown(KeyCode.Alpha6)) {
            int randomIncrease = Random.Range(1, 6); // 1~5 사이의 정수
            ModifyDanger(randomIncrease);
        }
    }

    private void ModifyDanger(int amount) {
        int maxDanger = MAX_SKULL_COUNT * dangerThresholdPerSkull;

        // 위험도 가감 및 범위 제한 (0 ~ 30)
        _currentDangerValue = Mathf.Clamp(_currentDangerValue + amount, 0, maxDanger);

        // 어떤 값이 얼마나 변했는지 확인하기 위한 로그
        string direction = amount > 0 ? "증가" : "감소";
        Debug.Log($"위험도 {direction}: {Mathf.Abs(amount)} | 현재 총 위험도: {_currentDangerValue}");

        UpdateDangerUI(_currentDangerValue);
    }

    public void UpdateDangerUI(int currentDanger) {
        // 새로운 해골 개수 계산
        int newCount = 0;
        if (currentDanger > 0) {
            newCount = ((currentDanger - 1) / dangerThresholdPerSkull) + 1;
        }
        newCount = Mathf.Min(newCount, MAX_SKULL_COUNT);

        // 이전 개수와 동일하면 아무것도 하지 않고 리턴 (최적화)
        if (_currentActiveSkullCount == newCount) return;

        // UI 갱신 로직 실행
        for (int i = 0; i < MAX_SKULL_COUNT; i++) {
            if (skullImages[i] == null) continue;

            if (i < newCount) {
                skullImages[i].sprite = filledSkull;
                SetAlpha(skullImages[i], 1.0f);
            }
            else {
                if (emptySkull != null) {
                    skullImages[i].sprite = emptySkull;
                    SetAlpha(skullImages[i], 1.0f);
                }
                else {
                    skullImages[i].sprite = filledSkull;
                    SetAlpha(skullImages[i], emptyAlpha);
                }
            }
        }

        // 로그 매니저 호출
        if (LogManager.Instance != null) {
            LogManager.Instance.AddLog($"위험도 변화: 해골이 {newCount}개로 변했습니다.");
        }

        // 현재 개수 업데이트
        _currentActiveSkullCount = newCount;
    }

    private void SetAlpha(Image img, float alpha) {
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }
}