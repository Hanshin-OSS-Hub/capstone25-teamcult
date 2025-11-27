using UnityEngine;
using UnityEngine.Rendering; // 볼륨 제어용

public class FireEffectController : MonoBehaviour
{
    [Header("연결할 것들")]
    public GameObject particleObject; // 파티클
    public Volume fireVolume;         // 볼륨
    [SerializeField] private PlayerHealth playerHealth; // 체력 관련

    // 2D 게임에서는 반드시 'OnTriggerEnter2D'를 써야 합니다!
    // (매개변수도 Collider가 아니라 Collider2D여야 함)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("FlameHeartItem"))
        {
            ActivateFireMode();
            playerHealth.GetFlameHeart();
            Destroy(other.gameObject); // 아이템 삭제
        }
    }

    void ActivateFireMode()
    {
        // 1. 파티클 켜기
        if (particleObject != null)
        {
            particleObject.SetActive(true);
        }

        // 2. 볼륨 켜기 (Weight 1로)
        if (fireVolume != null)
        {
            fireVolume.weight = 1f;
        }
    }
}