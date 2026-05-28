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
    private BoxCollider2D areaCollider;
    private GameObject wallObject;
    private GameObject[] wallParts = new GameObject[4];
    private int ActiveWalls = 0;
    private List<GameObject> currentRoomEnemies = new List<GameObject>();
    private EnemySpawner enemySpawner;

    private GameObject[] visualWalls = new GameObject[4];
    private TilemapCollider2D[] realColliders = new TilemapCollider2D[4];

    [Header("Wall Animation Settings")]
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private float wallUpYOffset = 2.0f;
    private Coroutine[] wallCoroutines = new Coroutine[4];

    void Start() {
        areaCollider = transform.Find("Area")?.GetComponent<BoxCollider2D>();
        wallObject = transform.Find("Wall")?.gameObject;

        if (wallObject != null) {
            int[] childNames = { 0, 1, 2, 3 };
            for (int i = 0; i < 4; i++) {
                Transform child = wallObject.transform.Find(childNames[i].ToString());
                if (child != null)
                    wallParts[i] = child.gameObject;
            }
        }
        SetupWallSeparation();
        roomManager = Object.FindAnyObjectByType<RoomManager>();
        enemySpawner = Object.FindAnyObjectByType<EnemySpawner>();

        if (roomManager == null) Debug.LogError("RoomManager를 찾을 수 없습니다!");
        if (enemySpawner == null) Debug.LogError("EnemySpawner를 찾을 수 없습니다!");

        int startIdx = roomManager.MapSize / 2;
        currentRoomIndex = new Vector2Int(startIdx, startIdx);
    }

    void Update() {
        if (playerTarget == null || roomManager == null) return;
        UpdateCameraPosition();
        HandleWallLock();
        CheckEnemiesStatus();
    }

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

        if (playerPos.x > currentPos.x + halfSize) { transform.position += new Vector3(gridSize, 0, 0); currentRoomIndex.x += 1; hasMoved = true; }
        else if (playerPos.x < currentPos.x - halfSize) { transform.position -= new Vector3(gridSize, 0, 0); currentRoomIndex.x -= 1; hasMoved = true; }

        if (playerPos.y > currentPos.y + halfSize) { transform.position += new Vector3(0, gridSize, 0); currentRoomIndex.y += 1; hasMoved = true; }
        else if (playerPos.y < currentPos.y - halfSize) { transform.position -= new Vector3(0, gridSize, 0); currentRoomIndex.y -= 1; hasMoved = true; }

        if (hasMoved) {
            int x = currentRoomIndex.x;
            int y = currentRoomIndex.y;

            if (roomManager != null && roomManager.rooms != null) {
                if (x >= 0 && x < roomManager.MapSize && y >= 0 && y < roomManager.MapSize) {
                    RoomData movedRoom = roomManager.rooms[x, y];

                    if (movedRoom != null && !IsCombatRoom(movedRoom)) {
                        ClearWallsImmediately();
                    }
                }
            }

            PrintRoomInfo();
        }
    }

    private bool IsPlayerCompletelyInside() {
        if (areaCollider == null) return false;
        Bounds areaBounds = areaCollider.bounds;
        Collider2D pCol = playerTarget.GetComponent<Collider2D>();
        if (pCol != null)
            return areaBounds.Contains(pCol.bounds.min) && areaBounds.Contains(pCol.bounds.max);
        return areaBounds.Contains(playerTarget.position);
    }

    private void HandleWallLock() {
        if (wallObject == null || roomManager.rooms == null) {
            return;
        }

        int x = currentRoomIndex.x;
        int y = currentRoomIndex.y;

        if (x < 0 || x >= roomManager.MapSize || y < 0 || y >= roomManager.MapSize) {
            return;
        }

        RoomData currentRoom = roomManager.rooms[x, y];

        if (currentRoom == null) {
            return;
        }

        bool isInside = IsPlayerCompletelyInside();

        if (currentRoom.status == RoomData.RoomStatus.Empty && currentRoom.isFirstVisit) {
            if (isInside && currentRoom.shouldLockOnVisit) {
                currentRoom.isFirstVisit = false;

                if (IsCombatRoom(currentRoom)) {
                    currentRoom.status = RoomData.RoomStatus.Locked;

                    AddSpecialRoomEnterLog(currentRoom);

                    SpawnMonsters(currentRoom);
                }
                else {
                    currentRoomEnemies.Clear();
                    ClearWallsImmediately();

                    Debug.Log($"<color=cyan><b>[Room]</b></color> 비전투방 입장 / Type: {currentRoom.type}, 문 잠금 없음");

                    UnlockAndReward(currentRoom);
                }
            }
        }

        if (currentRoom.status == RoomData.RoomStatus.Locked) {
            int doorMask = roomManager.GetDoorMask(x, y);
            SetWalls(doorMask);

            if (currentRoomEnemies.Count == 0) {
                UnlockAndReward(currentRoom);
            }
        }
        else {
            if (IsCombatRoom(currentRoom)) {
                SetWalls(0);
            }
            else {
                ClearWallsImmediately();
            }
        }
    }

    private bool IsCombatRoom(RoomData room) {
        if (room == null) {
            return false;
        }

        return RoomTypeHelper.IsEnemyRoom(room.type) || room.type == RoomType.Boss;
    }

    private void AddSpecialRoomEnterLog(RoomData room) {
        if (room == null) {
            return;
        }

        if (!RoomTypeHelper.IsSpecialEnemyRoom(room.type)) {
            return;
        }

        if (LogManager.Instance == null) {
            return;
        }

        string roomName = RoomTypeHelper.GetKoreanName(room.type);
        LogManager.Instance.AddLog($"{roomName} 방에 진입했습니다.");
    }

    private void ClearWallsImmediately() {
        if (wallObject == null) {
            return;
        }

        for (int i = 0; i < 4; i++) {
            if (wallCoroutines[i] != null) {
                StopCoroutine(wallCoroutines[i]);
                wallCoroutines[i] = null;
            }

            if (realColliders[i] != null) {
                realColliders[i].enabled = false;
            }

            if (visualWalls[i] != null) {
                Tilemap visualTilemap = visualWalls[i].GetComponent<Tilemap>();

                if (visualTilemap != null) {
                    Color color = visualTilemap.color;
                    color.a = 0f;
                    visualTilemap.color = color;
                }

                if (wallParts[i] != null) {
                    visualWalls[i].transform.localPosition = wallParts[i].transform.localPosition + new Vector3(0, wallUpYOffset, 0);
                }

                visualWalls[i].SetActive(false);
            }
        }

        ActiveWalls = 0;
        wallObject.SetActive(true);
    }


    private void SpawnMonsters(RoomData room) {
        currentRoomEnemies = new List<GameObject>();

        if (room == null) {
            Debug.Log("<color=#FFA500><b>주의!</b></color> SpawnMonsters에 전달된 RoomData가 null입니다.");
            return;
        }

        if (!IsCombatRoom(room)) {
            Debug.Log($"<color=#FFA500><b>주의!</b></color> 전투방이 아닌 방에서 SpawnMonsters가 호출되었습니다. Type: {room.type}");
            return;
        }

        if (enemySpawner == null) {
            Debug.Log("<color=#FFA500><b>주의!</b></color> EnemySpawner가 없어서 몬스터를 소환할 수 없습니다.");
            return;
        }

        if (room.type == RoomType.Boss) {
            Debug.Log($"<color=purple>보스전 시작!</color> BossIndex: {room.bossIndex}");
        }
        else {
            Debug.Log($"<color=red>전투 시작!</color> Type: {room.type}, MonsterCount: {room.monsterCount}, Floor: {roomManager.CurrentFloor}");
        }

        List<GameObject> spawnedEnemies = enemySpawner.SpawnByRoomData(room, roomManager.CurrentFloor, transform.position);

        if (spawnedEnemies != null) {
            currentRoomEnemies.AddRange(spawnedEnemies);
        }

        Debug.Log($"<color=cyan><b>[MoveMapBounds]</b></color> 실제 등록된 적 수: {currentRoomEnemies.Count}");
    }

    [SerializeField] private DangerUIHandler dangerUIHandler;

    private void CheckEnemiesStatus() {
        int sum_danger = 0;
        for (int i = currentRoomEnemies.Count - 1; i >= 0; i--) {
            if (currentRoomEnemies[i] == null)
                currentRoomEnemies.RemoveAt(i);
            else {
                EnemyStats stats = currentRoomEnemies[i].GetComponent<EnemyStats>();
                if (stats != null) sum_danger += stats.danger;
            }
        }
        if (dangerUIHandler != null) dangerUIHandler.UpdateDangerUI(sum_danger);
    }

    private void UnlockAndReward(RoomData room) {
        room.status = RoomData.RoomStatus.Cleared;
        SetWalls(0);

        if (room.rewardPrefabs != null && room.rewardPrefabs.Count > 0) {
            foreach (GameObject prefab in room.rewardPrefabs) {
                if (prefab == null) continue;
                Vector3 spawnOffset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
                Instantiate(prefab, transform.position + spawnOffset, Quaternion.identity);
                Debug.Log($"<color=cyan>보상 생성:</color> {prefab.name}");
            }
        }

        if (room.type == RoomType.Boss) {
            GameObject elevatorDoorClose = GameObject.Find("ElevatorDoorClose");
            if (elevatorDoorClose != null) {
                elevatorDoorClose.SetActive(false);
                Debug.Log("<color=green>성공:</color> ElevatorDoorClose 오브젝트를 비활성화했습니다.");
            }
            else {
                Debug.LogWarning("<color=red>경고:</color> 'ElevatorDoorClose' 오브젝트를 찾을 수 없습니다.");
            }
        }
    }

    private void PrintRoomInfo() {
        if (roomManager.rooms == null) {
            return;
        }

        int x = currentRoomIndex.x;
        int y = currentRoomIndex.y;

        if (x < 0 || x >= roomManager.MapSize || y < 0 || y >= roomManager.MapSize) {
            return;
        }

        RoomData room = roomManager.rooms[x, y];

        if (room == null) {
            Debug.Log($"방 이동: [{x}, {y}] RoomData가 null입니다.");
            return;
        }

        Debug.Log($"방 이동: [{x}, {y}] 타입: {room.type}, 상태: {room.status}, 몬스터: {room.monsterCount}, 전투방: {IsCombatRoom(room)}");
    }

    private void SetupWallSeparation() {
        if (wallObject == null) return;
        for (int i = 0; i < 4; i++) {
            if (wallParts[i] == null) continue;
            realColliders[i] = wallParts[i].GetComponent<TilemapCollider2D>();
            TilemapRenderer tr = wallParts[i].GetComponent<TilemapRenderer>();
            if (tr != null) tr.enabled = false;
            if (realColliders[i] != null) realColliders[i].enabled = false;

            visualWalls[i] = Instantiate(wallParts[i], wallParts[i].transform.position, wallParts[i].transform.rotation, wallParts[i].transform.parent);
            visualWalls[i].name = wallParts[i].name + "_Visual";

            TilemapCollider2D duplicateCol = visualWalls[i].GetComponent<TilemapCollider2D>();
            if (duplicateCol != null) Destroy(duplicateCol);

            TilemapRenderer visualRenderer = visualWalls[i].GetComponent<TilemapRenderer>();
            if (visualRenderer != null) visualRenderer.enabled = true;

            Tilemap visualTilemap = visualWalls[i].GetComponent<Tilemap>();
            if (visualTilemap != null) {
                Color vc = visualTilemap.color;
                vc.a = 0f;
                visualTilemap.color = vc;
            }

            Vector3 originLocalPos = wallParts[i].transform.localPosition;
            visualWalls[i].transform.localPosition = originLocalPos + new Vector3(0, wallUpYOffset, 0);
            visualWalls[i].SetActive(false);
        }
    }

    public void SetWalls(int mask) {
        if (wallObject == null) return;
        bool anyWallClosed = false;
        bool anyWallOpened = false;

        for (int i = 0; i < 4; i++) {
            if (visualWalls[i] == null) continue;
            bool shouldBeActive = (mask & (1 << i)) != 0;
            bool isCurrentlyActive = (ActiveWalls & (1 << i)) != 0;
            if (shouldBeActive != isCurrentlyActive) {
                if (shouldBeActive) anyWallClosed = true;
                else anyWallOpened = true;
                if (wallCoroutines[i] != null) StopCoroutine(wallCoroutines[i]);
                wallCoroutines[i] = StartCoroutine(AnimateWallSequence(i, shouldBeActive));
            }
        }

        if (SFXManager.Instance != null) {
            if (anyWallClosed) SFXManager.Instance.PlaySFX(SFXType.DoorClose);
            if (anyWallOpened) SFXManager.Instance.PlaySFX(SFXType.DoorOpen);
        }

        ActiveWalls = mask;
        wallObject.SetActive(true);
    }

    private IEnumerator AnimateWallSequence(int index, bool show) {
        GameObject vWall = visualWalls[index];
        Tilemap vTilemap = vWall.GetComponent<Tilemap>();
        TilemapCollider2D rCol = realColliders[index];
        Vector3 baseLocalPos = wallParts[index].transform.localPosition;

        if (show) { vWall.SetActive(true); if (rCol != null) rCol.enabled = true; }

        Vector3 startPos = show ? baseLocalPos + new Vector3(0, wallUpYOffset, 0) : baseLocalPos;
        Vector3 targetPos = show ? baseLocalPos : baseLocalPos + new Vector3(0, wallUpYOffset, 0);

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
        if (!show) { if (rCol != null) rCol.enabled = false; vWall.SetActive(false); }
        wallCoroutines[index] = null;
    }

    public void SetWallsActive(int mask = 15) { SetWalls(ActiveWalls | mask); }
    public void SetWallsInactive(int mask = 15) { SetWalls(ActiveWalls & ~mask); }

    public void RegisterRoomEnemy(GameObject enemy) {
        if (enemy == null) return;
        if (currentRoomEnemies == null) currentRoomEnemies = new List<GameObject>();
        currentRoomEnemies.Add(enemy);
    }
}