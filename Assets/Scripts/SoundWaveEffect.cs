using UnityEngine;

public class SoundWaveEffect : MonoBehaviour {
    public Material rippleMaterial;
    public Transform enemyTransform;
    public float effectDuration = 1.0f; // 조금 더 길게 잡는 것이 눈에 잘 띕니다.

    private float currentTimer = 0f;
    private Camera mainCamera;
    // 쉐이더 프로퍼티 ID를 미리 캐싱하면 성능이 향상됩니다.
    private static readonly int StrengthID = Shader.PropertyToID("_DistortionStrength");
    private static readonly int CenterID = Shader.PropertyToID("_WaveCenter");

    void Start() {
        mainCamera = Camera.main;
        // 시작할 때 효과 초기화
        rippleMaterial.SetFloat(StrengthID, 0f);
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Z)) {
            currentTimer = effectDuration;
            Debug.Log("Z누름");
        }

        if (currentTimer > 0) {
            currentTimer -= Time.deltaTime;

            // 0(시작) -> 1(끝)로 가는 진행률 계산
            float progress = 1.0f - (currentTimer / effectDuration);

            // 1. 강도 조절: 시작할 때 강하고 갈수록 약해지게 (또는 작성하신 Sin 방식 유지)
            // progress를 이용해 곡선을 그리면 더 찰집니다.
            float strength = Mathf.Lerp(0.1f, 0f, progress);
            rippleMaterial.SetFloat(StrengthID, strength);

            // 2. 적의 위치 업데이트
            if (enemyTransform != null) {
                Vector3 viewPos = mainCamera.WorldToViewportPoint(enemyTransform.position);

                // 적이 화면 안에 있을 때만 위치 갱신 (z > 0 이면 카메라 앞)
                if (viewPos.z > 0) {
                    rippleMaterial.SetVector(CenterID, new Vector4(viewPos.x, viewPos.y, 0, 0));
                }
            }
        }
        else if (rippleMaterial.GetFloat(StrengthID) > 0) {
            rippleMaterial.SetFloat(StrengthID, 0f);
        }
    }
}