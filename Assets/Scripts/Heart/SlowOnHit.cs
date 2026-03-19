using UnityEngine;

public class SlowOnHit : MonoBehaviour
{
    public ElementalManager elementalManager;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;
        if (elementalManager == null || !elementalManager.hasIceHeart) return;

        if (other.GetComponent<SlowEffect>() == null)
        {
            Debug.Log("[얼음 하트] 슬로우 부여!");
            other.gameObject.AddComponent<SlowEffect>();
        }
    }
}