using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HealthBarManager : MonoBehaviour {
    // === 인스펙터 설정 ===

    // 빈 하트(테두리) 프리팹 - 하트 4칸을 표현하는 Image 컴포넌트 (Type: Simple)
    public GameObject emptyHeartPrefab;

    // Fill 이미지 프리팹 (필요한 경우) - 보통 emptyHeartPrefab의 자식으로 구성됨 (Type: Filled)
    public Transform heartsContainer; // 하트들을 담을 부모 오브젝트

    // === 체력 데이터 ===
    private int maxHP = 12; // 최대 체력
    private int HP = 12; // 현재 체력 수치
    private const int HP_PER_HEART = 4; // 하트 1개당 체력 값 (고정)
    private int maxHeart = 3; // maxHP/HP_PER_HEART; 하트의 최대 개수

    // 생성된 EmptyHeart 오브젝트들을 관리할 리스트
    public List<GameObject> emptyHeartObjects = new List<GameObject>();


    private void Start() {
        // maxHeart 값을 maxHP와 HP_PER_HEART를 기반으로 계산하여 설정 (선택 사항)
        // this.maxHeart = Mathf.CeilToInt((float)maxHP / HP_PER_HEART); 

        GenerateHearts();
        UpdateHealthBar();
    }

    // Z/X 키를 이용한 테스트 함수 (이전 답변에서 제공)
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Z)) {
            if (HP > 0) {
                HP -= 1;
                UpdateHealthBar();
                Debug.Log("체력 감소! 현재 체력: " + HP);
            }
            else {
                Debug.Log("체력이 이미 0입니다.");
            }
        }
        if (Input.GetKeyDown(KeyCode.X)) {
            HP += 1;
            UpdateHealthBar();
            Debug.Log("체력 증가! 현재 체력: " + HP);
        }
    }


    private void GenerateHearts() {
        // 1. 기존 하트 모두 제거
        foreach (Transform child in heartsContainer) {
            Destroy(child.gameObject);
        }
        emptyHeartObjects.Clear(); // 새로운 리스트도 초기화

        // 2. maxHeart 개수만큼 하트 생성 (빈 하트)
        for (int i = 0; i < maxHeart; i++) {
            GameObject newHeart = Instantiate(emptyHeartPrefab, heartsContainer);

            // 4. X 좌표를 125씩 증가시키는 로직 추가
            RectTransform heartRect = newHeart.GetComponent<RectTransform>();
            if (heartRect != null) {
                // 기존 위치에서 오른쪽으로 125 * i 만큼 이동합니다.
                heartRect.anchoredPosition = new Vector2(i * 125f, heartRect.anchoredPosition.y);
            }

            // EmptyHeart 오브젝트 리스트에 추가
            emptyHeartObjects.Add(newHeart);

        }

        // 3. 함수 이름 변경 및 호출
        TurnOnHeartUnitComponents();
    }

    // 3. 함수 이름 변경: 모든 하트 유닛의 Image 컴포넌트를 활성화하는 함수
    public void TurnOnHeartUnitComponents() {

        // emptyHeartObjects 리스트를 순회합니다. (각 요소는 EmptyHeart GameObject입니다)
        foreach (GameObject emptyHeartObject in emptyHeartObjects) {
            if (emptyHeartObject != null) {

                // 1. emptyHeartObject의 이미지를 찾아서 키기 (A 이미지, 기존 로직 유지)
                // 부모 오브젝트(EmptyHeart, A 이미지)의 Image 컴포넌트를 활성화합니다.
                Image imageA = emptyHeartObject.GetComponent<Image>();
                if (imageA != null) {
                    imageA.enabled = true;
                }

                // --- 2단계, 3단계 로직 추가 ---

                // 2. emptyHeartObject의 자식 오브젝트 B를 찾을것
                // 자식 오브젝트는 FilledHeart이며, EmptyHeart의 첫 번째 자식이라고 가정합니다.
                Transform childTransformB = emptyHeartObject.transform.GetChild(0);

                if (childTransformB != null) {
                    // 3. B의 Image콤포넌트를 찾아서 킬것
                    Image imageB = childTransformB.GetComponent<Image>();

                    if (imageB != null) {
                        imageB.enabled = true;
                    }
                }
            }
        }
        Debug.Log("HealthBarManager: 모든 하트 유닛의 Image 컴포넌트가 활성화되었습니다.");
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
                    int hpToFill = Mathf.Min(remainingHP, HP_PER_HEART);

                    // FillAmount 계산 (0.0 ~ 1.0)
                    float fillAmount = (float)hpToFill / HP_PER_HEART;

                    // 3. Image 컴포넌트에 적용
                    fillImage.fillAmount = fillAmount;

                    // 남은 체력 업데이트 (다음 하트 계산을 위해)
                    remainingHP -= HP_PER_HEART;
                }
                else {
                    // 자식 Image를 찾지 못한 경우 (오류 방지)
                    remainingHP -= HP_PER_HEART;
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