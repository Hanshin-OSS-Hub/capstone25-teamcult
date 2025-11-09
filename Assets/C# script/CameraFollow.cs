using UnityEngine;

public class CameraFollow : MonoBehaviour
{
   
    public Transform target;

    
    [Range(0.01f, 1.0f)]
    public float smoothSpeed = 0.5f; 

    // 카메라의 Z축 깊이 (보통 2D나 쿼터뷰는 -10)
    private float cameraZ;

    void Start()
    {
        // 시작할 때 카메라의 Z축 값을 저장해 둡니다.
        cameraZ = transform.position.z;
    }

    void LateUpdate()
    {
        // 타겟(플레이어)이 설정되어 있는지 확인
        if (target != null)
        {
            // 카메라가 원하는 위치
            Vector3 desiredPosition = new Vector3(target.position.x, target.position.y, cameraZ);
            
            // 부드럽게 이동 (선형 보간)
            // Vector3.Lerp(현재위치, 목표위치, 속도)
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            
            // 카메라 위치 적용
            transform.position = smoothedPosition;
        }
    }
}