using UnityEngine;

public class FlameHeartItem : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // [수정] HeatController가 아니라 PlayerHealth를 찾아야 합니다!
            var playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                // 플레이어한테 "불꽃 하트 먹었어!"라고 알림
                playerHealth.GetFlameHeart();
            }

            Destroy(gameObject);
        }
    }
}