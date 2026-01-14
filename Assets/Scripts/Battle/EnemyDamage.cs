using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    public int damage = 1; // 플레이어에게 줄 데미지 (하트 1개)

    // 적이 무언가와 부딪혔을 때
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 부딪힌 게 "Player"라면?
        if (collision.gameObject.CompareTag("Player"))
        {
            // 플레이어의 체력 스크립트를 가져와서 데미지 주기
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log("플레이어가 적과 부딪혔습니다!");
            }
        }
    }
}