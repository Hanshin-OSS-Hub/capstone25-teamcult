using UnityEngine;

public class ElementalItem : MonoBehaviour
{
    [Header("Item Settings")]
    [Tooltip("Type exactly: Fire, Ice, or Poison")]
    public string elementType = "Fire";

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object colliding is the Player
        if (other.CompareTag("Player"))
        {
            // Find the ElementalManager in the scene
            ElementalManager manager = FindFirstObjectByType<ElementalManager>();

            if (manager != null)
            {
                // Activate the ability based on this item's element type
                manager.ActivateAbility(elementType);
            }

            // Destroy the item object after pickup
            Destroy(gameObject);
        }
    }
}