using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class FloorEnemyPrefabGroup {
    public int floor = 1;
    public List<GameObject> enemyPrefabs = new List<GameObject>();
}

[System.Serializable]
public class SpecialRoomEnemyGroup {
    public RoomType roomType = RoomType.Fire;
    public List<FloorEnemyPrefabGroup> floorEnemyGroups = new List<FloorEnemyPrefabGroup>();
}

public static class RoomTypeHelper {
    public static bool IsSpecialEnemyRoom(RoomType roomType) {
        return roomType == RoomType.Fire
            || roomType == RoomType.Ice
            || roomType == RoomType.Lightning;
    }

    public static bool IsEnemyRoom(RoomType roomType) {
        return roomType == RoomType.Normal || IsSpecialEnemyRoom(roomType);
    }

    public static RoomType[] GetSpecialRoomTypes() {
        return new RoomType[] {
            RoomType.Fire,
            RoomType.Ice,
            RoomType.Lightning
        };
    }

    public static string GetKoreanName(RoomType roomType) {
        switch (roomType) {
            case RoomType.Start:
                return "시작방";
            case RoomType.Normal:
                return "일반방";
            case RoomType.Empty:
                return "빈방";
            case RoomType.Shop:
                return "상점방";
            case RoomType.Boss:
                return "보스방";
            case RoomType.Chest:
                return "상자방";
            case RoomType.Fire:
                return "불";
            case RoomType.Ice:
                return "얼음";
            case RoomType.Lightning:
                return "번개";
            default:
                return roomType.ToString();
        }
    }
}

