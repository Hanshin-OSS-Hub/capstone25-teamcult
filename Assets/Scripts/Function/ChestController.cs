using UnityEngine;

public class ChestController : MonoBehaviour
{
    [Header("�⺻ ����")]
    public KeyCode interactKey = KeyCode.F; // ��ȣ�ۿ� Ű (�⺻ F)
    public GameObject[] lootPrefabs;        // ���ڸ� ������ �� ���� �����۵�

    [Header("UI ����")]
    public GameObject interactionUI;        // "FŰ�� ���� ����" Canvas �Ǵ� Text ������Ʈ

    private bool isPlayerNearby = false;
    private bool isOpen = false;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

        // ���� ���� �� UI�� ���� �ִٸ� Ȯ���ϰ� ���ݴϴ�.
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
    }

    void Update()
    {
        // �÷��̾ ��ó�� �ְ�, ���ڰ� ���� �ְ�, FŰ�� ������ ��
        if (isPlayerNearby && !isOpen && Input.GetKeyDown(interactKey))
        {
            OpenChest();
        }
    }

    private void OpenChest()
    {
        isOpen = true; // �ٽ� ���� ���ϰ� ���� ����

        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.PlaySFX(SFXType.ChestOpen);
        }

        // 1. �ִϸ��̼� ����
        if (animator != null)
        {
            animator.SetTrigger("Open");
        }

        // 2. ���ڰ� �������� �ȳ� UI �����
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }

        // 3. ������ ����
        SpawnLoot();
    }

    private void SpawnLoot() {
        // ����Ʈ�� ����ִ��� Ȯ��
        if (lootPrefabs == null || lootPrefabs.Length == 0) return;

        // 1. ���� �ε��� ���� (0���� lootPrefabs.Length - 1����)
        int randomIndex = UnityEngine.Random.Range(0, lootPrefabs.Length);
        GameObject selectedLoot = lootPrefabs[randomIndex];

        // 2. ���� ��ġ ���� (���� ��ġ���� y������ 1.5��ŭ �Ʒ�)
        Vector3 spawnPosition = transform.position + new Vector3(0, -1.5f, 0);

        // 3. ���õ� �ϳ��� �����۸� ����
        Instantiate(selectedLoot, spawnPosition, Quaternion.identity);
    }

    // �÷��̾ Ʈ����(ū ���� ����)�� ������ ��
    private void OnTriggerEnter2D(Collider2D other)
    {
        // ���� ������Ʈ�� �÷��̾��̰�, ���ڰ� ���� �� ���ȴٸ�
        if (other.CompareTag("Player") && !isOpen)
        {
            isPlayerNearby = true;

            // UI �ѱ�
            if (interactionUI != null)
            {
                interactionUI.SetActive(true);
            }
        }
    }

    // �÷��̾ Ʈ����(ū ���� ����)���� ������ ��
    private void OnTriggerExit2D(Collider2D other)
    {
        // ���� ������Ʈ�� �÷��̾���
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;

            // UI ����
            if (interactionUI != null)
            {
                interactionUI.SetActive(false);
            }
        }
    }
}