using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HealthBarManager : MonoBehaviour
{
    [SerializeField] private GameObject emptyHeartPrefab;
    [SerializeField] private GameObject filledHeartPrefab;

    [SerializeField] private Transform emptyHeartsContainer;
    [SerializeField] private Transform filledHeartsContainer;

    private int HP = 12;
    public int heart = 3; 
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

  

    public void ChangeHealth(int newHealth)
    {
        int diff = newHealth - HP;
        if (diff > 0) GainHP(diff);
        else if (diff < 0) LoseHP(-diff);
    }

  
    public int GetHeartHP(int index)
    {
        if (index < 0 || index >= filledHeartObjects.Count) return 0;

        filledHeart heartComponent = filledHeartObjects[index].GetComponent<filledHeart>();
        if (heartComponent != null) return heartComponent.HP;

        return 0;
    }
}