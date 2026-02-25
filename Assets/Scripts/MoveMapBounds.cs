using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MoveMapBounds : MonoBehaviour {
    [SerializeField] private Transform playerTarget;
    [SerializeField] private int gridSize = 20;
    [SerializeField] private Vector2Int currentRoomIndex = new Vector2Int(0, 0);
    public Vector2Int CurrentRoomIndex => currentRoomIndex;

    private RoomManager roomManager; 
    private BoxCollider2D areaCollider; // 방 내부 콜라이더
    private GameObject wallObject; // 벽
    private GameObject[] wallParts = new GameObject[4]; // 벽 상(0), 우(1), 하(2), 좌(3) 순서로 저장될 배열
    private int ActiveWalls = 0;
    private List<GameObject> currentRoomEnemies = new List<GameObject>(); // 현재 방에서 소환된 적들을 추적하는 리스트
    private EnemySpawner enemySpawner;

    private GameObject[] visualWalls = new GameObject[4]; // 애니메이션용 복제본
    private TilemapCollider2D[] realColliders = new TilemapCollider2D[4]; // 원본의 콜라이더

    [Header("Wall Animation Settings")]
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private float wallUpYOffset = 2.0f;
    private Coroutine[] wallCoroutines = new Coroutine[4];


    void Start() {
        // transform.position 초기화는 실제 시작 방의 월드 좌표와 맞춰야 함
        // transform.position = new Vector3(10, -10, transform.position.z);
        areaCollider = transform.Find("Area")?.GetComponent<BoxCollider2D>();
        wallObject = transform.Find("Wall")?.gameObject;

        if (wallObject != null) {
            int[] childNames = { 0, 1, 2, 3 };
            for (int i = 0; i < 4; i++) {
                Transform child = wallObject.transform.Find(childNames[i].ToString());
                if (child != null) {
                    wallParts[i] = child.gameObject;
                }
            }
        }
        SetupWallSeparation();
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

        // 인덱스 범위 체크
        if (x < 0 || x >= roomManager.MapSize || y < 0 || y >= roomManager.MapSize) return;

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

        // 2. 상태별 벽 제어
        if (currentRoom.status == RoomData.RoomStatus.Locked) {
            int doorMask = roomManager.GetDoorMask(x, y);
            SetWalls(doorMask);

            if (currentRoomEnemies.Count == 0) {
                UnlockAndReward(currentRoom);
            }
        }
        else {
            // 평소(Cleared 또는 Empty)에는 모든 벽을 내립니다.
            SetWalls(0);
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
        SetWalls(0); // 모든 벽 비활성화

        if (room.rewardPrefabs != null && room.rewardPrefabs.Count > 0) {
            foreach (GameObject prefab in room.rewardPrefabs) {
                if (prefab == null) continue;

                // 방 중앙(transform.position)에 생성
                // 여러 개일 경우를 대비해 랜덤한 오프셋을 줄 수도 있습니다.
                Vector3 spawnOffset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
                Instantiate(prefab, transform.position + spawnOffset, Quaternion.identity);

                Debug.Log($"<color=cyan>보상 생성됨:</color> {prefab.name}");
            }
        }
    }

    private void PrintRoomInfo() {
        if (roomManager.rooms == null) return;
        var room = roomManager.rooms[currentRoomIndex.x, currentRoomIndex.y];
        Debug.Log($"방 이동: [{currentRoomIndex.x}, {currentRoomIndex.y}] 상태: {room.status}, 남은몹: {room.monsterCount}");
    }


    //private int ActiveWalls = 0;
    /// <summary>
    /// 모든 벽의 상태를 매개변수로 받은 mask 값과 동일하게 설정합니다.
    /// </summary>
    /// <param name="mask">설정하고자 하는 벽 상태 (0~15)</param>
    private void SetupWallSeparation() {
        if (wallObject == null) return;

        for (int i = 0; i < 4; i++) {
            if (wallParts[i] == null) continue;

            // --- 1. 원본 설정 (물리 전용) ---
            realColliders[i] = wallParts[i].GetComponent<TilemapCollider2D>();
            TilemapRenderer tr = wallParts[i].GetComponent<TilemapRenderer>();
            if (tr != null) tr.enabled = false; // 물리 벽은 안 보이게 함

            if (realColliders[i] != null) {
                realColliders[i].enabled = false;
            }

            // --- 2. 시각적 복제본 생성 ---
            // Instantiate 할 때 부모만 지정하지 말고, 위치/회전값도 원본과 맞춥니다.
            visualWalls[i] = Instantiate(wallParts[i], wallParts[i].transform.position, wallParts[i].transform.rotation, wallParts[i].transform.parent);
            visualWalls[i].name = wallParts[i].name + "_Visual";

            // 복제본의 콜라이더 제거
            TilemapCollider2D duplicateCol = visualWalls[i].GetComponent<TilemapCollider2D>();
            if (duplicateCol != null) Destroy(duplicateCol);

            // 복제본의 렌더러는 켜기 (원본이 꺼져있으므로 명시적으로 켬)
            TilemapRenderer visualRenderer = visualWalls[i].GetComponent<TilemapRenderer>();
            if (visualRenderer != null) visualRenderer.enabled = true;

            Tilemap visualTilemap = visualWalls[i].GetComponent<Tilemap>();
            if (visualTilemap != null) {
                Color vc = visualTilemap.color;
                vc.a = 0f;
                visualTilemap.color = vc;
            }

            // 초기 위치 설정: 원본의 '상대적' 위치에서 Y축 오프셋만 줌
            // Vector3.zero가 아니라 원본의 localPosition이 기준이 되어야 합니다.
            Vector3 originLocalPos = wallParts[i].transform.localPosition;
            visualWalls[i].transform.localPosition = originLocalPos + new Vector3(0, wallUpYOffset, 0);
            visualWalls[i].SetActive(false);
        }
    }

    public void SetWalls(int mask) {
        if (wallObject == null) return;

        for (int i = 0; i < 4; i++) {
            if (visualWalls[i] == null) continue;

            bool shouldBeActive = (mask & (1 << i)) != 0;
            bool isCurrentlyActive = (ActiveWalls & (1 << i)) != 0;

            if (shouldBeActive != isCurrentlyActive) {
                if (wallCoroutines[i] != null) StopCoroutine(wallCoroutines[i]);
                wallCoroutines[i] = StartCoroutine(AnimateWallSequence(i, shouldBeActive));
            }
        }
        ActiveWalls = mask;
        wallObject.SetActive(true);
    }

    private IEnumerator AnimateWallSequence(int index, bool show) {
        GameObject vWall = visualWalls[index];
        Tilemap vTilemap = vWall.GetComponent<Tilemap>();
        TilemapCollider2D rCol = realColliders[index];

        // 기준이 되는 로컬 위치 (원본 벽의 위치)
        Vector3 baseLocalPos = wallParts[index].transform.localPosition;

        if (show) {
            vWall.SetActive(true);
            if (rCol != null) rCol.enabled = true;
        }

        Vector3 startPos = show ? baseLocalPos + new Vector3(0, wallUpYOffset, 0) : baseLocalPos;
        Vector3 targetPos = show ? baseLocalPos : baseLocalPos + new Vector3(0, wallUpYOffset, 0);

        // ... (이후 Lerp 로직은 동일) ...
        float elapsed = 0f;
        while (elapsed < animationDuration) {
            elapsed += Time.deltaTime;
            float percent = Mathf.Clamp01(elapsed / animationDuration);
            float curve = Mathf.SmoothStep(0, 1, percent);

            vWall.transform.localPosition = Vector3.Lerp(startPos, targetPos, curve);
            if (vTilemap != null) {
                Color c = vTilemap.color;
                c.a = Mathf.Lerp(show ? 0f : 1f, show ? 1f : 0f, curve);
                vTilemap.color = c;
            }
            yield return null;
        }

        vWall.transform.localPosition = targetPos;
        if (!show) {
            if (rCol != null) rCol.enabled = false;
            vWall.SetActive(false);
        }
        wallCoroutines[index] = null;
    }

    /// <summary>
    /// 입력된 mask에 해당하는 벽들을 추가로 활성화합니다.
    /// </summary>
    public void SetWallsActive(int mask = 15) {
        // 기존 상태에 새로운 비트를 추가 (OR 연산)
        int nextState = ActiveWalls | mask;
        SetWalls(nextState);
    }

    /// <summary>
    /// 입력된 mask에 해당하는 벽들을 비활성화합니다.
    /// </summary>
    public void SetWallsInactive(int mask = 15) {
        // 기존 상태에서 해당 비트만 제거 (NOT 연산 후 AND)
        int nextState = ActiveWalls & ~mask;
        SetWalls(nextState);
    }
}