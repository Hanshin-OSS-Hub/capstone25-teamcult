using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class SoundWaveController : MonoBehaviour {
    private class Wave {
        public Vector4 Center;
        public Vector3 WorldPos;
        public float Radius;
        public float Strength;
        public float Elapsed;
        public bool HasHitPlayer;
    }

    [Header("Wave Visuals (Ripple)")]
    [SerializeField] private Material rippleMaterial; // 파동 쉐이더가 적용된 머티리얼
    [SerializeField] private Transform enemyTransform;
    [SerializeField] private Transform playerTransform;

    [Header("Wave Stats")]
    [Range(0f, 1f)] [SerializeField] private float soundVolume = 1.0f;
    [SerializeField] private float effectDuration = 2.0f;
    [SerializeField] private float maxDistortion = 0.08f;
    [SerializeField] private float maxRadius = 3.0f;
    [SerializeField] private float maxWorldRadius = 50.0f;
    [SerializeField] private float waveThickness = 0.05f;
    [SerializeField] private float hitThreshold = 1.0f;

    [Header("Screen Distortion (URP Feature)")]
    [SerializeField] private UniversalRendererData rendererData;
    [SerializeField] private float glitchMaxDuration = 3.0f; // 총 지속 시간

    private List<Wave> _activeWaves = new List<Wave>();
    private Camera _mainCam;
    private ScriptableRendererFeature _glitchFeature;

    private Vector4[] _centersArray = new Vector4[30];
    private float[] _radiiArray = new float[30];
    private float[] _strengthsArray = new float[30];

    private float _glitchTimer = 100f;
    private static readonly int IntensityID = Shader.PropertyToID("_EffectIntensity");
    private Material _glitchMaterial;

    void Start() {
        if (rendererData != null) {
            _glitchFeature = rendererData.rendererFeatures.Find(x => x.name == "ScreenDistortionRF");

            if (_glitchFeature is FullScreenPassRendererFeature fullScreenPass) {
                // 원본 머티리얼을 직접 참조 (에디터 에셋 자체를 수정하게 됨)
                _glitchMaterial = fullScreenPass.passMaterial;

                if (_glitchMaterial == null) {
                    Debug.LogError("Renderer Feature에 머티리얼이 안 꽂혀있습니다!");
                }
            }
        }

        if (_glitchFeature != null) _glitchFeature.SetActive(false);
        _mainCam = Camera.main;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.X)) {
            CreateWave();
        }

        UpdateAndUploadWaves();
        UpdateGlitchIntensity();
    }
    private void OnDisable() {
        // 에디터에서 스크립트를 비활성화하거나 게임을 종료할 때 효과를 끕니다.
        if (_glitchFeature != null) {
            _glitchFeature.SetActive(false);
        }

        // 머티리얼 수치도 0으로 초기화해서 잔상이 남지 않게 합니다.
        if (_glitchMaterial != null) {
            _glitchMaterial.SetFloat(IntensityID, 0f);
        }
    }

    private void OnDestroy() {
        // 오브젝트가 파괴될 때도 확실히 꺼줍니다.
        if (_glitchFeature != null) {
            _glitchFeature.SetActive(false);
        }
    }

    // [추가 팁] 유니티 에디터의 인스펙터 값을 수정할 때 호출되는 함수
    private void OnValidate() {
        // 에디터에서 실수로 켜져 있다면 수동으로 끄기 편하게 체크박스를 연동할 수 있습니다.
        // 하지만 위 OnDisable만으로도 실행 전에는 꺼지게 됩니다.
    }

    // 효과를 시작하거나 리셋하는 함수 (명칭 변경)
    public void TriggerScreenDistortion() {
        _glitchTimer = 0f; // 맞을 때마다 0으로 초기화 (중첩 처리)

        if (_glitchFeature != null) {
            _glitchFeature.SetActive(true);
        }
    }

    private void UpdateGlitchIntensity() {


        if (_glitchFeature == null || _glitchTimer > glitchMaxDuration) return;

        _glitchTimer += Time.deltaTime;
        float intensity = 0f;

        if (_glitchTimer <= 1.0f) {
            intensity = 1.0f;
        }
        else if (_glitchTimer <= glitchMaxDuration) {
            float t = (_glitchTimer - 1.0f) / (glitchMaxDuration - 1.0f);
            intensity = Mathf.Lerp(1.0f, 0.0f, t);
        }
        else {
            intensity = 0f;
            _glitchFeature.SetActive(false);
            //Debug.Log("Glitch Effect Finished and Disabled");
        }

        // 로그 2: 실제 계산된 강도와 머티리얼 연결 상태 확인
        if (_glitchMaterial != null) {
            _glitchMaterial.SetFloat(IntensityID, intensity);
            // 이 로그가 콘솔에 찍혀야 정상입니다.
            //Debug.Log($"Glitch Active! Intensity: {intensity} | Material: {_glitchMaterial.name}");
        }
        else {
            Debug.LogError("Glitch Material is MISSING! Start에서 참조를 못 가져왔습니다.");
        }
    }

    private void CreateWave() {
        if (_activeWaves.Count >= 30 || enemyTransform == null) return;

        _activeWaves.Add(new Wave {
            Center = GetEnemyScreenPos(),
            WorldPos = enemyTransform.position,
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

            if (!wave.HasHitPlayer && playerTransform != null) {
                float currentWorldRadius = progress * maxWorldRadius;
                float distanceToPlayer = Vector3.Distance(wave.WorldPos, playerTransform.position);

                if (Mathf.Abs(distanceToPlayer - currentWorldRadius) < hitThreshold) {
                    wave.HasHitPlayer = true;
                    TriggerScreenDistortion(); // 플레이어 피격 시 실행
                }
            }

            if (progress >= 1.0f) {
                _activeWaves.RemoveAt(i);
                continue;
            }

            _centersArray[i] = wave.Center;
            _radiiArray[i] = wave.Radius;
            _strengthsArray[i] = wave.Strength;
        }

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