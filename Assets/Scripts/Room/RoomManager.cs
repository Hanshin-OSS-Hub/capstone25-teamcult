using UnityEngine;
using System.Collections.Generic;
using System.Text; // 로그 문자열 조립용

[System.Serializable]
public class RewardWeight {
    public GameObject rewardPrefab;
    public float weight; // 등장 가중치
}

[System.Serializable]
public class RoomData {
    public enum RoomStatus { Empty, Locked, Cleared }

    public RoomStatus status = RoomStatus.Empty;
    public bool isFirstVisit = true; // 첫 방문 여부
    public bool shouldLockOnVisit = true; // 첫 방문 시 잠글지 여부

    public int monsterCount = 1; // 스폰할 몬스터 수
    public List<GameObject> rewardPrefabs = new List<GameObject>();

    //// 미사용 방 클리어 조건 체크 (예: 몬스터가 0마리인가?)
    //public bool CheckClearCondition() {
    //    return monsterCount <= 0;
    //}
}

public class RoomManager : MonoBehaviour {
    [SerializeField] Vector2Int roomSize = new Vector2Int(20, 20);
    [SerializeField] List<GameObject> allRooms;
    [SerializeField] List<GameObject>[] roomByDoors = new List<GameObject>[16];
    [SerializeField] int mapSize = 11;
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
        new Vector2Int(0, 1),  // 0: Up
        new Vector2Int(1, 0),  // 1: Right
        new Vector2Int(0, -1), // 2: Down
        new Vector2Int(-1, 0)  // 3: Left
    };

    [Header("Reward Settings")]
    [SerializeField] private List<RewardWeight> allRewards = new List<RewardWeight>();

    // --- (Awake, Start 등 기존 메서드 동일) ---
    void Awake() {
        for (int i = 0; i < 16; i++) roomByDoors[i] = new List<GameObject>();
        foreach (GameObject prefab in allRooms) {
            RoomConnector connector = prefab.GetComponent<RoomConnector>();
            if (connector == null) continue;
            int doorMask = 0;
            foreach (Direction direct in connector.availableDoors) doorMask |= 1 << (int)direct;
            roomByDoors[doorMask].Add(prefab);
        }
    }

    void Start() { 
        GenerateDungeon(); 
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
            // 시작방(루트) 세팅
            rooms[startPos.x, startPos.y].status = RoomData.RoomStatus.Empty;
            rooms[startPos.x, startPos.y].shouldLockOnVisit = false;

            // 각 가지별 위치 저장을 위한 리스트
            List<Vector2Int> mainBranchRooms = new List<Vector2Int>();
            List<Vector2Int> subBranchRooms = new List<Vector2Int>();
            List<Vector2Int> twigRooms = new List<Vector2Int>();

            // 1. 메인 가지 생성
            mainBranchRooms = CreateBranchList(startPos, mainBranchLength);
            if (mainBranchRooms == null) continue;

            // 2. 서브 가지 생성
            subBranchRooms = CreateBranchList(startPos, subBranchLength);
            if (subBranchRooms == null) continue;

            // 3. 잔가지 생성 준비 (몸통 합치기 - 각 가지의 마지막 칸 제외)
            List<Vector2Int> bodyRooms = new List<Vector2Int>();
            bodyRooms.Add(startPos); // 시작점 추가

            // 메인 가지: 마지막 칸을 제외하고 추가
            if (mainBranchRooms.Count > 0) {
                for (int i = 0; i < mainBranchRooms.Count - 1; i++) {
                    bodyRooms.Add(mainBranchRooms[i]);
                }
            }

            // 서브 가지: 마지막 칸을 제외하고 추가
            if (subBranchRooms.Count > 0) {
                for (int i = 0; i < subBranchRooms.Count - 1; i++) {
                    bodyRooms.Add(subBranchRooms[i]);
                }
            }

            // 4. 잔가지 생성
            int createdTwigs = 0;
            List<Vector2Int> shuffledBody = new List<Vector2Int>(bodyRooms);
            for (int i = 0; i < shuffledBody.Count; i++) {
                int rand = Random.Range(i, shuffledBody.Count);
                Vector2Int temp = shuffledBody[i];
                shuffledBody[i] = shuffledBody[rand];
                shuffledBody[rand] = temp;
            }

            foreach (var roomPos in shuffledBody) {
                if (createdTwigs >= twigCount) break;
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

            // --- GenerateDungeon 함수 하단 디버깅 출력부 ---
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<color=cyan><b>[던전 생성 보고서]</b></color>");
            sb.AppendLine($"시작 지점: {startPos}");
            sb.AppendLine($"<b>메인 가지 (길이: {mainBranchRooms.Count + 1}):</b> {startPos} -> {ListToString(mainBranchRooms, " -> ")}");
            sb.AppendLine($"<b>서브 가지 (길이: {subBranchRooms.Count + 1}):</b> {startPos} -> {ListToString(subBranchRooms, " -> ")}");
            sb.AppendLine($"<b>잔가지 (개수: {twigRooms.Count}):</b> {ListToString(twigRooms, ", ")}");
            // 총 방 개수 계산 (시작점 + 각 리스트의 카운트)
            int finalCount = 1 + mainBranchRooms.Count + subBranchRooms.Count + twigRooms.Count;
            sb.AppendLine($"<color=yellow>총 방 개수: {finalCount}</color>");
            Debug.Log(sb.ToString());
            
        }

        PreparePrefixWeights();
        for (int x = 0; x < mapSize; x++) {
            for (int y = 0; y < mapSize; y++) {
                // 방이 존재하는 위치인지 확인 (mapPlan 활용)
                if (mapPlan[x, y] > 0) {
                    if (x == mapSize / 2 && y == mapSize / 2) continue; // 시작 방은 제외
                    AssignRandomRewards(rooms[x, y]);
                }
            }
        }

        DrawMap();
    }

    // 리스트를 문자열로 예쁘게 변환해주는 유틸리티
    string ListToString(List<Vector2Int> list, string separator) {
        if (list.Count == 0) return "없음";
        return string.Join(separator, list);
    }

    // 가지 생성을 위한 전용 리스트 반환 함수
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
            if (!found) return null; // 생성 실패 시 null 반환
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
        for (int x = 0; x < mapSize; x++) {
            for (int y = 0; y < mapSize; y++) {
                int requiredMask = mapPlan[x, y];
                if (requiredMask > 0) {
                    GameObject prefab = GetRandomRoomByMask(requiredMask);
                    if (prefab != null) PlaceRoom(x - (mapSize / 2), y - (mapSize / 2), prefab);
                }
            }
        }
    }

    bool IsInsideMap(Vector2Int pos) { 
        return pos.x >= 0 && pos.x < mapSize && pos.y >= 0 && pos.y < mapSize; 
    }

    void PlaceRoom(int gridX, int gridY, GameObject roomPrefab) {
        Vector3 spawnPos = new Vector3(gridX * roomSize.x, gridY * roomSize.y, 0);
        Instantiate(roomPrefab, spawnPos, Quaternion.identity);
    }

    GameObject GetRandomRoomByMask(int mask) {
        if (roomByDoors[mask].Count > 0) return roomByDoors[mask][Random.Range(0, roomByDoors[mask].Count)];
        return null;
    }

    public int GetDoorMask(int x, int y) {
        if (x < 0 || x >= mapSize || y < 0 || y >= mapSize) return 0;
        return mapPlan[x, y];
    }

    private List<float> prefixWeights = new List<float>();
    private float totalWeightSum = 0;

    void PreparePrefixWeights() {
        prefixWeights.Clear();
        totalWeightSum = 0;
        foreach (var rw in allRewards) {
            totalWeightSum += rw.weight;
            prefixWeights.Add(totalWeightSum);
        }
    }

    /// <summary>
    /// 특정 방에 보상을 랜덤하게 할당합니다.
    /// </summary>
    void AssignRandomRewards(RoomData room) {
        if (allRewards == null || allRewards.Count == 0) return;
        // 보상 평균 개수 결정
        float goal = 0.5f; // 
        // N은 goal의 3배로 하되, 정수형으로 올림. 최소 3번은 던지도록 설정.
        int N = Mathf.Max(3, Mathf.CeilToInt(goal * 3));
        // 개별 시도의 성공 확률
        float p = goal / N;
        int rewardCount = 0;
        for (int i = 0; i < N; i++) {
            if (Random.value <= p) {
                rewardCount++;
            }
        }

        // 보상이 0개로 결정되면 종료
        if (rewardCount == 0) return;


        //  결정된 개수만큼 가중치 랜덤 보상 선택 (이분 탐색) ---
        for (int i = 0; i < rewardCount; i++) {
            float randomValue = Random.Range(0, totalWeightSum);

            // 이분 탐색 시작
            int low = 0;
            int high = prefixWeights.Count - 1;
            int selectedIndex = high;

            while (low <= high) {
                int mid = (low + high) / 2;
                if (prefixWeights[mid] >= randomValue) {
                    selectedIndex = mid; // 후보로 저장하고 더 작은 쪽을 탐색
                    high = mid - 1;
                }
                else {
                    low = mid + 1;
                }
            }

            room.rewardPrefabs.Add(allRewards[selectedIndex].rewardPrefab);
        }
    }
}