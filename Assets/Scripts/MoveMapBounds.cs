using UnityEngine;

public class MoveMapBounds : MonoBehaviour {
    [SerializeField] private Transform playerTarget; // 플레이어 설정
    [SerializeField] private int gridSize = 20;       // 격자 크기 (20)

    void Start() {
        // 시작 위치를 10, -10으로 설정
        transform.position = new Vector3(10f, -10f, transform.position.z);
    }

    void Update() {
        if (playerTarget == null) return;

        Vector3 currentPos = transform.position;
        Vector3 playerPos = playerTarget.position;

        // 영역의 반지름 계산 (20 기준 중심에서 사방으로 10씩)
        float halfSize = gridSize / 2f;

        // 새로운 위치를 저장할 변수
        float nextX = currentPos.x;
        float nextY = currentPos.y;

        // 플레이어가 오른쪽 경계를 벗어남
        if (playerPos.x > currentPos.x + halfSize) nextX += gridSize;
        // 플레이어가 왼쪽 경계를 벗어남
        else if (playerPos.x < currentPos.x - halfSize) nextX -= gridSize;

        // 플레이어가 위쪽 경계를 벗어남
        if (playerPos.y > currentPos.y + halfSize) nextY += gridSize;
        // 플레이어가 아래쪽 경계를 벗어남
        else if (playerPos.y < currentPos.y - halfSize) nextY -= gridSize;

        // 위치 변화가 있다면 업데이트
        transform.position = new Vector3(nextX, nextY, currentPos.z);
    }
}