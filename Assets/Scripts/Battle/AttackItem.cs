using UnityEngine;

public class AttackItem : MonoBehaviour
{
    [Header("아이템 능력치 설정")]
    [Tooltip("기본 공격력을 얼마나 올려줄까요? (예: 1, 5)")]
    public float damageAmount = 0f;

    [Tooltip("배율을 얼마나 올려줄까요? (예: 0.1 = 10%, 0.5 = 50%)")]
    public float multiplierAmount = 0f;

    void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어와 닿았을 때
        if (other.CompareTag("Player"))
        {
            // 플레이어의 공격 스크립트 가져오기
            PlayerAttack playerAttack = other.GetComponent<PlayerAttack>();

            if (playerAttack != null)
            {
                // 1. 깡공격력이 설정되어 있으면 추가
                if (damageAmount > 0)
                {
                    playerAttack.AddDamage(damageAmount);
                }

                // 2. 배율이 설정되어 있으면 추가
                if (multiplierAmount > 0)
                {
                    playerAttack.AddMultiplier(multiplierAmount);
                }

                // 3. 아이템 먹었으니 삭제
                Destroy(gameObject);
            }
        }
    }
}