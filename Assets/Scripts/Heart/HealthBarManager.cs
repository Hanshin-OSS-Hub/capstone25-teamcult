using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HealthBarManager : MonoBehaviour
{
    [SerializeField] private GameObject emptyHeartPrefab;
    [SerializeField] private GameObject filledHeartPrefab;

    [SerializeField] private Transform emptyHeartsContainer;
    [SerializeField] private Transform filledHeartsContainer;

    // === 체력 데이터 ===
    private int HP = 12;
    public int heart = 3; // 하트 개수
    private int maxHeart = 10;
    private int HPperHeart = 4;

    private List<GameObject> emptyHeartObjects = new List<GameObject>();
    private List<GameObject> filledHeartObjects = new List<GameObject>();


    private void Start()
    {
        GenerateHearts();
    }

    private void Update()
    {
        // (테스트용 단축키 코드는 그대로 두셔도 되고 지우셔도 됩니다)
       

        if (Input.GetKeyDown(KeyCode.C) && heart > 0) ChangeHeartType(HeartAttribute.Ice, heart - 1);
        if (Input.GetKeyDown(KeyCode.V) && heart > 0) ChangeHeartType(HeartAttribute.Fire, heart - 1);
    }

    public void ChangeHeartType(HeartAttribute type, int index = 0)
    {
        if (index < 0 || index >= filledHeartObjects.Count) return;

        filledHeart heartComponent = filledHeartObjects[index].GetComponent<filledHeart>();
        if (heartComponent != null) heartComponent.SetAttribute(type);
    }

    private void GenerateHearts()
    {
        foreach (Transform child in emptyHeartsContainer) Destroy(child.gameObject);
        foreach (Transform child in filledHeartsContainer) Destroy(child.gameObject);

        emptyHeartObjects.Clear();
        filledHeartObjects.Clear();

        // 기존 로직 유지: 현재 heart 개수만큼 생성
        int currentHeartCount = heart;
        heart = 0;
        HP = 0;
        AddHeart(currentHeartCount, true);
    }

    public int AddHeart(int cnt = 1, bool full = false)
    {
        int targetHeart = Mathf.Min(heart + cnt, maxHeart);
        for (; heart < targetHeart;)
        {
            GameObject emptyHeart = Instantiate(emptyHeartPrefab, emptyHeartsContainer);
            emptyHeartObjects.Insert(heart, emptyHeart);

            GameObject filledheart = Instantiate(filledHeartPrefab, filledHeartsContainer);
            filledHeartObjects.Insert(heart, filledheart);

            heart++;
            if (full) GainHP(HPperHeart);
        }
        return HP;
    }

    public int GainHP(int cnt)
    {
        for (int i = 0; i < heart; i++)
        {
            if (cnt == 0) break;
            filledHeart heartComponent = filledHeartObjects[i].GetComponent<filledHeart>();
            int space = HPperHeart - heartComponent.HP;
            int t = Mathf.Min(cnt, space);

            heartComponent.HP += t;
            UpdateHeart(filledHeartObjects[i]);
            cnt -= t;
            HP += t;
        }
        return HP;
    }

    public int LoseHP(int cnt)
    {
        for (int i = filledHeartObjects.Count - 1; i >= 0; i--)
        {
            if (cnt == 0) break;
            filledHeart heartComponent = filledHeartObjects[i].GetComponent<filledHeart>();
            int t = Mathf.Min(cnt, heartComponent.HP);

            heartComponent.HP -= t;
            UpdateHeart(filledHeartObjects[i]);
            cnt -= t;
            HP -= t;
        }
        return HP;
    }

    private void UpdateHeart(GameObject filledHeartObj)
    {
        filledHeart heartComponent = filledHeartObj.GetComponent<filledHeart>();
        Image fillHeart = filledHeartObj.GetComponent<Image>();
        if (fillHeart != null)
        {
            fillHeart.fillAmount = (float)heartComponent.HP / HPperHeart;
        }
    }

    private void UpdateHealthBar()
    {
        // (기존 코드에 있던 함수인데, 현재 로직에서는 LoseHP/GainHP에서 직접 처리하므로 안 쓰일 수도 있습니다. 
        //  혹시 몰라 유지합니다.)
        // ... (내용 생략) ...
    }

    public void ChangeHealth(int newHealth)
    {
        int diff = newHealth - HP;
        if (diff > 0) GainHP(diff);
        else if (diff < 0) LoseHP(-diff);
    }

    // ★★★ [우리가 추가한 핵심 기능] ★★★
    // 몇 번째 하트의 체력이 몇 남았는지 확인하는 함수
    public int GetHeartHP(int index)
    {
        // 안전장치: 없는 번호를 물어보면 0 리턴
        if (index < 0 || index >= filledHeartObjects.Count) return 0;

        // 해당 순서의 하트 스크립트를 가져와서 HP를 알려줌
        filledHeart heartComponent = filledHeartObjects[index].GetComponent<filledHeart>();
        if (heartComponent != null) return heartComponent.HP;

        return 0;
    }
}