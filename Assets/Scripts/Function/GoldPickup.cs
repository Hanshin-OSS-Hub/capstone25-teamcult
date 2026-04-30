using UnityEngine;

public class GoldPickup : MonoBehaviour
{
    [Header("�� ������ ������ ������ ��� ��")]
    public int goldAmount = 100;

    // ������(�÷��̾�)�� ������ ����� �� ����˴ϴ�.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ���� ������Ʈ�� �±װ� "Player"���� Ȯ���մϴ�.
        if (collision.CompareTag("Player"))
        {
            // �÷��̾� ������ ã�� ��带 �÷��ݴϴ�.
            if (PlayerStats.instance != null)
            {
                PlayerStats.instance.AddGold(goldAmount);
            }

            if (SFXManager.Instance != null)
            {
                SFXManager.Instance.PlaySFX(SFXType.Gold);
            }

            // ��带 �Ծ����� ȭ�鿡�� ������ ���۴ϴ�.
            Destroy(gameObject);
        }
    }
}