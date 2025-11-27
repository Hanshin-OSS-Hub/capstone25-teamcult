using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HealthBarManager : MonoBehaviour {
    // 하트 프리팹
    [SerializeField] private GameObject emptyHeartPrefab;
    [SerializeField] private GameObject filledHeartPrefab;

    [SerializeField] private Transform emptyHeartsContainer; // 빈 하트들을 담을 부모 오브젝트
    [SerializeField] private Transform filledHeartsContainer; // 꽉찬 하트들을 담을 부모 오브젝트

    // === 체력 데이터 ===
    private int HP = 12; // 체력
    public int heart = 2; // 하트 개수
    private int maxHeart = 10; // 최대 하트 개수
    private int HPperHeart = 4; // 하트당 체력

    // 생성된 EmptyHeart 오브젝트들을 관리할 리스트
    private List<GameObject> emptyHeartObjects = new List<GameObject>();
    private List<GameObject> filledHeartObjects = new List<GameObject>();


    private void Start() {
        // maxHeart 값을 maxHP와 HP_PER_HEART를 기반으로 계산하여 설정 (선택 사항)
        // this.maxHeart = Mathf.CeilToInt((float)maxHP / HP_PER_HEART); 

        GenerateHearts();
    }

    // Z/X 키를 이용한 테스트 함수 (이전 답변에서 제공)
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Z)) {
            if (HP > 0) {
                LoseHP(1);
                Debug.Log("체력 감소! 현재 체력: " + HP);
            }
            else {
                Debug.Log("체력이 이미 0입니다.");
            }
        }
        if (Input.GetKeyDown(KeyCode.X)) {
            GainHP(1);
            Debug.Log("체력 증가! 현재 체력: " + HP);
        }
        if (Input.GetKeyDown(KeyCode.C) && heart >= 0) {
            ChangeHeartType(HeartAttribute.Ice, heart-1);
            Debug.Log("얼음하트");
        }
        if (Input.GetKeyDown(KeyCode.V) && heart >= 0) {
            ChangeHeartType(HeartAttribute.Fire, heart-1);
            Debug.Log("불하트");
        }
    }

    public void ChangeHeartType(HeartAttribute type, int index = 0) {
        if (index >= filledHeartObjects.Count) { return; }
        filledHeart heartComponent = filledHeartObjects[index].GetComponent<filledHeart>();
        heartComponent.SetAttribute(type);
    }

    private void GenerateHearts() {
        // 1. 기존 하트 모두 제거
        heart = 0;
        HP = 0;
        foreach (Transform child in emptyHeartsContainer) {
            Destroy(child.gameObject);
        }
        emptyHeartObjects.Clear();
        foreach (Transform child in filledHeartsContainer) {
            Destroy(child.gameObject);
        }
        filledHeartObjects.Clear();

        // 2. maxHeart 개수만큼 하트 생성 (빈 하트)
        AddHeart(2, true);
        AddHeart(1, false);
    }

    public int AddHeart(int cnt = 1, bool full = false) { // 추가할 하트 개수, 하트를 채울지
        int targetHeart = Mathf.Min(heart+cnt, maxHeart);
        for (; heart < targetHeart;) {
            GameObject emptyHeart = Instantiate(emptyHeartPrefab, emptyHeartsContainer);
            emptyHeartObjects.Insert(heart, emptyHeart); // emptyHeart 오브젝트 리스트에 추가
            GameObject filledheart = Instantiate(filledHeartPrefab, filledHeartsContainer);
            filledHeartObjects.Insert(heart, filledheart); // filledheart 오브젝트 리스트에 추가
            heart++;
            if (full) { GainHP(HPperHeart); }
        }
        return HP;
    }

    public int GainHP(int cnt) {
        Debug.Log("GainHP : " + cnt);
        for (int i = 0; i < heart; i++) {
            if (cnt == 0) { break; }
            Debug.Log(heart + ", " + filledHeartObjects.Count);
            filledHeart heartComponent = filledHeartObjects[i].GetComponent<filledHeart>();
            int t = Mathf.Min(cnt, HPperHeart - heartComponent.HP);
            heartComponent.HP += t;
            UpdateHeart(filledHeartObjects[i]);
            cnt -= t;
            HP += t;
        }
        return HP;
    }
    public int LoseHP(int cnt) {
        Debug.Log("LoseHP : " + cnt);
        for (int i = filledHeartObjects.Count-1; i >= 0; i--) {
            if (cnt == 0) { break; }
            filledHeart heartComponent = filledHeartObjects[i].GetComponent<filledHeart>();
            int t = Mathf.Min(cnt, heartComponent.HP);
            heartComponent.HP -= t;
            UpdateHeart(filledHeartObjects[i]);
            cnt -= t;
            HP -= t;
        }
        return HP;
    }
    private void UpdateHeart(GameObject filledHeart) {
        filledHeart heartComponent = filledHeart.GetComponent<filledHeart>();
        Image fillHeart = filledHeart.GetComponent<Image>();
        fillHeart.fillAmount = (float)heartComponent.HP / HPperHeart;

    }
    private void UpdateHealthBar() {
        int remainingHP = this.HP; // 남은 체력 (초과분 계산용)
        Debug.Log("체력바업데이트테스트");

        // emptyHeartObjects 리스트의 개수(최대 하트 개수)만큼 루프를 돕니다.
        for (int i = 0; i < emptyHeartObjects.Count; i++) {

            GameObject emptyHeartObject = emptyHeartObjects[i];

            Transform childTransformB = emptyHeartObject.transform.GetChild(0);

            if (childTransformB != null) {
                // 3. B의 Image콤포넌트를 찾아서 킬것
                Image fillImage = childTransformB.GetComponent<Image>();

                if (fillImage != null) {
                    // 2. 체력 계산 로직
                    int hpToFill = Mathf.Min(remainingHP, HPperHeart);

                    // FillAmount 계산 (0.0 ~ 1.0)
                    float fillAmount = (float)hpToFill / HPperHeart;

                    // 3. Image 컴포넌트에 적용
                    fillImage.fillAmount = fillAmount;

                    // 남은 체력 업데이트 (다음 하트 계산을 위해)
                    remainingHP -= HPperHeart;
                }
                else {
                    // 자식 Image를 찾지 못한 경우 (오류 방지)
                    remainingHP -= HPperHeart;
                    Debug.LogError(emptyHeartObject.name + "에서 Filled Heart Image를 찾을 수 없습니다.");
                }
            }


            // 남은 체력이 0 이하면 루프 종료
            if (remainingHP <= 0) {
                remainingHP = 0;
                // 남아있는 나머지 하트들은 fillAmount가 0이어야 하지만, 
                // 현재 체력 계산 로직상 0으로 유지되므로 별도의 처리는 필요 없습니다.
            }
        }
    }

    // 외부에서 호출될 체력 변경 함수 (예시)
    public void ChangeHealth(int newHealth) {
        HP = newHealth;
        UpdateHealthBar();
    }
}