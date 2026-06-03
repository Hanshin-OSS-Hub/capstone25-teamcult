using UnityEngine;
using UnityEngine.SceneManagement;

public class Elevator : MonoBehaviour
{
    [SerializeField] private string sceneName = "demo";

    public void SetSceneName(string nextSceneName)
    {
        if (string.IsNullOrWhiteSpace(nextSceneName)) return;
        sceneName = nextSceneName;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        Debug.Log("Player entered elevator. Loading next scene.");
        SavePlayerState(collision.gameObject);
        SceneManager.LoadScene(sceneName);
    }

    private void SavePlayerState(GameObject playerObject)
    {
        if (PlayerStateManager.Instance == null) return;

        PlayerHealth playerHealth = playerObject.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            PlayerStateManager.Instance.SetHealth(playerHealth.currentHealth, playerHealth.maxHealth);
        }

        PlayerStats playerStats = playerObject.GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            PlayerStateManager.Instance.SetGold(playerStats.currentGold);
        }

        if (TabController.instance != null)
        {
            PlayerStateManager.Instance.SetInventory(TabController.instance.inventoryItems);
            PlayerStateManager.Instance.SetEquippedItem(Item.ItemType.Helmet, TabController.instance.GetEquippedItem(Item.ItemType.Helmet));
            PlayerStateManager.Instance.SetEquippedItem(Item.ItemType.Weapon, TabController.instance.GetEquippedItem(Item.ItemType.Weapon));
            PlayerStateManager.Instance.SetEquippedItem(Item.ItemType.Upper, TabController.instance.GetEquippedItem(Item.ItemType.Upper));
            PlayerStateManager.Instance.SetEquippedItem(Item.ItemType.Bottom, TabController.instance.GetEquippedItem(Item.ItemType.Bottom));
        }

        PlayerStateManager.Instance.MarkPendingState();

        Debug.Log($"[PlayerState] Saved before scene load: {PlayerStateManager.Instance.GetDebugSummary()}");
    }
}
