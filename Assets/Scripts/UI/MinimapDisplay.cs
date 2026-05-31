using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MinimapDisplay : MonoBehaviour {
    // 인스펙터에서 방 종류별 색상을 설정하기 위한 구조체
    [System.Serializable]
    public struct TypeColor {
        public RoomType roomType; // 체크할 방 종류
        public Color color;                // 표시할 색상
    }

    private RoomManager roomManager;
    private MoveMapBounds mapBounds;
    private RawImage rawImage;
    private Texture2D minimapTexture;

    [Header("Display Settings")]
    [Range(1, 20)]
    [SerializeField] private int scale = 5;
    private const int baseRoomSize = 5;

    [Header("Colors - Default")]
    [SerializeField] private Color wallOutlineColor = new Color(0.2f, 0.2f, 0.2f);
    [SerializeField] private Color wallInnerColor = Color.gray; // 아직 안 가본 방 (가시권)
    [SerializeField] private Color playerColor = Color.green;   // 현재 플레이어 위치
    [SerializeField] private Color pathColor = Color.white;
    [SerializeField] private Color emptyColor = new Color(0, 0, 0, 0f);

    [Header("Custom Room Type Colors")]
    [Tooltip("RoomType별로 미니맵에 표시할 색상을 설정하세요. (예: Boss - Red, Shop - Yellow)")]
    [SerializeField] private List<TypeColor> roomTypeColors = new List<TypeColor>();

    void Start() {
        rawImage = GetComponent<RawImage>();
        roomManager = Object.FindAnyObjectByType<RoomManager>();
        mapBounds = Object.FindAnyObjectByType<MoveMapBounds>();
        if (roomManager != null) InitMinimap();
    }

    void InitMinimap() {
        int mapDataSize = roomManager.MapSize;
        int totalPx = mapDataSize * baseRoomSize * scale;
        minimapTexture = new Texture2D(totalPx, totalPx);
        minimapTexture.filterMode = FilterMode.Point;
        minimapTexture.wrapMode = TextureWrapMode.Clamp;
        rawImage.texture = minimapTexture;
        UpdateRectSize(totalPx);
    }

    void UpdateRectSize(int size) {
        RectTransform rt = GetComponent<RectTransform>();
        if (rt != null) rt.sizeDelta = new Vector2(size, size);
    }

    void Update() {
        if (roomManager == null || mapBounds == null || minimapTexture == null) return;
        UpdateMinimap();
    }

    void UpdateMinimap() {
        int mapDataSize = roomManager.MapSize;
        Vector2Int playerIdx = mapBounds.CurrentRoomIndex;

        Color32[] resetPixels = new Color32[minimapTexture.width * minimapTexture.height];
        for (int i = 0; i < resetPixels.Length; i++) resetPixels[i] = emptyColor;
        minimapTexture.SetPixels32(resetPixels);

        for (int x = 0; x < mapDataSize; x++) {
            for (int y = 0; y < mapDataSize; y++) {
                int mask = roomManager.GetDoorMask(x, y);
                if (mask <= 0) continue;

                bool isVisited = IsRoomVisited(x, y, mapDataSize);
                bool isVisibleViaDoor = CheckVisibleViaDoor(x, y, mapDataSize);

                if (!isVisited && !isVisibleViaDoor) continue;

                int roomPixelBlock = baseRoomSize * scale;
                int startX = x * roomPixelBlock;
                int startY = y * roomPixelBlock;

                // --- 핵심 수정: RoomType 기반 색상 결정 ---
                Color innerColor;
                if (x == playerIdx.x && y == playerIdx.y) {
                    innerColor = playerColor; // 현재 위치 최우선
                }
                else if (isVisited) {
                    // 방문한 방은 RoomType 리스트에서 설정된 색상을 가져옴
                    innerColor = GetColorByType(roomManager.rooms[x, y].type);
                }
                else {
                    innerColor = wallInnerColor; // 방문 전 가시권 방
                }

                DrawScaledRoom(startX, startY, innerColor);
                DrawSmartDoors(x, y, startX, startY, mask, isVisited, innerColor);
            }
        }
        minimapTexture.Apply();
    }

    // RoomType에 맞는 색상을 리스트에서 찾아 반환
    private Color GetColorByType(RoomType type) {
        foreach (var tc in roomTypeColors) {
            if (tc.roomType == type) return tc.color;
        }
        return Color.white; // 리스트에 설정되지 않은 Type일 경우 기본 흰색
    }

    // --- 헬퍼 함수 (이전과 동일) ---
    bool CheckVisibleViaDoor(int x, int y, int size) {
        if (y + 1 < size && IsRoomVisited(x, y + 1, size) && (roomManager.GetDoorMask(x, y + 1) & (1 << 2)) != 0) return true;
        if (y - 1 >= 0 && IsRoomVisited(x, y - 1, size) && (roomManager.GetDoorMask(x, y - 1) & (1 << 0)) != 0) return true;
        if (x + 1 < size && IsRoomVisited(x + 1, y, size) && (roomManager.GetDoorMask(x + 1, y) & (1 << 3)) != 0) return true;
        if (x - 1 >= 0 && IsRoomVisited(x - 1, y, size) && (roomManager.GetDoorMask(x - 1, y) & (1 << 1)) != 0) return true;
        return false;
    }

    bool IsRoomVisited(int x, int y, int size) {
        // 이미 클리어했거나, 처음 방문이 아니거나(진입 중), 시작 방인 경우 방문으로 간주
        var room = roomManager.rooms[x, y];
        return (x == size / 2 && y == size / 2) ||
               room.status == RoomData.RoomStatus.Cleared ||
               !room.isFirstVisit;
    }

    void DrawScaledRoom(int startX, int startY, Color innerColor) {
        int fullSize = baseRoomSize * scale;
        for (int i = 0; i < fullSize; i++) {
            for (int j = 0; j < fullSize; j++) {
                bool isOutline = (i < scale || i >= fullSize - scale || j < scale || j >= fullSize - scale);
                minimapTexture.SetPixel(startX + i, startY + j, isOutline ? wallOutlineColor : innerColor);
            }
        }
    }

    void DrawSmartDoors(int x, int y, int startX, int startY, int mask, bool isVisited, Color innerColor) {
        int mapSize = roomManager.MapSize;
        Color doorPixelColor = (innerColor == wallInnerColor) ? wallOutlineColor : pathColor;
        int doorStart = 2 * scale;
        int doorEnd = 3 * scale;

        for (int d = doorStart; d < doorEnd; d++) {
            if ((mask & (1 << 0)) != 0 && (isVisited || (y + 1 < mapSize && IsRoomVisited(x, y + 1, mapSize))))
                for (int h = 0; h < scale; h++) minimapTexture.SetPixel(startX + d, startY + (baseRoomSize - 1) * scale + h, doorPixelColor);
            if ((mask & (1 << 1)) != 0 && (isVisited || (x + 1 < mapSize && IsRoomVisited(x + 1, y, mapSize))))
                for (int w = 0; w < scale; w++) minimapTexture.SetPixel(startX + (baseRoomSize - 1) * scale + w, startY + d, doorPixelColor);
            if ((mask & (1 << 2)) != 0 && (isVisited || (y - 1 >= 0 && IsRoomVisited(x, y - 1, mapSize))))
                for (int h = 0; h < scale; h++) minimapTexture.SetPixel(startX + d, startY + h, doorPixelColor);
            if ((mask & (1 << 3)) != 0 && (isVisited || (x - 1 >= 0 && IsRoomVisited(x - 1, y, mapSize))))
                for (int w = 0; w < scale; w++) minimapTexture.SetPixel(startX + w, startY + d, doorPixelColor);
        }
    }
}