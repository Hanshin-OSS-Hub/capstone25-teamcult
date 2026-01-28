using System.Collections.Generic;
using UnityEngine;

public class MoveMapBounds : MonoBehaviour {
    [SerializeField] private Transform playerTarget;
    [SerializeField] private int gridSize = 20;
    [SerializeField] private Vector2Int currentRoomIndex = new Vector2Int(0, 0);

    private RoomManager roomManager; 
    private BoxCollider2D areaCollider; // 방 내부 콜라이더
    private GameObject wallObject; // 벽
    private List<GameObject> currentRoomEnemies = new List<GameObject>(); // 현재 방에서 소환된 적들을 추적하는 리스트
    private EnemySpawner enemySpawner;

    void Start() {
        // transform.position 초기화는 실제 시작 방의 월드 좌표와 맞춰야 함
        // transform.position = new Vector3(10, -10, transform.position.z);
        areaCollider = transform.Find("Area")?.GetComponent<BoxCollider2D>();
        wallObject = transform.Find("Wall")?.gameObject;
        roomManager = Object.FindAnyObjectByType<RoomManager>();
        enemySpawner = Object.FindAnyObjectByType<EnemySpawner>();

        if (roomManager == null) { Debug.LogError("RoomManager를 찾을 수 없습니다!"); }
        if (enemySpawner == null) { Debug.LogError("EnemySpawner를 찾을 수 없습니다!"); }

        // 초기 위치 설정 (RoomManager의 중앙 인덱스와 일치시킴)
        // RoomManager의 mapSize가 11이라면 중앙은 5
        int startIdx = roomManager.MapSize / 2;
        currentRoomIndex = new Vector2Int(startIdx, startIdx);
    }

    void Update() {
        if (playerTarget == null || roomManager == null) return;

        UpdateCameraPosition();
        HandleWallLock();
        CheckEnemiesStatus();

        if (Input.GetKeyDown(KeyCode.Z)) {
            ReduceMonsterCountTest();
        }
    }

    // [테스트 기능] 몬스터 수를 줄여서 문이 열리는지 확인
    private void ReduceMonsterCountTest() {
        var room = roomManager.rooms[currentRoomIndex.x, currentRoomIndex.y];
        if (room.status == RoomData.RoomStatus.Locked) {
            room.monsterCount--;
            Debug.Log($"몬스터 처치! 남은 수: {room.monsterCount}");
        }
    }

    private void UpdateCameraPosition() {
        Vector3 currentPos = transform.position;
        Vector3 playerPos = playerTarget.position;
        float halfSize = gridSize / 2f;
        bool hasMoved = false;

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
            PrintRoomInfo();
        }
    }

    private bool IsPlayerCompletelyInside() {
        if (areaCollider == null) return false;
        Bounds areaBounds = areaCollider.bounds;
        Collider2D pCol = playerTarget.GetComponent<Collider2D>();

        if (pCol != null) {
            return areaBounds.Contains(pCol.bounds.min) && areaBounds.Contains(pCol.bounds.max);
        }
        return areaBounds.Contains(playerTarget.position);
    }

private void HandleWallLock() {
        if (wallObject == null || roomManager.rooms == null) return;

        int x = currentRoomIndex.x;
        int y = currentRoomIndex.y;

        if (x < 0 || x >= roomManager.rooms.GetLength(0) || y < 0 || y >= roomManager.rooms.GetLength(1)) return;

        RoomData currentRoom = roomManager.rooms[x, y];
        bool isInside = IsPlayerCompletelyInside();

        // 1. 입장 시 잠금 처리
        if (currentRoom.status == RoomData.RoomStatus.Empty && currentRoom.isFirstVisit) {
            if (isInside && currentRoom.shouldLockOnVisit) {
                currentRoom.status = RoomData.RoomStatus.Locked;
                currentRoom.isFirstVisit = false;
                SpawnMonsters(currentRoom);
            }
        }

        // 2. 잠긴 상태에서 클리어 체크
        if (currentRoom.status == RoomData.RoomStatus.Locked) {
            if (!wallObject.activeSelf) wallObject.SetActive(true);

            // 미사용 조건 변경: 데이터상의 숫자와 실제 리스트가 모두 비었을 때 클리어
            //if (currentRoom.CheckClearCondition() && currentRoomEnemies.Count == 0) {
            if (currentRoomEnemies.Count == 0) {
                UnlockAndReward(currentRoom);
            }
        }
        else {
            if (wallObject.activeSelf) wallObject.SetActive(false);
        }
    }

    private void SpawnMonsters(RoomData room) {
        if (room.monsterCount <= 0) return;
        Debug.Log($"<color=red>전투 시작!</color> {room.monsterCount}마리 소환 시도");

        if (enemySpawner != null) {
            // EnemySpawner로부터 생성된 적 리스트를 전달받음
            currentRoomEnemies = enemySpawner.SpawnEnemy(room.monsterCount);
        }
    }

    // 적이 죽었는지(Destroy 되었는지) 확인하는 로직
    // 리스트의 마지막 원소만 확인하는 방식, 모든 적이 죽었는지 확인하는 용도로만 사용할것
    // 적이 정확히 몇마리인지 알고싶다면 모든 원소를 순회할것
    private void CheckEnemiesStatus() {
        while (currentRoomEnemies.Count > 0) {
            int lastIndex = currentRoomEnemies.Count - 1;
            // 마지막 요소가 Destroy 되었는지 확인
            if (currentRoomEnemies[lastIndex] == null) {
                currentRoomEnemies.RemoveAt(lastIndex); // 죽었으면 제거
            }
            else { // 적이 하나 이상 남아있음
                break;
            }
        }
    }

    private void UnlockAndReward(RoomData room) {
        room.status = RoomData.RoomStatus.Cleared;
        Debug.Log($"<color=cyan>방 클리어!</color> 보상: {room.rewardItemName}");
        // TODO: 보상 상자 생성 로직
    }

    private void PrintRoomInfo() {
        if (roomManager.rooms == null) return;
        var room = roomManager.rooms[currentRoomIndex.x, currentRoomIndex.y];
        Debug.Log($"방 이동: [{currentRoomIndex.x}, {currentRoomIndex.y}] 상태: {room.status}, 남은몹: {room.monsterCount}");
    }
}