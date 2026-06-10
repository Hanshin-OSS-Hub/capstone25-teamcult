using UnityEngine;

public class ChestController : MonoBehaviour
{
    [Header("기본 설정")]
    public KeyCode interactKey = KeyCode.F; 
    public GameObject[] lootPrefabs;        

    [Header("UI 설정")]
    public GameObject interactionUI;        

    private bool isPlayerNearby = false;
    private bool isOpen = false;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

       if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
    }

    void Update()
    {
        if (isPlayerNearby && !isOpen && Input.GetKeyDown(interactKey))
        {
            OpenChest();
        }
    }

    private void OpenChest()
    {
        isOpen = true;

        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.PlaySFX(SFXType.ChestOpen);
        }

        if (animator != null)
        {
            animator.SetTrigger("Open");
        }

        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }

        SpawnLoot();
    }

    private void SpawnLoot() {
        if (lootPrefabs == null || lootPrefabs.Length == 0) return;

        int randomIndex = UnityEngine.Random.Range(0, lootPrefabs.Length);
        GameObject selectedLoot = lootPrefabs[randomIndex];

        Vector3 spawnPosition = transform.position + new Vector3(0, -1.5f, 0);

        Instantiate(selectedLoot, spawnPosition, Quaternion.identity);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isOpen)
        {
            isPlayerNearby = true;

            if (interactionUI != null)
            {
                interactionUI.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;

            if (interactionUI != null)
            {
                interactionUI.SetActive(false);
            }
        }
    }
}