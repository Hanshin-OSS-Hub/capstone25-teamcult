using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

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
    [SerializeField] private Material rippleMaterial;
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

    [Header("Score & Dynamic Effect Settings")]
    [SerializeField] private int waveScore = 0;
    [SerializeField] private int maxScore = 100; // 점수 최대치
    [SerializeField] private float scoreDecayDelay = 10f; // 점수 감소 시작 대기시간
    private float _lastHitTime; // 마지막으로 맞은 시간 저장

    [Header("Screen Distortion (URP Feature)")]
    [SerializeField] private UniversalRendererData rendererData;

    private List<Wave> _activeWaves = new List<Wave>();
    private Camera _mainCam;
    private ScriptableRendererFeature _glitchFeature;

    private Vector4[] _centersArray = new Vector4[30];
    private float[] _radiiArray = new float[30];
    private float[] _strengthsArray = new float[30];

    private float _glitchTimer = 100f;
    private float _currentDynamicDuration = 1.0f; // 현재 계산된 지속 시간
    private static readonly int IntensityID = Shader.PropertyToID("_EffectIntensity");
    private Material _glitchMaterial;

    void Start() {
        if (rendererData != null) {
            _glitchFeature = rendererData.rendererFeatures.Find(x => x.name == "ScreenDistortionRF");
            if (_glitchFeature is FullScreenPassRendererFeature fullScreenPass) {
                _glitchMaterial = fullScreenPass.passMaterial;
            }
        }

        if (_glitchFeature != null) _glitchFeature.SetActive(false);
        _mainCam = Camera.main;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.X)) CreateWave();

        UpdateAndUploadWaves();
        UpdateGlitchIntensity();
        HandleScoreDecay(); // 점수 감소 로직 추가
    }

    // [변경] 피격 시 점수 기반 로직
    public void TriggerScreenDistortion() {
        // 1. 점수 추가 및 마지막 피격 시간 갱신
        waveScore = Mathf.Min(waveScore + 10, maxScore);
        _lastHitTime = Time.time;

        // 2. 점수에 따른 이펙트 지속 시간 계산 (최소 1초 ~ 최대 3초)
        // 예: 0점일 때 1초, 100점일 때 3초
        _currentDynamicDuration = Mathf.Lerp(1.0f, 3.0f, (float)waveScore / maxScore);

        // 3. 타이머 리셋 및 피쳐 활성화
        _glitchTimer = 0f;
        if (_glitchFeature != null) _glitchFeature.SetActive(true);
    }

    private void UpdateGlitchIntensity() {
        if (_glitchFeature == null || _glitchTimer > _currentDynamicDuration) return;

        _glitchTimer += Time.deltaTime;
        float intensity = 0f;

        // 점수가 높을수록 이펙트의 기본 강도(Base Intensity)도 강해지게 설정 가능
        float maxIntensity = Mathf.Lerp(0.3f, 1.0f, (float)waveScore / maxScore);

        if (_glitchTimer <= 0.5f) { // 도입부는 빠르게 강해짐
            intensity = maxIntensity;
        }
        else if (_glitchTimer <= _currentDynamicDuration) {
            float t = (_glitchTimer - 0.5f) / (_currentDynamicDuration - 0.5f);
            intensity = Mathf.Lerp(maxIntensity, 0.0f, t);
        }
        else {
            _glitchFeature.SetActive(false);
        }

        if (_glitchMaterial != null) {
            _glitchMaterial.SetFloat(IntensityID, intensity);
        }
    }

    // [추가] 점수 감소 로직
    private void HandleScoreDecay() {
        if (waveScore <= 0) return;

        // 마지막 피격으로부터 10초가 지났는지 확인
        if (Time.time - _lastHitTime >= scoreDecayDelay) {
            // 초당 1점씩 감소 (Time.deltaTime 사용)
            float decayAmount = Time.deltaTime * 1.0f;

            // 정수 점수 처리를 위해 별도의 float 변수를 쓰거나, 점진적으로 깎음
            if (Time.frameCount % 60 == 0) { // 매 프레임 깎으면 너무 빠르므로 대략 1초마다 1점 하락
                waveScore = Mathf.Max(waveScore - 1, 0);
            }
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
                    TriggerScreenDistortion();
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

    private void OnDisable() {
        if (_glitchFeature != null) _glitchFeature.SetActive(false);
        if (_glitchMaterial != null) _glitchMaterial.SetFloat(IntensityID, 0f);
    }
}