using UnityEngine;

public class FlameHeartItem : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // [����] HeatController�� �ƴ϶� PlayerHealth�� ã�ƾ� �մϴ�!
            var playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                // �÷��̾����� "�Ҳ� ��Ʈ �Ծ���!"��� �˸�
                playerHealth.GetFlameHeart();
            }

            if (MusicDirector.Instance != null)
            {
                MusicDirector.Instance.SetFlameMode(true);
            }

            Destroy(gameObject);
        }
    }
}