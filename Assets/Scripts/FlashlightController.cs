using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    // 빛의 방향이 안 맞으면 이 값을 90, -90, 180 등으로 바꿔보세요.
    public float angleOffset = -90f;

    void Update()
    {
        // 1. 키보드 입력(WASD 또는 방향키)을 받아옵니다.
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 2. 이동 중일 때만 회전합니다. (멈춰있으면 마지막 방향 유지)
        // (h나 v 둘 중 하나라도 0이 아니면 = 키를 누르고 있으면)
        if (h != 0 || v != 0)
        {
            // 3. 입력받은 방향(x, y)을 각도로 변환합니다. (아크탄젠트 공식)
            float angle = Mathf.Atan2(v, h) * Mathf.Rad2Deg;

            // 4. 빛을 회전시킵니다.
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle + angleOffset));
        }
    }
}