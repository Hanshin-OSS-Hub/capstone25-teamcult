using System.Collections.Generic;
using UnityEngine;

public class PlayerStateManager : MonoBehaviour
{
    public static PlayerStateManager Instance { get; private set; }

    [SerializeField] private PlayerStateData state = new PlayerStateData();
    [SerializeField] private bool hasPendingState = false;

    public PlayerStateData State => state;
    public bool HasPendingState => hasPendingState;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null) return;

        GameObject managerObject = new GameObject(nameof(PlayerStateManager));
        managerObject.AddComponent<PlayerStateManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (state == null)
        {
            state = new PlayerStateData();
        }
    }

    public void ResetState()
    {
        state.ResetToDefaults();
        hasPendingState = false;
    }

    public void MarkPendingState()
    {
        hasPendingState = true;
    }

    public void ClearPendingState()
    {
        state.ResetToDefaults();
        hasPendingState = false;
    }

    public void SetHealth(float currentHealth, float maxHealth)
    {
        state.maxHealth = Mathf.Max(1f, maxHealth);
        state.currentHealth = Mathf.Clamp(currentHealth, 0f, state.maxHealth);
    }

    public void SetGold(int gold)
    {
        state.gold = Mathf.Max(0, gold);
    }

    public void SetInventory(IEnumerable<Item> items)
    {
        state.inventoryItems.Clear();

        if (items == null) return;

        foreach (Item item in items)
        {
            if (item != null)
            {
                state.inventoryItems.Add(item);
            }
        }
    }

    public void AddInventoryItem(Item item)
    {
        if (item != null)
        {
            state.inventoryItems.Add(item);
        }
    }

    public void RemoveInventoryItem(Item item)
    {
        if (item != null)
        {
            state.inventoryItems.Remove(item);
        }
    }

    public void SetEquippedItem(Item.ItemType itemType, Item item)
    {
        state.SetEquippedItem(itemType, item);
    }

    public string GetDebugSummary()
    {
        return $"Health {state.currentHealth}/{state.maxHealth}, Gold {state.gold}, Inventory {state.inventoryItems.Count}, " +
            $"Helmet {GetItemName(state.equippedHelmet)}, Weapon {GetItemName(state.equippedWeapon)}, " +
            $"Upper {GetItemName(state.equippedUpper)}, Bottom {GetItemName(state.equippedBottom)}";
    }

    private string GetItemName(Item item)
    {
        return item != null ? item.itemName : "None";
    }
}
