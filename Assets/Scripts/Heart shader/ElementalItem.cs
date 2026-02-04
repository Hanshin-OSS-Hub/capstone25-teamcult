using UnityEngine;

public class ElementalItem : MonoBehaviour
{
    // 드롭다운 메뉴로 만들기 (오타 방지)
    public enum ElementType
    {
        Fire,
        Ice,
        Poison
    }

    [Header("속성 설정")]
    public ElementType elementType; // 여기서 불/얼음 고르세요!

    [Header("효과음 (선택)")]
    public AudioClip pickupSound;

    void OnTriggerEnter2D(Collider2D other)
    {
        // 1. 플레이어랑 닿았는지 확인
        // (주의: 플레이어 오브젝트의 Tag가 "Player"여야 합니다!)
        if (other.CompareTag("Player"))
        {
            // 2. 매니저를 찾아서 명령 내리기
            HeartAbilityManager manager = FindFirstObjectByType<HeartAbilityManager>();

            if (manager != null)
            {
                // Enum을 문자열("Fire", "Ice")로 바꿔서 전달함
                manager.ActivateAbility(elementType.ToString());
            }
            else
            {
                Debug.LogError(" HeartAbilityManager가 씬에 없습니다!");
            }

            // 3. 소리 재생 (있으면)
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            // 4. 아이템 삭제 (냠냠)
            Destroy(gameObject);
        }
    }
}