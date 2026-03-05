using UnityEngine;

public class GoldPickup : MonoBehaviour
{
    [Header("이 동전을 먹으면 오르는 골드 양")]
    public int goldAmount = 100;

    // 누군가(플레이어)가 동전에 닿았을 때 실행됩니다.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 닿은 오브젝트의 태그가 "Player"인지 확인합니다.
        if (collision.CompareTag("Player"))
        {
            // 플레이어 스탯을 찾아 골드를 올려줍니다.
            if (PlayerStats.instance != null)
            {
                PlayerStats.instance.AddGold(goldAmount);
            }

            // 골드를 먹었으니 화면에서 동전을 없앱니다.
            Destroy(gameObject);
        }
    }
}