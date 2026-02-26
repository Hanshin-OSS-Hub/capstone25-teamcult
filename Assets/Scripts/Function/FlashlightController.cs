using UnityEngine;
using UnityEngine.Rendering.Universal; // 1. URP 조명을 쓰려면 이게 꼭 필요합니다!

public class FlashlightController : MonoBehaviour
{
    [Header("Settings")]
    public float angleOffset = -90f; // 빛 방향 보정
    public KeyCode toggleKey = KeyCode.F; // 끄고 킬 키

    private Light2D myLight; // 제어할 빛 컴포넌트

    void Start()
    {
        // 1. 내 몸에 붙은 Light 2D 컴포넌트를 찾아옵니다.
        myLight = GetComponent<Light2D>();

        // [추가된 부분] 시작하자마자 강제로 끕니다.
        if (myLight != null)
        {
            myLight.enabled = false;
        }
    }

    void Update()
    {
        // --- [추가된 기능] F키로 불 끄고 켜기 ---
        if (Input.GetKeyDown(toggleKey))
        {
            if (myLight != null)
            {
                // enabled가 true면 false로, false면 true로 뒤집습니다 (스위치)
                myLight.enabled = !myLight.enabled;
            }
        }

        // --- [기존 기능] 이동 방향 보면서 회전 ---
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        if (h != 0 || v != 0)
        {
            float angle = Mathf.Atan2(v, h) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle + angleOffset));
        }
    }
}