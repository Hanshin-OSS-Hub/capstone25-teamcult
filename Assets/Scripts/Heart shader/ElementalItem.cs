using UnityEngine;

public class ElementalItem : MonoBehaviour
{
    [Header("Item Settings")]
    [Tooltip("Type exactly: Fire, Ice, or Poison")]
    public string elementType = "Fire";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ElementalManager manager = FindFirstObjectByType<ElementalManager>();

            if (manager != null)
            {
                manager.ActivateAbility(elementType);
            }

            // 검은 재 파티클
            if (elementType == "Fire")
            {
                GameObject pfxObj = new GameObject("AshPFX");
                pfxObj.transform.position = transform.position;
                HeartPickupParticle pfx = pfxObj.AddComponent<HeartPickupParticle>();
                pfx.Play(transform.position);
                Destroy(pfxObj, 3f);
            }

            Destroy(gameObject);
        }
    }
}