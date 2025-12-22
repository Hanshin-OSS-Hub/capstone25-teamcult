using UnityEngine;
using System.Collections.Generic;

public class RoomManager : MonoBehaviour {
    public Vector2Int roomSize = new Vector2Int(20, 20);
    public List<GameObject> allRooms;
    public List<GameObject>[] roomByDoors = new List<GameObject>[16];
    
    void Awake() { // 게임 시작 시 즉시 분류
        for (int i = 0; i < 16; i++) {
            roomByDoors[i] = new List<GameObject>();
        }
        // 열린 문에 따라 분류
        foreach (GameObject prefab in allRooms) {
            RoomConnector connector = prefab.GetComponent<RoomConnector>();
            if (connector == null) continue;
            int doorMask = 0;
            foreach (Direction direct in connector.availableDoors) { // 비트마스킹
                doorMask |= 1 << (int)direct;
            }
            roomByDoors[doorMask].Add(prefab);
        }
    }

    void PlaceRoom(int gridX, int gridY, GameObject roomPrefab) {
        // 방 크기를 곱해서 정확한 월드 위치 산출
        float spawnX = gridX * roomSize.x;
        float spawnY = gridY * roomSize.y;

        Vector3 spawnPos = new Vector3(spawnX, spawnY, 0);
        Instantiate(roomPrefab, spawnPos, Quaternion.identity);
    }

    void Start() {
        TestCreateConnectedRooms();
    }

    void TestCreateConnectedRooms() { // 테스트 방 생성
        // [중앙 방] 왼쪽(8)과 아래(4) 문이 있는 방 필요 (Mask: 12)
        // 12번 리스트에 방이 없을 수도 있으니 '최소한' 해당 문들을 포함하는지 체크하는 로직이 좋지만, 
        // 지금은 딱 맞는 방이 리스트에 있다고 가정하고 테스트합니다.

        GameObject centerPrefab = GetRandomRoomByMask(12); // 아래 커스텀 함수 참고
        GameObject leftPrefab = GetRandomRoomByMask(2);   // 오른쪽 문이 있는 방
        GameObject downPrefab = GetRandomRoomByMask(1);   // 위쪽 문이 있는 방

        // 방 배치 실행
        if (centerPrefab != null) PlaceRoom(0, 0, centerPrefab);
        if (leftPrefab != null) PlaceRoom(-1, 0, leftPrefab);
        if (downPrefab != null) PlaceRoom(0, -1, downPrefab);

        Debug.Log("테스트 방 배치 완료!");
    }

    // 특정 마스크를 가진 방 리스트에서 랜덤으로 하나를 뽑아주는 보조 함수
    GameObject GetRandomRoomByMask(int mask) {
        if (roomByDoors[mask].Count > 0) {
            int randomIndex = Random.Range(0, roomByDoors[mask].Count);
            return roomByDoors[mask][randomIndex];
        }
        else {
            Debug.LogError($"{mask}번 마스크를 가진 방이 리스트에 없습니다! 리스트를 확인해주세요.");
            return null;
        }
    }
}
