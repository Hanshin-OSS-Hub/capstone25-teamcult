using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;  
    public Vector3 offset = new Vector3(0, 0, -10f); // 카메라와 플레이어의 거리

    // LateUpdate는 모든 움직임(Update)이 끝난 후 호출되므로 카메라 떨림 방지에 좋습니다.
    void LateUpdate()
    {
        if (target != null)
        {
            // 카메라의 위치를 플레이어 위치 + 오프셋으로 설정
            transform.position = target.position + offset;
        }
    }
}