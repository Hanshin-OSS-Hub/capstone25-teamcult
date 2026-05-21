using UnityEngine;
using System.Collections.Generic;
using System.Text;

public enum RoomType { Start, Normal, Empty, Shop, Boss, Chest }

[System.Serializable]
public class RoomTypeGroup {
    public RoomType type;
    public List<GameObject> interiorPrefabs; // 해당 타입에 속하는 내부 레이아웃 프리팹들
}

[System.Serializable]
public class RewardWeight {
    public GameObject rewardPrefab;
    public double weight;
}

[System.Serializable]
public class RoomData {
    public enum RoomStatus { Empty, Locked, Cleared }

    public RoomStatus status = RoomStatus.Empty;
    public RoomType type = RoomType.Normal;
    public bool isFirstVisit = true;
    public bool shouldLockOnVisit = true;

    public int monsterCount = 0;
    public int bossIndex = 0;
    public List<GameObject> rewardPrefabs = new List<GameObject>();
}

public class RoomManager : MonoBehaviour {
    [SerializeField] Vector2Int roomSize = new Vector2Int(20, 20);

    [Header("Wall Settings")]
    [SerializeField] List<GameObject> allWalls = new List<GameObject>();

    private Dictionary<string, GameObject> wallPrefabDict = new Dictionary<string, GameObject>();

    [SerializeField] int mapSize = 11;

    [Header("Room Interior Settings")]
    // 인스펙터에서 RoomType별로 프리팹 리스트를 설정할 수 있습니다.
    [SerializeField] List<RoomTypeGroup> roomGroups = new List<RoomTypeGroup>();

    // 빠른 탐색을 위한 딕셔너리
    private Dictionary<RoomType, List<GameObject>> roomGroupDict = new Dictionary<RoomType, List<GameObject>>();

    public int MapSize {
        get { return mapSize; }
    }

    [Header("Branch Settings")]
    [SerializeField] int maxRooms = 15;
    [SerializeField] int mainBranchLength = 6;
    [SerializeField] int subBranchLength = 5;
    [SerializeField] int twigCount = 4;

    private int[,] mapPlan;
    public RoomData[,] rooms;

    private Vector2Int[] directions = {
        new Vector2Int(0, 1),   // 0: 위
        new Vector2Int(1, 0),   // 1: 오른쪽
        new Vector2Int(0, -1),  // 2: 아래
        new Vector2Int(-1, 0)   // 3: 왼쪽
    };

    [Header("Reward Settings")]
    [SerializeField] private List<RewardWeight> allRewards = new List<RewardWeight>();

    private SumSegmentTree rewardWeightTree = new SumSegmentTree();
    private readonly System.Random rewardRandom = new System.Random();

    void Awake() {
        InitWallPrefabDict();

        // 방 그룹 딕셔너리 초기화
        foreach (var group in roomGroups) {
            if (!roomGroupDict.ContainsKey(group.type)) {
                roomGroupDict.Add(group.type, group.interiorPrefabs);
            }
        }
    }

    void Start() {
        GenerateDungeon();
    }

    void InitWallPrefabDict() {
        wallPrefabDict.Clear();

        foreach (GameObject prefab in allWalls) {
            if (prefab == null) {
                continue;
            }

            string key = GetWallKeyFromPrefabName(prefab.name);

            if (string.IsNullOrEmpty(key)) {
                Debug.LogWarning($"<color=yellow><b>[Wall]</b></color> 벽 프리팹 이름 형식이 잘못되었습니다: {prefab.name}");
                continue;
            }

            if (wallPrefabDict.ContainsKey(key)) {
                Debug.LogWarning($"<color=yellow><b>[Wall]</b></color> 중복된 벽 프리팹 키가 있습니다: {key}, Prefab: {prefab.name}");
                continue;
            }

            wallPrefabDict.Add(key, prefab);
        }

        CheckRequiredWallPrefabs();
    }

    string GetWallKeyFromPrefabName(string prefabName) {
        int openIndex = prefabName.LastIndexOf('(');
        int closeIndex = prefabName.LastIndexOf(')');

        if (openIndex < 0 || closeIndex < 0 || closeIndex <= openIndex + 1) {
            return "";
        }

        string key = prefabName.Substring(openIndex + 1, closeIndex - openIndex - 1);
        return key.Trim().ToLower();
    }

