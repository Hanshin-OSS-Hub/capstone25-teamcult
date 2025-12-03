using UnityEngine;

public class FlameHeartItem : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어 태그 확인 (Player 태그가 맞는지 꼭 확인하세요!)
        if (other.CompareTag("Player"))
        {
            // 씬에 있는 HeatController를 찾아서 켭니다.
            var controller = FindObjectOfType<HeatController>();
            if (controller != null)
            {
                controller.TriggerEffect();
            }
            else
            {
                Debug.Log("HeatController를 못 찾았어요! 카메라에 붙였는지 확인하세요.");
            }

            // 아이템 삭제
            Destroy(gameObject);
        }
    }
}