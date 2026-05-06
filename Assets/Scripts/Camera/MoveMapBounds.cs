using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MoveMapBounds : MonoBehaviour
{
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

    void Start()
    {
        areaCollider = transform.Find("Area")?.GetComponent<BoxCollider2D>();
        wallObject = transform.Find("Wall")?.gameObject;

        if (wallObject != null)
        {
            int[] childNames = { 0, 1, 2, 3 };
            for (int i = 0; i < 4; i++)
            {
                Transform child = wallObject.transform.Find(childNames[i].ToString());
                if (child != null)
                {
                    wallParts[i] = child.gameObject;
                }
            }
        }
        SetupWallSeparation();
        roomManager = Object.FindAnyObjectByType<RoomManager>();
        enemySpawner = Object.FindAnyObjectByType<EnemySpawner>();

        if (roomManager == null) { Debug.LogError("RoomManager를 찾을 수 없습니다!"); }
        if (enemySpawner == null) { Debug.LogError("EnemySpawner를 찾을 수 없습니다!"); }

        int startIdx = roomManager.MapSize / 2;
        currentRoomIndex = new Vector2Int(startIdx, startIdx);
    }

    void Update()
    {
        if (playerTarget == null || roomManager == null) return;

        UpdateCameraPosition();
        HandleWallLock();
        CheckEnemiesStatus();

        if (false && Input.GetKeyDown(KeyCode.C))
        {
            ReduceMonsterCountTest();
        }
    }

    private void ReduceMonsterCountTest()
    {
        var room = roomManager.rooms[currentRoomIndex.x, currentRoomIndex.y];
        if (room.status == RoomData.RoomStatus.Locked)
        {
            room.monsterCount--;
            Debug.Log($"몬스터 처치! 남은 수: {room.monsterCount}");
        }
    }

    private void UpdateCameraPosition()
    {
        Vector3 currentPos = transform.position;
        Vector3 playerPos = playerTarget.position;
        float halfSize = gridSize / 2f;
        bool hasMoved = false;

        if (playerPos.x > currentPos.x + halfSize)
        {
            transform.position += new Vector3(gridSize, 0, 0);
            currentRoomIndex.x += 1;
            hasMoved = true;
        }
        else if (playerPos.x < currentPos.x - halfSize)
        {
            transform.position -= new Vector3(gridSize, 0, 0);
            currentRoomIndex.x -= 1;
            hasMoved = true;
        }

        if (playerPos.y > currentPos.y + halfSize)
        {
            transform.position += new Vector3(0, gridSize, 0);
            currentRoomIndex.y += 1;
            hasMoved = true;
        }
        else if (playerPos.y < currentPos.y - halfSize)
        {
            transform.position -= new Vector3(0, gridSize, 0);
            currentRoomIndex.y -= 1;
            hasMoved = true;
        }

        if (hasMoved)
        {
            PrintRoomInfo();
        }
    }

    private bool IsPlayerCompletelyInside()
    {
        if (areaCollider == null) return false;
        Bounds areaBounds = areaCollider.bounds;
        Collider2D pCol = playerTarget.GetComponent<Collider2D>();

        if (pCol != null)
        {
            return areaBounds.Contains(pCol.bounds.min) && areaBounds.Contains(pCol.bounds.max);
        }
        return areaBounds.Contains(playerTarget.position);
    }

    private void HandleWallLock()
    {
        if (wallObject == null || roomManager.rooms == null) return;

        int x = currentRoomIndex.x;
        int y = currentRoomIndex.y;

        if (x < 0 || x >= roomManager.MapSize || y < 0 || y >= roomManager.MapSize) return;

        RoomData currentRoom = roomManager.rooms[x, y];
        bool isInside = IsPlayerCompletelyInside();

        if (currentRoom.status == RoomData.RoomStatus.Empty && currentRoom.isFirstVisit)
        {
            if (isInside && currentRoom.shouldLockOnVisit)
            {
                currentRoom.status = RoomData.RoomStatus.Locked;
                currentRoom.isFirstVisit = false;
                SpawnMonsters(currentRoom);
            }
        }

        if (currentRoom.status == RoomData.RoomStatus.Locked)
        {
            int doorMask = roomManager.GetDoorMask(x, y);
            SetWalls(doorMask);

            if (currentRoomEnemies.Count == 0)
            {
                UnlockAndReward(currentRoom);
            }
        }
        else
        {
            SetWalls(0);
        }
    }

    private void SpawnMonsters(RoomData room)
    {
        if (room.type == RoomType.Normal)
        {
            if (room.monsterCount <= 0) return;

            Debug.Log($"<color=red>전투 시작!</color> {room.monsterCount}마리 소환 시도");

            if (enemySpawner != null)
            {
                currentRoomEnemies = enemySpawner.SpawnEnemy(room.monsterCount);
            }
        }
        else if (room.type == RoomType.Boss)
        {
            Debug.Log("<color=purple>보스전 시작!</color>");

            if (enemySpawner != null)
            {
                if (currentRoomEnemies == null) currentRoomEnemies = new List<GameObject>();

                GameObject boss = enemySpawner.SpawnBoss(room.bossIndex);

                if (boss != null)
                {
                    currentRoomEnemies.Add(boss);
                }
            }
        }
    }

    [SerializeField] private DangerUIHandler dangerUIHandler;

    private void CheckEnemiesStatus() {
        int sum_danger = 0;
        for (int i = currentRoomEnemies.Count - 1; i >= 0; i--) {
            if (currentRoomEnemies[i] == null) {
                currentRoomEnemies.RemoveAt(i);
            }
            else {
                EnemyStats stats = currentRoomEnemies[i].GetComponent<EnemyStats>();
                if (stats != null) {
                    sum_danger += stats.danger;
                }
            }
        }

        if (dangerUIHandler != null) {
            dangerUIHandler.UpdateDangerUI(sum_danger);
        }
    }

    private void UnlockAndReward(RoomData room)
    {
        room.status = RoomData.RoomStatus.Cleared;
        SetWalls(0); 

        

        if (room.rewardPrefabs != null && room.rewardPrefabs.Count > 0)
        {
            foreach (GameObject prefab in room.rewardPrefabs)
            {
                if (prefab == null) continue;

                Vector3 spawnOffset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
                Instantiate(prefab, transform.position + spawnOffset, Quaternion.identity);

                Debug.Log($"<color=cyan>보상 생성:</color> {prefab.name}");
            }
        }

        if (room.type == RoomType.Boss)
        {
            GameObject elevatorDoorClose = GameObject.Find("ElevatorDoorClose");

            if (elevatorDoorClose != null)
            {
                elevatorDoorClose.SetActive(false);
                Debug.Log("<color=green>성공:</color> ElevatorDoorClose 오브젝트를 비활성화했습니다.");
            }
            else
            {
                Debug.LogWarning("<color=red>경고:</color> 'ElevatorDoorClose' 오브젝트를 찾을 수 없습니다.");
            }
        }
    }

    private void PrintRoomInfo()
    {
        if (roomManager.rooms == null) return;
        var room = roomManager.rooms[currentRoomIndex.x, currentRoomIndex.y];
        Debug.Log($"방 이동: [{currentRoomIndex.x}, {currentRoomIndex.y}] 상태: {room.status}, 몬스터: {room.monsterCount}");
    }

    private void SetupWallSeparation()
    {
        if (wallObject == null) return;

        for (int i = 0; i < 4; i++)
        {
            if (wallParts[i] == null) continue;

            realColliders[i] = wallParts[i].GetComponent<TilemapCollider2D>();
            TilemapRenderer tr = wallParts[i].GetComponent<TilemapRenderer>();
            if (tr != null) tr.enabled = false;

            if (realColliders[i] != null)
            {
                realColliders[i].enabled = false;
            }

            visualWalls[i] = Instantiate(wallParts[i], wallParts[i].transform.position, wallParts[i].transform.rotation, wallParts[i].transform.parent);
            visualWalls[i].name = wallParts[i].name + "_Visual";

            TilemapCollider2D duplicateCol = visualWalls[i].GetComponent<TilemapCollider2D>();
            if (duplicateCol != null) Destroy(duplicateCol);

            TilemapRenderer visualRenderer = visualWalls[i].GetComponent<TilemapRenderer>();
            if (visualRenderer != null) visualRenderer.enabled = true;

            Tilemap visualTilemap = visualWalls[i].GetComponent<Tilemap>();
            if (visualTilemap != null)
            {
                Color vc = visualTilemap.color;
                vc.a = 0f;
                visualTilemap.color = vc;
            }

            Vector3 originLocalPos = wallParts[i].transform.localPosition;
            visualWalls[i].transform.localPosition = originLocalPos + new Vector3(0, wallUpYOffset, 0);
            visualWalls[i].SetActive(false);
        }
    }

    // ★ 수정된 SetWalls 함수: 문이 여러 개 움직여도 사운드는 한 번만 재생
    public void SetWalls(int mask)
    {
        if (wallObject == null) return;

        bool anyWallClosed = false; // 문이 닫힘 (벽이 생성됨)
        bool anyWallOpened = false; // 문이 열림 (벽이 사라짐)

        for (int i = 0; i < 4; i++)
        {
            if (visualWalls[i] == null) continue;

            bool shouldBeActive = (mask & (1 << i)) != 0;
            bool isCurrentlyActive = (ActiveWalls & (1 << i)) != 0;

            if (shouldBeActive != isCurrentlyActive)
            {
                if (shouldBeActive) anyWallClosed = true;
                else anyWallOpened = true;

                if (wallCoroutines[i] != null) StopCoroutine(wallCoroutines[i]);
                wallCoroutines[i] = StartCoroutine(AnimateWallSequence(i, shouldBeActive));
            }
        }

        // ★ 사운드 재생 로직 (for문 바깥에서 한 번만 재생)
        if (SFXManager.Instance != null)
        {
            if (anyWallClosed) SFXManager.Instance.PlaySFX(SFXType.DoorClose);
            if (anyWallOpened) SFXManager.Instance.PlaySFX(SFXType.DoorOpen);
        }

        ActiveWalls = mask;
        wallObject.SetActive(true);
    }

    private IEnumerator AnimateWallSequence(int index, bool show)
    {
        GameObject vWall = visualWalls[index];
        Tilemap vTilemap = vWall.GetComponent<Tilemap>();
        TilemapCollider2D rCol = realColliders[index];

        Vector3 baseLocalPos = wallParts[index].transform.localPosition;

        if (show)
        {
            vWall.SetActive(true);
            if (rCol != null) rCol.enabled = true;
        }

        Vector3 startPos = show ? baseLocalPos + new Vector3(0, wallUpYOffset, 0) : baseLocalPos;
        Vector3 targetPos = show ? baseLocalPos : baseLocalPos + new Vector3(0, wallUpYOffset, 0);

        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float percent = Mathf.Clamp01(elapsed / animationDuration);
            float curve = Mathf.SmoothStep(0, 1, percent);

            vWall.transform.localPosition = Vector3.Lerp(startPos, targetPos, curve);
            if (vTilemap != null)
            {
                Color c = vTilemap.color;
                c.a = Mathf.Lerp(show ? 0f : 1f, show ? 1f : 0f, curve);
                vTilemap.color = c;
            }
            yield return null;
        }

        vWall.transform.localPosition = targetPos;
        if (!show)
        {
            if (rCol != null) rCol.enabled = false;
            vWall.SetActive(false);
        }
        wallCoroutines[index] = null;
    }

    public void SetWallsActive(int mask = 15)
    {
        int nextState = ActiveWalls | mask;
        SetWalls(nextState);
    }

    public void SetWallsInactive(int mask = 15)
    {
        int nextState = ActiveWalls & ~mask;
        SetWalls(nextState);
    }
}