    void CheckRequiredWallPrefabs() {
        string[] requiredKeys = { "open", "0", "1", "2", "3" };

        foreach (string key in requiredKeys) {
            if (!wallPrefabDict.ContainsKey(key)) {
                Debug.LogError($"<color=red><b>[Wall]</b></color> 필수 벽 프리팹이 allWalls에 없습니다: wall ({key})");
            }
        }
    }

    void GenerateDungeon() {
        int totalPlanned = 1 + (mainBranchLength - 1) + (subBranchLength - 1) + twigCount;

        if (totalPlanned > maxRooms) {
            twigCount = maxRooms - (1 + (mainBranchLength - 1) + (subBranchLength - 1));
            twigCount = Mathf.Max(0, twigCount);
        }

        bool generationSuccess = false;
        int safetyNet = 0;

        while (!generationSuccess && safetyNet < 100) {
            safetyNet++;
            mapPlan = new int[mapSize, mapSize];
            rooms = new RoomData[mapSize, mapSize];

            for (int x = 0; x < mapSize; x++) {
                for (int y = 0; y < mapSize; y++) {
                    rooms[x, y] = new RoomData();
                }
            }

            Vector2Int startPos = new Vector2Int(mapSize / 2, mapSize / 2);
            rooms[startPos.x, startPos.y].status = RoomData.RoomStatus.Empty;
            rooms[startPos.x, startPos.y].shouldLockOnVisit = false;

            List<Vector2Int> mainBranchRooms = new List<Vector2Int>();
            List<Vector2Int> subBranchRooms = new List<Vector2Int>();
            List<Vector2Int> twigRooms = new List<Vector2Int>();

            mainBranchRooms = CreateBranchList(startPos, mainBranchLength);

            if (mainBranchRooms == null) {
                continue;
            }

            subBranchRooms = CreateBranchList(startPos, subBranchLength);

            if (subBranchRooms == null) {
                continue;
            }

            List<Vector2Int> bodyRooms = new List<Vector2Int>();
            bodyRooms.Add(startPos);

            if (mainBranchRooms.Count > 0) {
                for (int i = 0; i < mainBranchRooms.Count - 1; i++) {
                    bodyRooms.Add(mainBranchRooms[i]);
                }
            }

            if (subBranchRooms.Count > 0) {
                for (int i = 0; i < subBranchRooms.Count - 1; i++) {
                    bodyRooms.Add(subBranchRooms[i]);
                }
            }

            int createdTwigs = 0;
            List<Vector2Int> shuffledBody = new List<Vector2Int>(bodyRooms);

            for (int i = 0; i < shuffledBody.Count; i++) {
                int rand = Random.Range(i, shuffledBody.Count);
                Vector2Int temp = shuffledBody[i];
                shuffledBody[i] = shuffledBody[rand];
                shuffledBody[rand] = temp;
            }

            foreach (var roomPos in shuffledBody) {
                if (createdTwigs >= twigCount) {
                    break;
                }

                int[] dirs = { 0, 1, 2, 3 };
                ShuffleArray(dirs);

                foreach (int d in dirs) {
                    Vector2Int twigPos = roomPos + directions[d];

                    if (IsInsideMap(twigPos) && mapPlan[twigPos.x, twigPos.y] == 0) {
                        mapPlan[roomPos.x, roomPos.y] |= (1 << d);
                        mapPlan[twigPos.x, twigPos.y] |= (1 << ((d + 2) % 4));
                        twigRooms.Add(twigPos);
                        createdTwigs++;
                        break;
                    }
                }
            }

            generationSuccess = true;
            Debug.Log($"<color=green><b>[1] 지도 생성 성공!</b></color> 생성된 방 개수: {1 + mainBranchRooms.Count + subBranchRooms.Count + twigRooms.Count}");
            AssignRoomTypes(startPos, mainBranchRooms, subBranchRooms, twigRooms);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<color=cyan><b>[던전 생성 보고서]</b></color>");
            sb.AppendLine($"시작 지점: {startPos}");
            sb.AppendLine($"<b>메인 가지 (길이: {mainBranchRooms.Count + 1}):</b> {startPos} -> {ListToString(mainBranchRooms, " -> ")}");
            sb.AppendLine($"<b>서브 가지 (길이: {subBranchRooms.Count + 1}):</b> {startPos} -> {ListToString(subBranchRooms, " -> ")}");
            sb.AppendLine($"<b>잔가지 (개수: {twigRooms.Count}):</b> {ListToString(twigRooms, ", ")}");

            int finalCount = 1 + mainBranchRooms.Count + subBranchRooms.Count + twigRooms.Count;
            sb.AppendLine($"<color=yellow>총 방 개수: {finalCount}</color>");
            Debug.Log(sb.ToString());
        }

        PrepareRewardWeightTree();

        for (int x = 0; x < mapSize; x++) {
            for (int y = 0; y < mapSize; y++) {
                if (mapPlan[x, y] > 0) {
                    if (x == mapSize / 2 && y == mapSize / 2) {
                        continue;
                    }

                    rooms[x, y].monsterCount = Random.Range(1, 4); // 방마다 1~3마리 랜덤
                    AssignRandomRewards(rooms[x, y]);
                }
            }
        }

        DrawMap();
    }

