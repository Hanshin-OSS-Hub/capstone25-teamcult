using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    [Header("기믹 설정")]
    public int hitCountToBreak = 3;  // 부서지기까지 필요한 타격 횟수 (3대)
    private int currentHits = 0;     // 현재 맞은 횟수

    [Header("이펙트 효과")]
    public GameObject damageTextPrefab; // 데미지 텍스트 띄우기용

    // 무기에 맞았을 때 실행되는 함수
    public void TakeDamage(int damage)
    {
        // 들어온 데미지가 얼마든 상관없이, 맞은 횟수를 1 올립니다!
        currentHits++;

        // ?? 상자 위로 데미지 텍스트 띄우기
        if (damageTextPrefab != null)
        {
            Vector3 spawnPos = transform.position + new Vector3(0, 0.5f, 0);
            GameObject textObj = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);

            DamageText dmgTextScript = textObj.GetComponent<DamageText>();
            if (dmgTextScript != null)
            {
                // 화면에는 플레이어가 넣은 데미지 숫자를 그대로 보여줍니다.
                dmgTextScript.Setup(damage);
            }
        }

        // 맞은 횟수가 목표치(3)에 도달하면 파괴!
        if (currentHits >= hitCountToBreak)
        {
            Break();
        }
    }

    void Break()
    {
        // ? 골드 들어오는 코드는 삭제했습니다!
        Debug.Log("[기믹 파괴] 나무 상자가 3대 맞고 부서졌습니다!");

        // (나중에 여기에 상자 부서지는 파편 파티클이나 소리를 넣으시면 됩니다)

        // 맵에서 상자 삭제
        Destroy(gameObject);
    }
}