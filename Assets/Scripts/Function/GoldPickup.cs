using UnityEngine;

public class GoldPickup : MonoBehaviour
{
    [Header("이 오브젝트를 획득했을 때 추가될 골드 양")]
    public int goldAmount = 100;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (PlayerStats.instance != null)
            {
                PlayerStats.instance.AddGold(goldAmount);
            }

            if (SFXManager.Instance != null)
            {
                SFXManager.Instance.PlaySFX(SFXType.Gold);
            }

            Destroy(gameObject);
        }
    }
}