    string ListToString(List<Vector2Int> list, string separator) {
        if (list.Count == 0) {
            return "없음";
        }

        return string.Join(separator, list);
    }

    List<Vector2Int> CreateBranchList(Vector2Int start, int length) {
        List<Vector2Int> branch = new List<Vector2Int>();
        Vector2Int current = start;

        for (int i = 1; i < length; i++) {
            int[] dirs = { 0, 1, 2, 3 };
            ShuffleArray(dirs);

            bool found = false;

            foreach (int d in dirs) {
                Vector2Int next = current + directions[d];

                if (IsInsideMap(next) && mapPlan[next.x, next.y] == 0) {
                    mapPlan[current.x, current.y] |= (1 << d);
                    mapPlan[next.x, next.y] |= (1 << ((d + 2) % 4));
                    current = next;
                    branch.Add(current);
                    found = true;
                    break;
                }
            }

            if (!found) {
                return null;
            }
        }

        return branch;
    }

    void ShuffleArray(int[] array) {
        for (int i = array.Length - 1; i > 0; i--) {
            int j = Random.Range(0, i + 1);
            int temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }

    void DrawMap() {
        int spawnCount = 0;

        for (int x = 0; x < mapSize; x++) {
            for (int y = 0; y < mapSize; y++) {
                int requiredMask = mapPlan[x, y];

                if (requiredMask > 0) {
                    // 1. 벽 조각 조합 배치
                    GameObject spawnedRoom = PlaceRoomWalls(x - (mapSize / 2), y - (mapSize / 2), requiredMask);

                    if (spawnedRoom == null) {
                        continue;
                    }

                    spawnCount++;

                    // 2. 내부 레이아웃 배치
                    RoomType currentType = rooms[x, y].type;
                    GameObject interiorPrefab = GetRandomInteriorByType(currentType);

                    spawnedRoom.name = $"({x}, {y}) - {currentType}";

                    if (interiorPrefab != null) {
                        // 내부 프리팹을 벽 프리팹의 자식으로 생성하거나 같은 위치에 생성
                        Instantiate(interiorPrefab, spawnedRoom.transform.position, Quaternion.identity, spawnedRoom.transform);
                    }
                }
            }
        }

        Debug.Log($"<color=cyan><b>[3] 드로우 완료!</b></color> 실제 씬에 배치된 방 개수: {spawnCount}");
    }

    GameObject GetRandomInteriorByType(RoomType type) {
        if (roomGroupDict.ContainsKey(type) && roomGroupDict[type].Count > 0) {
            int randomIndex = Random.Range(0, roomGroupDict[type].Count);
            return roomGroupDict[type][randomIndex];
        }

        return null;
    }

    bool IsInsideMap(Vector2Int pos) {
        return pos.x >= 0 && pos.x < mapSize && pos.y >= 0 && pos.y < mapSize;
    }

    GameObject PlaceRoomWalls(int gridX, int gridY, int doorMask) {
        Vector3 spawnPos = new Vector3(gridX * roomSize.x, gridY * roomSize.y, 0);

        GameObject openPrefab = GetWallPrefab("open");

        if (openPrefab == null) {
            Debug.LogError("<color=red><b>[Wall]</b></color> wall (open) 프리팹이 없어서 방을 생성할 수 없습니다.");
            return null;
        }

        GameObject roomRoot = Instantiate(openPrefab, spawnPos, Quaternion.identity);

        // doorMask에서 bit가 1이면 해당 방향이 열린 것.
        // bit가 0이면 해당 방향이 막힌 것이므로 wall (0~3)을 추가로 설치.
        for (int direction = 0; direction < 4; direction++) {
            bool isOpen = (doorMask & (1 << direction)) != 0;

            if (!isOpen) {
                GameObject closedWallPrefab = GetWallPrefab(direction.ToString());

                if (closedWallPrefab == null) {
                    Debug.LogError($"<color=red><b>[Wall]</b></color> wall ({direction}) 프리팹이 없어서 해당 방향 벽을 막을 수 없습니다.");
                    continue;
                }

                GameObject closedWall = Instantiate(closedWallPrefab, spawnPos, Quaternion.identity, roomRoot.transform);
                closedWall.name = $"wall ({direction})";
            }
        }

        return roomRoot;
    }

    GameObject GetWallPrefab(string key) {
        key = key.ToLower();

        if (wallPrefabDict.ContainsKey(key)) {
            return wallPrefabDict[key];
        }

        return null;
    }

    public int GetDoorMask(int x, int y) {
        if (x < 0 || x >= mapSize || y < 0 || y >= mapSize) {
            return 0;
        }

        return mapPlan[x, y];
    }

    void PrepareRewardWeightTree() {
        double[] weights = new double[allRewards.Count];

        for (int i = 0; i < allRewards.Count; i++) {
            weights[i] = allRewards[i].weight;
        }

        rewardWeightTree.Build(weights);
    }

    // 방의 위치와 리스트를 바탕으로 타입을 결정하는 메서드
    void AssignRoomTypes(Vector2Int startPos, List<Vector2Int> main, List<Vector2Int> sub, List<Vector2Int> twigs) {
        // 1. 모든 방을 기본적으로 Normal로 초기화 (혹시 모를 중복 생성 방지)
        // (기존 코드에서 이미 생성되어 있다면 이 과정은 생략 가능합니다.)

        // 2. 시작 지점 설정
        rooms[startPos.x, startPos.y].type = RoomType.Start;

        // 3. 메인 가지(Main Branch)의 마지막 방 -> 보스방
        if (main != null && main.Count > 0) {
            Vector2Int bossPos = main[main.Count - 1];
            rooms[bossPos.x, bossPos.y].type = RoomType.Boss;
        }

        // 4. 서브 가지(Sub Branch)의 마지막 방 -> 상점
        if (sub != null && sub.Count > 0) {
            Vector2Int shopPos = sub[sub.Count - 1];
            rooms[shopPos.x, shopPos.y].type = RoomType.Shop;
        }

        // 5. 모든 잔가지(Twigs) -> 보물상자 방 (Chest)
        if (twigs != null) {
            foreach (var twigPos in twigs) {
                rooms[twigPos.x, twigPos.y].type = RoomType.Chest;
                // 보물상자 방에는 몬스터가 없어야 한다면 아래 설정 추가
                rooms[twigPos.x, twigPos.y].monsterCount = 0;
            }
        }
    }

    void AssignRandomRewards(RoomData room) {
        // 보상 대상 방이 아니거나 보상 데이터가 없으면 종료
        if (allRewards == null || allRewards.Count == 0) {
            return;
        }

        if (room.type != RoomType.Normal && room.type != RoomType.Boss) {
            return;
        }

        // 세그먼트 트리의 전체 가중치 합이 0이면 선택 불가
        if (rewardWeightTree.TotalSum <= 0.0) {
            return;
        }

        // [0, 전체합) 범위에서 랜덤 누적합 값 생성
        double randomValue = rewardRandom.NextDouble() * rewardWeightTree.TotalSum;

        // 세그먼트 트리에서 누적합 기준으로 선택될 보상 index 탐색
        int selectedIndex = rewardWeightTree.LowerBoundByPrefixSum(randomValue);

        if (selectedIndex < 0) {
            return;
        }

        // 원본 보상 리스트에서 선택된 프리팹을 방에 추가
        room.rewardPrefabs.Add(allRewards[selectedIndex].rewardPrefab);

        // 원본 보상 가중치를 절반으로 감소
        double newWeight = allRewards[selectedIndex].weight * 0.5;
        allRewards[selectedIndex].weight = newWeight;

        // 변경된 원본 가중치를 세그먼트 트리에 반영
        rewardWeightTree.SetValue(selectedIndex, newWeight);
    }
}