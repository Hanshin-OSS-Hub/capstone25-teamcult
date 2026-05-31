using UnityEngine;

public class GoldPickup : MonoBehaviour
{
    [Header("이 오브젝트를 획득했을 때 추가될 골드 양")]
    public int goldAmount = 100;

    // 충돌체(플레이어)와 접촉하면 골드를 추가하고 오브젝트를 제거합니다.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 충돌한 오브젝트의 태그가 "Player"인지 확인합니다.
        if (collision.CompareTag("Player"))
        {
            // 플레이어 스탯을 찾아 골드를 추가합니다.
            if (PlayerStats.instance != null)
            {
                PlayerStats.instance.AddGold(goldAmount);
            }

            if (SFXManager.Instance != null)
            {
                SFXManager.Instance.PlaySFX(SFXType.Gold);
            }

            // 골드를 획득했으므로 화면에서 오브젝트를 제거합니다.
            Destroy(gameObject);
        }
    }
}