public class EnemySpawner : MonoBehaviour {
    [Header("Fallback Enemy Settings")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [Header("Normal Room Enemy Settings")]
    [SerializeField] private List<FloorEnemyPrefabGroup> normalEnemyGroups = new List<FloorEnemyPrefabGroup>();

    [Header("Special Room Enemy Settings")]
    [SerializeField] private List<SpecialRoomEnemyGroup> specialRoomEnemyGroups = new List<SpecialRoomEnemyGroup>();

    [Header("Normal Room Mix Settings")]
    [SerializeField, Range(0.0f, 1.0f)] private float specialEnemyChanceInNormalRoom = 0.1f;

    [Header("Boss Settings")]
    [SerializeField] private GameObject[] bossPrefabs;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 1.0f;
    [SerializeField] private float spawnRadius = 3.0f;
    [SerializeField] private int enemiesPerTick = 1;

    [Header("Debug Auto Spawn")]
    [SerializeField] private bool useAutoSpawnForDebug = false;
    [SerializeField] private int debugFloor = 1;

    private Transform player;

    void Start() {
        GameObject p = GameObject.Find("Player");

        if (p != null) {
            player = p.transform;
        }

        if (useAutoSpawnForDebug) {
            StartCoroutine(SpawnRoutine());
        }
    }

    IEnumerator SpawnRoutine() {
        if (spawnInterval <= 0) {
            yield break;
        }

        while (player != null && player.gameObject.activeSelf) {
            SpawnEnemy(enemiesPerTick, debugFloor);
            yield return new WaitForSeconds(spawnInterval);
        }
    }



    public List<GameObject> SpawnByRoomData(RoomData roomData, int currentFloor, Vector3 spawnCenter) {
        List<GameObject> spawnedList = new List<GameObject>();

        Debug.Log($"<color=cyan><b>[SpawnByRoomData]</b></color> 호출됨 / CurrentFloor: {currentFloor}, SpawnCenter: {spawnCenter}");

        if (roomData == null) {
            Debug.Log("<color=#FFA500><b>주의!</b></color> SpawnByRoomData에 전달된 RoomData가 null입니다.");
            return spawnedList;
        }

        Debug.Log($"<color=cyan><b>[SpawnByRoomData]</b></color> RoomType: {roomData.type}, Status: {roomData.status}, MonsterCount: {roomData.monsterCount}, BossIndex: {roomData.bossIndex}");

        if (roomData.status == RoomData.RoomStatus.Cleared) {
            Debug.Log("<color=#FFA500><b>주의!</b></color> 이미 Cleared 상태인 방이라 몬스터를 스폰하지 않습니다.");
            return spawnedList;
        }

        if (roomData.type == RoomType.Boss) {
            Debug.Log($"<color=cyan><b>[SpawnByRoomData]</b></color> 보스방 처리 시작 / BossIndex: {roomData.bossIndex}");

            GameObject boss = SpawnBoss(roomData.bossIndex, spawnCenter);

            if (boss != null) {
                spawnedList.Add(boss);
                Debug.Log($"<color=cyan><b>[SpawnByRoomData]</b></color> 보스 스폰 성공: {boss.name}");
            }
            else {
                Debug.Log("<color=#FFA500><b>주의!</b></color> 보스 스폰 실패");
            }

            Debug.Log($"<color=cyan><b>[SpawnByRoomData]</b></color> 최종 스폰 수: {spawnedList.Count}");
            return spawnedList;
        }

        if (roomData.type == RoomType.Normal) {
            Debug.Log($"<color=cyan><b>[SpawnByRoomData]</b></color> 일반방 처리 시작 / MonsterCount: {roomData.monsterCount}");

            spawnedList.AddRange(SpawnEnemy(roomData.monsterCount, currentFloor, spawnCenter));

            Debug.Log($"<color=cyan><b>[SpawnByRoomData]</b></color> 일반방 최종 스폰 수: {spawnedList.Count}");
            return spawnedList;
        }

        if (RoomTypeHelper.IsSpecialEnemyRoom(roomData.type)) {
            Debug.Log($"<color=cyan><b>[SpawnByRoomData]</b></color> 특수방 처리 시작 / Type: {roomData.type}, MonsterCount: {roomData.monsterCount}");

            spawnedList.AddRange(SpawnSpecialEnemies(roomData.type, roomData.monsterCount, currentFloor, spawnCenter));

            Debug.Log($"<color=cyan><b>[SpawnByRoomData]</b></color> 특수방 최종 스폰 수: {spawnedList.Count}");
            return spawnedList;
        }

        Debug.Log($"<color=#FFA500><b>주의!</b></color> 전투방이 아닌 RoomType입니다. 스폰하지 않습니다. Type: {roomData.type}");
        return spawnedList;
    }

    public List<GameObject> SpawnByRoomData(RoomData roomData, int currentFloor) {
        return SpawnByRoomData(roomData, currentFloor, transform.position);
    }

    public List<GameObject> SpawnEnemy(int cnt) {
        return SpawnEnemy(cnt, debugFloor, transform.position);
    }

    public List<GameObject> SpawnEnemy(int cnt, int currentFloor) {
        return SpawnEnemy(cnt, currentFloor, transform.position);
    }

    public List<GameObject> SpawnEnemy(int cnt, int currentFloor, Vector3 spawnCenter) {
        List<GameObject> enemyList = new List<GameObject>();

        if (cnt <= 0) {
            return enemyList;
        }

        int usedFloor = -1;
        List<GameObject> normalPrefabs = GetNormalEnemyPrefabsByFloor(currentFloor, out usedFloor);

        if (normalPrefabs.Count == 0) {
            Debug.Log($"<color=#FFA500><b>주의!</b></color> {currentFloor}층 일반 몬스터 데이터가 없습니다. 일반 몬스터를 스폰하지 않습니다.");
            return enemyList;
        }

        if (usedFloor > 0 && usedFloor != currentFloor) {
            Debug.Log($"<color=#FFA500><b>주의!</b></color> {currentFloor}층 일반 몬스터 데이터가 없어 {usedFloor}층 일반 몬스터 데이터를 사용합니다.");
        }
        else if (usedFloor == 0) {
            Debug.Log($"<color=#FFA500><b>주의!</b></color> 층별 일반 몬스터 데이터가 없어 Fallback Enemy Settings를 사용합니다.");
        }

        List<GameObject> specialPrefabsForNormalRoom = GetAllAvailableSpecialEnemyPrefabs(currentFloor);

        for (int i = 0; i < cnt; i++) {
            List<GameObject> selectedPrefabs = normalPrefabs;

            if (specialPrefabsForNormalRoom.Count > 0 && Random.value < specialEnemyChanceInNormalRoom) {
                selectedPrefabs = specialPrefabsForNormalRoom;
            }

            GameObject enemy = SpawnOneFromList(selectedPrefabs, spawnCenter);

            if (enemy != null) {
                enemyList.Add(enemy);
            }
        }

        return enemyList;
    }

    public List<GameObject> SpawnSpecialEnemies(RoomType roomType, int cnt, int currentFloor, Vector3 spawnCenter) {
        List<GameObject> enemyList = new List<GameObject>();

        Debug.Log($"<color=cyan><b>[SpawnSpecialEnemies]</b></color> 호출됨 / Type: {roomType}, Count: {cnt}, CurrentFloor: {currentFloor}");

        if (cnt <= 0) {
            Debug.Log($"<color=#FFA500><b>주의!</b></color> 특수방 몬스터 수가 0 이하입니다. Type: {roomType}, Count: {cnt}");
            return enemyList;
        }

        if (!RoomTypeHelper.IsSpecialEnemyRoom(roomType)) {
            Debug.Log($"<color=#FFA500><b>주의!</b></color> {roomType}은 특수방 타입이 아닙니다. 일반 몬스터 스폰으로 대체합니다.");
            return SpawnEnemy(cnt, currentFloor, spawnCenter);
        }

        int usedFloor = -1;
        List<GameObject> specialPrefabs = GetSpecialEnemyPrefabsByRoomType(roomType, currentFloor, out usedFloor);

        Debug.Log($"<color=cyan><b>[SpawnSpecialEnemies]</b></color> 특수방 프리팹 조회 결과 / Type: {roomType}, 요청 층: {currentFloor}, 사용 층: {usedFloor}, 유효 프리팹 수: {specialPrefabs.Count}");

        if (specialPrefabs.Count == 0) {
            Debug.Log($"<color=#FFA500><b>주의!</b></color> {roomType} 특수방 몬스터 데이터가 없습니다. 일반 몬스터 스폰으로 대체합니다.");
            return SpawnEnemy(cnt, currentFloor, spawnCenter);
        }

        if (usedFloor != currentFloor) {
            Debug.Log($"<color=#FFA500><b>주의!</b></color> {currentFloor}층 {roomType} 특수방 데이터가 없어 {usedFloor}층 데이터를 사용합니다.");
        }

        for (int i = 0; i < cnt; i++) {
            GameObject enemy = SpawnOneFromList(specialPrefabs, spawnCenter);

            if (enemy != null) {
                enemyList.Add(enemy);
                Debug.Log($"<color=cyan><b>[SpawnSpecialEnemies]</b></color> 특수몹 스폰 성공 [{i}] = {enemy.name}");
            }
            else {
                Debug.Log($"<color=#FFA500><b>주의!</b></color> 특수몹 스폰 실패 [{i}]");
            }
        }

        Debug.Log($"<color=cyan><b>[SpawnSpecialEnemies]</b></color> 최종 스폰 수: {enemyList.Count}");
        return enemyList;
    }

    public GameObject SpawnBoss(int index) {
        return SpawnBoss(index, transform.position);
    }

    public GameObject SpawnBoss(int index, Vector3 spawnCenter) {
        if (bossPrefabs == null || bossPrefabs.Length == 0) {
            Debug.Log("<color=#FFA500><b>주의!</b></color> 보스 프리팹 배열이 비어 있습니다.");
            return null;
        }

        if (index < 0 || index >= bossPrefabs.Length) {
            Debug.Log($"<color=#FFA500><b>주의!</b></color> 보스 인덱스가 범위를 벗어났습니다. Index: {index}, Boss Count: {bossPrefabs.Length}");
            return null;
        }

        GameObject bossPrefab = bossPrefabs[index];

        if (bossPrefab == null) {
            Debug.Log($"<color=#FFA500><b>주의!</b></color> Boss Prefabs Element {index}가 null입니다.");
            return null;
        }

        GameObject boss = Instantiate(bossPrefab, spawnCenter, Quaternion.identity);
        return boss;
    }

    public int GetMonsterCountForRoom(RoomType roomType, int currentFloor) {
        if (!RoomTypeHelper.IsEnemyRoom(roomType)) {
            return 0;
        }

        int minCount = Mathf.Max(1, currentFloor);
        int maxCountExclusive = minCount + 3;

        return Random.Range(minCount, maxCountExclusive);
    }

    public List<RoomType> GetCreatableSpecialRoomTypes(int currentFloor) {
        List<RoomType> result = new List<RoomType>();
        RoomType[] specialRoomTypes = RoomTypeHelper.GetSpecialRoomTypes();

        Debug.Log($"<color=cyan><b>[EnemySpawner]</b></color> {currentFloor}층 특수방 몬스터 데이터 검사 시작");

        for (int i = 0; i < specialRoomTypes.Length; i++) {
            RoomType roomType = specialRoomTypes[i];

            int usedFloor = -1;
            List<GameObject> prefabs = GetSpecialEnemyPrefabsByRoomType(roomType, currentFloor, out usedFloor);

            if (prefabs.Count > 0) {
                result.Add(roomType);

                if (usedFloor == currentFloor) {
                    Debug.Log($"<color=cyan><b>[EnemySpawner]</b></color> {RoomTypeHelper.GetKoreanName(roomType)} 특수방 생성 가능. Floor: {usedFloor}, Monster Count: {prefabs.Count}");
                }
                else {
                    Debug.Log($"<color=#FFA500><b>주의!</b></color> {currentFloor}층 {RoomTypeHelper.GetKoreanName(roomType)} 특수방 데이터가 없어 {usedFloor}층 데이터로 생성 가능합니다. Monster Count: {prefabs.Count}");
                }
            }
            else {
                Debug.Log($"<color=#FFA500><b>주의!</b></color> {RoomTypeHelper.GetKoreanName(roomType)} 특수방에 사용할 몬스터 데이터가 없어 생성 대상에서 제외합니다.");
            }
        }

        Debug.Log($"<color=cyan><b>[EnemySpawner]</b></color> 생성 가능한 특수방 개수: {result.Count}");
        return result;
    }

    private GameObject SpawnOneFromList(IList<GameObject> prefabs, Vector3 spawnCenter) {
        if (prefabs == null || prefabs.Count == 0) {
            return null;
        }

        int randomIndex = Random.Range(0, prefabs.Count);
        GameObject enemyPrefab = prefabs[randomIndex];

        if (enemyPrefab == null) {
            return null;
        }

        Vector3 spawnPos = GetRandomSpawnPosition(spawnCenter);
        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        return enemy;
    }

    private Vector3 GetRandomSpawnPosition(Vector3 spawnCenter) {
        Vector2 randomDir = Random.insideUnitCircle;

        if (randomDir.sqrMagnitude <= 0.0001f) {
            randomDir = Vector2.right;
        }

        randomDir = randomDir.normalized;

        Vector3 spawnPos = spawnCenter;
        spawnPos += (Vector3)randomDir * spawnRadius;

        return spawnPos;
    }

    private List<GameObject> GetNormalEnemyPrefabsByFloor(int currentFloor, out int usedFloor) {
        List<GameObject> prefabs = GetBestFloorEnemyPrefabs(normalEnemyGroups, currentFloor, out usedFloor);

        if (prefabs.Count > 0) {
            return prefabs;
        }

        usedFloor = 0;

        List<GameObject> fallbackPrefabs = new List<GameObject>();
        AddValidPrefabs(fallbackPrefabs, enemyPrefabs);

        return fallbackPrefabs;
    }

    private List<GameObject> GetSpecialEnemyPrefabsByRoomType(RoomType roomType, int currentFloor, out int usedFloor) {
        usedFloor = -1;

        SpecialRoomEnemyGroup specialGroup = FindSpecialRoomEnemyGroup(roomType);

        if (specialGroup == null) {
            return new List<GameObject>();
        }

        return GetBestFloorEnemyPrefabs(specialGroup.floorEnemyGroups, currentFloor, out usedFloor);
    }

    private List<GameObject> GetAllAvailableSpecialEnemyPrefabs(int currentFloor) {
        List<GameObject> result = new List<GameObject>();
        RoomType[] specialRoomTypes = RoomTypeHelper.GetSpecialRoomTypes();

        for (int i = 0; i < specialRoomTypes.Length; i++) {
            int usedFloor = -1;
            List<GameObject> prefabs = GetSpecialEnemyPrefabsByRoomType(specialRoomTypes[i], currentFloor, out usedFloor);

            for (int j = 0; j < prefabs.Count; j++) {
                result.Add(prefabs[j]);
            }
        }

        return result;
    }

    private SpecialRoomEnemyGroup FindSpecialRoomEnemyGroup(RoomType roomType) {
        if (specialRoomEnemyGroups == null) {
            return null;
        }

        for (int i = 0; i < specialRoomEnemyGroups.Count; i++) {
            SpecialRoomEnemyGroup group = specialRoomEnemyGroups[i];

            if (group == null) {
                continue;
            }

            if (group.roomType == roomType) {
                return group;
            }
        }

        return null;
    }

    private List<GameObject> GetBestFloorEnemyPrefabs(List<FloorEnemyPrefabGroup> groups, int currentFloor, out int usedFloor) {
        usedFloor = -1;

        List<GameObject> result = new List<GameObject>();

        if (groups == null || groups.Count == 0) {
            return result;
        }

        int startFloor = Mathf.Max(1, currentFloor);

        for (int floor = startFloor; floor >= 1; floor--) {
            result.Clear();

            for (int i = 0; i < groups.Count; i++) {
                FloorEnemyPrefabGroup group = groups[i];

                if (group == null) {
                    continue;
                }

                if (group.floor != floor) {
                    continue;
                }

                AddValidPrefabs(result, group.enemyPrefabs);
            }

            if (result.Count > 0) {
                usedFloor = floor;
                return new List<GameObject>(result);
            }
        }

        return new List<GameObject>();
    }

    private void AddValidPrefabs(List<GameObject> result, IList<GameObject> source) {
        if (result == null) {
            return;
        }

        if (source == null) {
            return;
        }

        for (int i = 0; i < source.Count; i++) {
            if (source[i] == null) {
                continue;
            }

            result.Add(source[i]);
        }
    }

    public void DebugPrintSpawnerData(int currentFloor) {
        Debug.Log($"<color=cyan><b>[EnemySpawner Debug]</b></color> ===== EnemySpawner 데이터 검사 시작 / CurrentFloor: {currentFloor} =====");

        DebugPrintNormalEnemyData(currentFloor);
        DebugPrintSpecialEnemyData(RoomType.Fire, currentFloor);
        DebugPrintSpecialEnemyData(RoomType.Ice, currentFloor);
        DebugPrintSpecialEnemyData(RoomType.Lightning, currentFloor);
        DebugPrintBossData();

        Debug.Log("<color=cyan><b>[EnemySpawner Debug]</b></color> ===== EnemySpawner 데이터 검사 끝 =====");
    }

    private void DebugPrintNormalEnemyData(int currentFloor) {
        int usedFloor = -1;
        List<GameObject> prefabs = GetNormalEnemyPrefabsByFloor(currentFloor, out usedFloor);

        Debug.Log($"<color=cyan><b>[EnemySpawner Debug]</b></color> 일반몹 검사 / 요청 층: {currentFloor}, 사용 층: {usedFloor}, 유효 프리팹 수: {prefabs.Count}");

        for (int i = 0; i < prefabs.Count; i++) {
            string prefabName = "null";

            if (prefabs[i] != null) {
                prefabName = prefabs[i].name;
            }

            Debug.Log($"<color=cyan><b>[EnemySpawner Debug]</b></color> 일반몹 [{i}] = {prefabName}");
        }
    }

    private void DebugPrintSpecialEnemyData(RoomType roomType, int currentFloor) {
        int usedFloor = -1;
        List<GameObject> prefabs = GetSpecialEnemyPrefabsByRoomType(roomType, currentFloor, out usedFloor);

        Debug.Log($"<color=cyan><b>[EnemySpawner Debug]</b></color> 특수방 검사 / Type: {roomType}, 요청 층: {currentFloor}, 사용 층: {usedFloor}, 유효 프리팹 수: {prefabs.Count}");

        if (prefabs.Count == 0) {
            Debug.Log($"<color=#FFA500><b>주의!</b></color> {roomType} 특수방에 사용할 유효 몬스터 프리팹이 없습니다.");
        }

        for (int i = 0; i < prefabs.Count; i++) {
            string prefabName = "null";

            if (prefabs[i] != null) {
                prefabName = prefabs[i].name;
            }

            Debug.Log($"<color=cyan><b>[EnemySpawner Debug]</b></color> {roomType} 몹 [{i}] = {prefabName}");
        }
    }

    private void DebugPrintBossData() {
        if (bossPrefabs == null) {
            Debug.Log("<color=#FFA500><b>주의!</b></color> Boss Prefabs 배열이 null입니다.");
            return;
        }

        Debug.Log($"<color=cyan><b>[EnemySpawner Debug]</b></color> 보스 프리팹 수: {bossPrefabs.Length}");

        for (int i = 0; i < bossPrefabs.Length; i++) {
            string prefabName = "null";

            if (bossPrefabs[i] != null) {
                prefabName = bossPrefabs[i].name;
            }

            Debug.Log($"<color=cyan><b>[EnemySpawner Debug]</b></color> Boss Prefabs [{i}] = {prefabName}");
        }
    }
}