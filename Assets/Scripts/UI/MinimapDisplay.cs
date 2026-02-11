using UnityEngine;
using UnityEngine.UI;

public class MinimapDisplay : MonoBehaviour {
    private RoomManager roomManager;
    private MoveMapBounds mapBounds;
    private RawImage rawImage;
    private Texture2D minimapTexture;

    [Header("Display Settings")]
    [Range(1, 20)]
    [SerializeField] private int scale = 5;
    private const int baseRoomSize = 5;

    [Header("Colors")]
    [SerializeField] private Color wallOutlineColor = new Color(0.2f, 0.2f, 0.2f);
    [SerializeField] private Color wallInnerColor = Color.gray;
    [SerializeField] private Color clearedInnerColor = Color.white;
    [SerializeField] private Color playerColor = Color.green;
    [SerializeField] private Color pathColor = Color.white;
    [SerializeField] private Color emptyColor = new Color(0, 0, 0, 0f);

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

                bool isVisited = (x == mapDataSize / 2 && y == mapDataSize / 2) || roomManager.rooms[x, y].status == RoomData.RoomStatus.Cleared;
                bool isVisibleViaDoor = CheckVisibleViaDoor(x, y, mapDataSize);

                if (!isVisited && !isVisibleViaDoor) continue;

                int roomPixelBlock = baseRoomSize * scale;
                int startX = x * roomPixelBlock;
                int startY = y * roomPixelBlock;

                Color innerColor = (x == playerIdx.x && y == playerIdx.y) ? playerColor :
                                  (isVisited ? clearedInnerColor : wallInnerColor);

                DrawScaledRoom(startX, startY, innerColor);

                // 핵심 수정: 방문하지 않았더라도 가시권에 있는 방이라면 연결된 문만 표시
                DrawSmartDoors(x, y, startX, startY, mask, isVisited, innerColor);
            }
        }
        minimapTexture.Apply();
    }

    bool CheckVisibleViaDoor(int x, int y, int size) {
        if (y + 1 < size && IsRoomVisited(x, y + 1, size) && (roomManager.GetDoorMask(x, y + 1) & (1 << 2)) != 0) return true;
        if (y - 1 >= 0 && IsRoomVisited(x, y - 1, size) && (roomManager.GetDoorMask(x, y - 1) & (1 << 0)) != 0) return true;
        if (x + 1 < size && IsRoomVisited(x + 1, y, size) && (roomManager.GetDoorMask(x + 1, y) & (1 << 3)) != 0) return true;
        if (x - 1 >= 0 && IsRoomVisited(x - 1, y, size) && (roomManager.GetDoorMask(x - 1, y) & (1 << 1)) != 0) return true;
        return false;
    }

    bool IsRoomVisited(int x, int y, int size) {
        return (x == size / 2 && y == size / 2) || roomManager.rooms[x, y].status == RoomData.RoomStatus.Cleared;
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

    // 방 상태에 따라 문을 영리하게 그리는 함수
    void DrawSmartDoors(int x, int y, int startX, int startY, int mask, bool isVisited, Color innerColor) {
        int mapSize = roomManager.MapSize;
        Color doorPixelColor = (innerColor == wallInnerColor) ? wallInnerColor : pathColor;
        int doorStart = 2 * scale;
        int doorEnd = 3 * scale;

        for (int d = doorStart; d < doorEnd; d++) {
            // 0: Up
            if ((mask & (1 << 0)) != 0 && (isVisited || (y + 1 < mapSize && IsRoomVisited(x, y + 1, mapSize))))
                for (int h = 0; h < scale; h++) minimapTexture.SetPixel(startX + d, startY + (baseRoomSize - 1) * scale + h, doorPixelColor);

            // 1: Right
            if ((mask & (1 << 1)) != 0 && (isVisited || (x + 1 < mapSize && IsRoomVisited(x + 1, y, mapSize))))
                for (int w = 0; w < scale; w++) minimapTexture.SetPixel(startX + (baseRoomSize - 1) * scale + w, startY + d, doorPixelColor);

            // 2: Down
            if ((mask & (1 << 2)) != 0 && (isVisited || (y - 1 >= 0 && IsRoomVisited(x, y - 1, mapSize))))
                for (int h = 0; h < scale; h++) minimapTexture.SetPixel(startX + d, startY + h, doorPixelColor);

            // 3: Left
            if ((mask & (1 << 3)) != 0 && (isVisited || (x - 1 >= 0 && IsRoomVisited(x - 1, y, mapSize))))
                for (int w = 0; w < scale; w++) minimapTexture.SetPixel(startX + w, startY + d, doorPixelColor);
        }
    }
}