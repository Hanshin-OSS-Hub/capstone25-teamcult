using UnityEngine;

public class BurnOnHit : MonoBehaviour
{
    public ElementalManager elementalManager;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;
        if (elementalManager == null || !elementalManager.hasFireHeart) return;

        // 중복 방지
        if (other.GetComponent<BurnEffect>() == null)
        {
            Debug.Log("[불 하트] 화상 부여!");
            other.gameObject.AddComponent<BurnEffect>();
        }
    }
}