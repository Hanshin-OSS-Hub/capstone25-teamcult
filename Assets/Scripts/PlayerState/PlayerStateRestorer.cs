using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerStateRestorer : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        GameObject restorerObject = new GameObject(nameof(PlayerStateRestorer));
        DontDestroyOnLoad(restorerObject);
        restorerObject.AddComponent<PlayerStateRestorer>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(RestoreAfterSceneStart(scene.name));
    }

    private IEnumerator RestoreAfterSceneStart(string sceneName)
    {
        yield return null;

        if (PlayerStateManager.Instance == null || !PlayerStateManager.Instance.HasPendingState)
        {
            yield break;
        }

        PlayerStateData state = PlayerStateManager.Instance.State;
        Debug.Log($"[PlayerState] Restoring after scene load ({sceneName}): {PlayerStateManager.Instance.GetDebugSummary()}");

        RestoreHealth(state);
        RestoreGold(state);
        RestoreInventoryAndEquipment(state);

        Debug.Log($"[PlayerState] Restore completed ({sceneName}). Clearing pending transition state.");
        PlayerStateManager.Instance.ClearPendingState();
    }

    private void RestoreHealth(PlayerStateData state)
    {
        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth == null) return;

        playerHealth.maxHealth = state.maxHealth;
        playerHealth.currentHealth = state.currentHealth;
        playerHealth.UpdateUI();
    }

    private void RestoreGold(PlayerStateData state)
    {
        PlayerStats playerStats = FindFirstObjectByType<PlayerStats>();
        if (playerStats == null) return;

        playerStats.SetGold(state.gold);
    }

    private void RestoreInventoryAndEquipment(PlayerStateData state)
    {
        if (TabController.instance == null) return;

        TabController.instance.RestoreFromState(state);
        TabController.instance.UpdateGoldUI(state.gold);
    }
}
