using UnityEngine;

public class MoveMapBounds : MonoBehaviour {
    [SerializeField] private Transform playerTarget;
    [SerializeField] private int gridSize = 20;
    [SerializeField] private Vector2Int currentRoomIndex = new Vector2Int(0, 0);

    private RoomManager roomManager;
    private BoxCollider2D areaCollider;
    private GameObject wallObject; // 자식 "Wall" 오브젝트 참조

    void Start() {
        // 1. 위치 초기화 (기본 인덱스 설정 - mapSize 11 기준 중앙값 5)
        int startIdx = 5;
        currentRoomIndex = new Vector2Int(startIdx, startIdx);
        transform.position = new Vector3(10f, -10f, transform.position.z);

        // 2. 컴포넌트 및 자식 오브젝트 찾기
        areaCollider = transform.Find("Area")?.GetComponent<BoxCollider2D>();
        wallObject = transform.Find("Wall")?.gameObject; // "Wall" 오브젝트 찾기
        roomManager = Object.FindAnyObjectByType<RoomManager>();

        if (roomManager == null) Debug.LogError("RoomManager를 찾을 수 없습니다!");
        if (wallObject == null) Debug.LogWarning("자식 오브젝트 'Wall'을 찾을 수 없습니다.");

        PrintMapLockInfo();
    }

    void Update() {
        if (playerTarget == null || roomManager == null) return;

        UpdateCameraPosition();
        HandleWallLock();

        // --- 테스트용 코드: Z키를 누르면 현재 방 해제 ---
        if (Input.GetKeyDown(KeyCode.Z)) {
            UnlockCurrentRoom();
        }
    }

    private void UnlockCurrentRoom() {
        if (roomManager.mapLock == null) return;

        int x = currentRoomIndex.x;
        int y = currentRoomIndex.y;

        // 인덱스 범위 확인 후 값 변경
        if (x >= 0 && x < roomManager.mapLock.GetLength(0) &&
            y >= 0 && y < roomManager.mapLock.GetLength(1)) {

            roomManager.mapLock[x, y] = 0; // 잠금 해제!
            Debug.Log($"<color=yellow>테스트:</color> [{x}, {y}] 번 방의 잠금을 해제했습니다.");

            // HandleWallLock이 Update에서 돌아가고 있으므로, 
            // 값이 0이 되는 순간 다음 프레임에 Wall이 즉시 비활성화됩니다.
        }
    }

    private void UpdateCameraPosition() {
        Vector3 currentPos = transform.position;
        Vector3 playerPos = playerTarget.position;
        float halfSize = gridSize / 2f;
        bool hasMoved = false;

        // X축 이동
        if (playerPos.x > currentPos.x + halfSize) {
            transform.position += new Vector3(gridSize, 0, 0);
            currentRoomIndex.x += 1;
            hasMoved = true;
        }
        else if (playerPos.x < currentPos.x - halfSize) {
            transform.position -= new Vector3(gridSize, 0, 0);
            currentRoomIndex.x -= 1;
            hasMoved = true;
        }

        // Y축 이동 (RoomManager의 배치 방식에 맞춰 + 인덱스 증가로 수정)
        if (playerPos.y > currentPos.y + halfSize) {
            transform.position += new Vector3(0, gridSize, 0);
            currentRoomIndex.y += 1;
            hasMoved = true;
        }
        else if (playerPos.y < currentPos.y - halfSize) {
            transform.position -= new Vector3(0, gridSize, 0);
            currentRoomIndex.y -= 1;
            hasMoved = true;
        }

        if (hasMoved) {
            PrintMapLockInfo();
        }
    }

    // 1. 플레이어가 영역 안에 완전히 포함되었는지 체크 (bool 반환)
    private bool IsPlayerCompletelyInside() {
        if (areaCollider == null) return false;

        Bounds areaBounds = areaCollider.bounds;
        Collider2D pCol = playerTarget.GetComponent<Collider2D>();

        if (pCol != null) {
            // 플레이어 콜라이더의 모든 끝점이 영역 안에 있어야 함
            return areaBounds.Contains(pCol.bounds.min) && areaBounds.Contains(pCol.bounds.max);
        }

        // 콜라이더가 없을 경우 피벗 포인트만 체크
        return areaBounds.Contains(playerTarget.position);
    }

    // 2. mapLock 상태와 위치를 체크하여 Wall 활성화/비활성화
    private void HandleWallLock() {
        if (wallObject == null || roomManager.mapLock == null) return;

        int x = currentRoomIndex.x;
        int y = currentRoomIndex.y;

        // 인덱스 유효성 검사
        if (x >= 0 && x < roomManager.mapLock.GetLength(0) &&
            y >= 0 && y < roomManager.mapLock.GetLength(1)) {

            int lockStatus = roomManager.mapLock[x, y];
            bool isInside = IsPlayerCompletelyInside();

            // 조건: mapLock이 1(잠김)이고 플레이어가 완전히 들어왔을 때만 Wall 활성화
            if (lockStatus == 1 && isInside) {
                if (!wallObject.activeSelf) wallObject.SetActive(true);
            }
            // mapLock이 0(해제)이거나 플레이어가 아직 들어오는 중이면 Wall 비활성화
            else if (lockStatus == 0) {
                if (wallObject.activeSelf) wallObject.SetActive(false);
            }
        }
    }

    private void PrintMapLockInfo() {
        if (roomManager?.mapLock == null) return;

        int x = currentRoomIndex.x;
        int y = currentRoomIndex.y;

        if (x >= 0 && x < roomManager.mapLock.GetLength(0) &&
            y >= 0 && y < roomManager.mapLock.GetLength(1)) {
            Debug.Log($"방 인덱스: [{x}, {y}], mapLock 값: {roomManager.mapLock[x, y]}");
        }
    }
}