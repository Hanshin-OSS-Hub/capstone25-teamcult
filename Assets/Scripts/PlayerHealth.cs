using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    // [예시 변수] 체력 관련 변수가 이미 있다면 본인 것으로 유지하세요.
    public int maxHealth = 3;
    public int currentHealth;

    void Start()
    {
        // 게임 시작 시 체력 초기화
        currentHealth = maxHealth;
    }

    // -------------------------------------------------------
    // ▼▼▼ 오류 해결을 위해 추가된 함수 ▼▼▼
    // -------------------------------------------------------
    public void GetFlameHeart()
    {
        Debug.Log("불꽃 심장을 획득했습니다! (GetFlameHeart 호출됨)");

        // 원하는 기능을 여기에 작성하세요. 
        // 예: 체력을 회복하거나 불꽃 모드 활성화 등

        // 예시: 체력을 최대치로 회복
        currentHealth = maxHealth;
    }
    // -------------------------------------------------------

    // (기존에 있던 데미지 처리 함수 등은 아래에 유지...)
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Debug.Log("플레이어 사망");
        }
    }
}