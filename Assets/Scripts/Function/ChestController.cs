using UnityEngine;

public class ChestController : MonoBehaviour
{
    [Header("기본 설정")]
    public KeyCode interactKey = KeyCode.F; // 상호작용 키 (기본 F)
    public GameObject[] lootPrefabs;        // 상자를 열었을 때 나올 아이템들

    [Header("UI 연결")]
    public GameObject interactionUI;        // "F키를 눌러 열기" Canvas 또는 Text 오브젝트

    private bool isPlayerNearby = false;
    private bool isOpen = false;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

        // 게임 시작 시 UI가 켜져 있다면 확실하게 꺼줍니다.
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
    }

    void Update()
    {
        // 플레이어가 근처에 있고, 상자가 닫혀 있고, F키를 눌렀을 때
        if (isPlayerNearby && !isOpen && Input.GetKeyDown(interactKey))
        {
            OpenChest();
        }
    }

    private void OpenChest()
    {
        isOpen = true; // 다시 열지 못하게 상태 변경

        // 1. 애니메이션 실행
        if (animator != null)
        {
            animator.SetTrigger("Open");
        }

        // 2. 상자가 열렸으니 안내 UI 숨기기
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }

        // 3. 아이템 생성
        SpawnLoot();
    }

    private void SpawnLoot()
    {
        if (lootPrefabs == null || lootPrefabs.Length == 0) return;

        foreach (GameObject loot in lootPrefabs)
        {
            // 상자 위치보다 살짝 위(y축 +0.5)로 스폰 위치 조정
            Vector3 spawnPosition = transform.position + new Vector3(0, 0.5f, 0);
            Instantiate(loot, spawnPosition, Quaternion.identity);
        }
    }

    // 플레이어가 트리거(큰 투명 영역)에 들어왔을 때
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 들어온 오브젝트가 플레이어이고, 상자가 아직 안 열렸다면
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

    // 플레이어가 트리거(큰 투명 영역)에서 나갔을 때
    private void OnTriggerExit2D(Collider2D other)
    {
        // 나간 오브젝트가 플레이어라면
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