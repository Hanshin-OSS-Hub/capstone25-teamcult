using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    [Header("기믹 설정")]
    public int hitCountToBreak = 3;  
    private int currentHits = 0;     

    [Header("이펙트 효과")]
    public GameObject damageTextPrefab; 

    public void TakeDamage(int damage)
    {
        currentHits++;

        if (damageTextPrefab != null)
        {
            Vector3 spawnPos = transform.position + new Vector3(0, 0.5f, 0);
            GameObject textObj = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);

            DamageText dmgTextScript = textObj.GetComponent<DamageText>();
            if (dmgTextScript != null)
            {
                dmgTextScript.Setup(damage);
            }
        }

        if (currentHits >= hitCountToBreak)
        {
            Break();
        }
    }

    void Break()
    {
        Debug.Log("[기믹 파괴] 나무 상자가 3대 맞고 부서졌습니다!");


        Destroy(gameObject);
    }
}