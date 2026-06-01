using UnityEngine;

public class ChestController : MonoBehaviour
{
    [Header("기본 설정")]
    public KeyCode interactKey = KeyCode.F; // 상호작용 키 (기본 F)
    public GameObject[] lootPrefabs;        // 상자에서 생성될 수 있는 아이템 프리팹

    [Header("UI 설정")]
    public GameObject interactionUI;        // "F키로 상자 열기" Canvas 또는 Text 오브젝트

    private bool isPlayerNearby = false;
    private bool isOpen = false;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

        // 시작 시 상호작용 안내 UI가 있으면 비활성화합니다.
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
    }

    void Update()
    {
        // 플레이어가 근처에 있고, 상자가 닫혀 있고, 상호작용 키를 누르면 열기
        if (isPlayerNearby && !isOpen && Input.GetKeyDown(interactKey))
        {
            OpenChest();
        }
    }

    private void OpenChest()
    {
        isOpen = true; // 다시 열리지 않도록 상태를 고정

        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.PlaySFX(SFXType.ChestOpen);
        }

        // 1. 애니메이션 재생
        if (animator != null)
        {
            animator.SetTrigger("Open");
        }

        // 2. 상자가 열렸으므로 안내 UI 비활성화
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }

        // 3. 아이템 생성
        SpawnLoot();
    }

    private void SpawnLoot() {
        // 리스트가 비어있는지 확인
        if (lootPrefabs == null || lootPrefabs.Length == 0) return;

        // 1. 랜덤 인덱스 선택 (0부터 lootPrefabs.Length - 1까지)
        int randomIndex = UnityEngine.Random.Range(0, lootPrefabs.Length);
        GameObject selectedLoot = lootPrefabs[randomIndex];

        // 2. 생성 위치 계산 (상자 위치에서 y축으로 1.5만큼 아래)
        Vector3 spawnPosition = transform.position + new Vector3(0, -1.5f, 0);

        // 3. 선택된 아이템 프리팹 생성
        Instantiate(selectedLoot, spawnPosition, Quaternion.identity);
    }

    // 플레이어가 트리거(큰 원형 범위)에 들어왔을 때
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 충돌 오브젝트가 플레이어이고, 상자가 열리지 않은 상태라면
        if (other.CompareTag("Player") && !isOpen)
        {
            isPlayerNearby = true;

            // UI 켜기
            if (interactionUI != null)
            {
                interactionUI.SetActive(true);
            }
        }
    }

    // 플레이어가 트리거(큰 원형 범위)에서 나갔을 때
    private void OnTriggerExit2D(Collider2D other)
    {
        // 충돌 오브젝트가 플레이어일 때
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;

            // UI 끄기
            if (interactionUI != null)
            {
                interactionUI.SetActive(false);
            }
        }
    }
}