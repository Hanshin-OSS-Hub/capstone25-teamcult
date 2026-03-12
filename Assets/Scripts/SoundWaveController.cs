using UnityEngine;
using System.Collections.Generic;

public class SoundWaveController : MonoBehaviour {
    private class Wave {
        public Vector4 Center;    // 화면 좌표(쉐이더용)
        public Vector3 WorldPos;  // 월드 좌표(충돌 체크용)
        public float Radius;
        public float Strength;
        public float Elapsed;
        public bool HasHitPlayer; // 플레이어 적중 여부
    }

    [SerializeField] private Material rippleMaterial;
    [SerializeField] private Transform enemyTransform;   // 파동 근원지
    [SerializeField] private Transform playerTransform;  // 플레이어 (거리 체크 대상)

    [Header("Effect Control")]
    [Range(0f, 1f)] [SerializeField] private float soundVolume = 1.0f;
    [SerializeField] private float effectDuration = 2.0f;
    [SerializeField] private float maxDistortion = 0.08f;
    [SerializeField] private float maxRadius = 3.0f;       // 쉐이더용 반지름
    [SerializeField] private float maxWorldRadius = 20.0f; // 실제 월드에서 파동이 퍼지는 거리
    [SerializeField] private float waveThickness = 0.05f;  // 쉐이더 두께
    [SerializeField] private float hitThreshold = 1.0f;    // 충돌 판정 두께 (월드 단위)

    private List<Wave> _activeWaves = new List<Wave>();
    private Camera _mainCam;

    private Vector4[] _centersArray = new Vector4[30];
    private float[] _radiiArray = new float[30];
    private float[] _strengthsArray = new float[30];

    void Start() {
        _mainCam = Camera.main;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Z)) {
            CreateWave();
        }
        UpdateAndUploadWaves();
    }

    private void CreateWave() {
        if (_activeWaves.Count >= 30) return;

        _activeWaves.Add(new Wave {
            Center = GetEnemyScreenPos(),
            WorldPos = enemyTransform.position, // 생성 시점의 적 위치 저장
            Radius = 0f,
            Strength = soundVolume * maxDistortion,
            Elapsed = 0f,
            HasHitPlayer = false
        });
    }

    private void UpdateAndUploadWaves() {
        if (rippleMaterial == null) return;

        for (int i = _activeWaves.Count - 1; i >= 0; i--) {
            Wave wave = _activeWaves[i];
            wave.Elapsed += Time.deltaTime;
            float progress = wave.Elapsed / effectDuration;
            wave.Radius = progress * maxRadius;

            // --- 충돌 판정 로직 시작 ---
            if (!wave.HasHitPlayer && playerTransform != null) {
                // 1. 현재 파동의 월드 반지름 계산
                float currentWorldRadius = progress * maxWorldRadius;

                // 2. 근원지로부터 플레이어까지의 거리 계산
                float distanceToPlayer = Vector3.Distance(wave.WorldPos, playerTransform.position);

                // 3. 플레이어가 파동의 테두리(반지름) 근처에 있는지 체크
                if (Mathf.Abs(distanceToPlayer - currentWorldRadius) < hitThreshold) {
                    wave.HasHitPlayer = true;
                    Debug.Log($"<color=red><b>[HIT]</b></color> 파동에 맞았습니다! (파동 ID: {wave.GetHashCode()})");
                    // 여기서 플레이어 데미지 함수를 호출하면 됩니다. playerTransform.GetComponent<Player>().TakeDamage();
                }
            }
            // --- 충돌 판정 로직 끝 ---

            if (progress >= 1.0f) {
                _activeWaves.RemoveAt(i);
                continue;
            }

            _centersArray[i] = wave.Center;
            _radiiArray[i] = wave.Radius;
            _strengthsArray[i] = wave.Strength;
        }

        // 쉐이더 데이터 전송
        rippleMaterial.SetInt("_ActiveWaveCount", _activeWaves.Count);
        rippleMaterial.SetVectorArray("_WaveCenters", _centersArray);
        rippleMaterial.SetFloatArray("_WaveRadii", _radiiArray);
        rippleMaterial.SetFloatArray("_WaveStrengths", _strengthsArray);
        rippleMaterial.SetFloat("_WaveThickness", waveThickness);
    }

    private Vector4 GetEnemyScreenPos() {
        if (enemyTransform == null || _mainCam == null) return new Vector4(0.5f, 0.5f, 0, 0);
        Vector3 viewPos = _mainCam.WorldToViewportPoint(enemyTransform.position);
        return new Vector4(viewPos.x, viewPos.y, 0, 0);
    }
}