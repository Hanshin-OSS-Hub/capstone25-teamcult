using UnityEngine;
public class BurnOnHit : MonoBehaviour
{
    public ElementalManager elementalManager;
    public float burnDamage = 3f; // 추가!

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;
        if (elementalManager == null || !elementalManager.hasFireHeart) return;

        if (other.GetComponent<BurnEffect>() == null)
        {
            BurnEffect burn = other.gameObject.AddComponent<BurnEffect>();
            burn.damage = burnDamage; // 데미지 전달!
        }
    }
}