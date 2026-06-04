using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

public class SoundWaveController : MonoBehaviour
{
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
    [Range(0f, 1f)][SerializeField] private float soundVolume = 1.0f;
    [SerializeField] private float effectDuration = 2.0f;
    [SerializeField] private float maxDistortion = 0.08f;
    [SerializeField] private float maxRadius = 3.0f;
    [SerializeField] private float maxWorldRadius = 50.0f;
    [SerializeField] private float waveThickness = 0.05f;
    [SerializeField] private float hitThreshold = 1.0f;

    [Header("Score & Dynamic Effect Settings")]
    [SerializeField] private int waveScore = 0;
    [SerializeField] private int maxScore = 100;
    [SerializeField] private float scoreDecayDelay = 10f;
    private float _lastHitTime; 

    [Header("Screen Distortion (URP Feature)")]
    [SerializeField] private UniversalRendererData rendererData;

    private List<Wave> _activeWaves = new List<Wave>();
    private Camera _mainCam;
    private ScriptableRendererFeature _glitchFeature;

    private Vector4[] _centersArray = new Vector4[30];
    private float[] _radiiArray = new float[30];
    private float[] _strengthsArray = new float[30];

    private float _glitchTimer = 100f;
    private float _currentDynamicDuration = 1.0f;
    private static readonly int IntensityID = Shader.PropertyToID("_EffectIntensity");
    private Material _glitchMaterial;
    private bool _ownerDead;
    private bool _isCleaningUp;

    void Start()
    {
        if (rendererData != null)
        {
            _glitchFeature = rendererData.rendererFeatures.Find(x => x.name == "ScreenDistortionRF");
            if (_glitchFeature is FullScreenPassRendererFeature fullScreenPass) {
                _glitchMaterial = fullScreenPass.passMaterial;
            }
        }

        if (_glitchFeature != null) { _glitchFeature.SetActive(false); }
        _mainCam = Camera.main;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) {
            playerTransform = playerObj.transform;
        }
        else {
            Debug.LogWarning("Player 태그를 가진 오브젝트를 찾을 수 없습니다!");
        }
    }

    void Update() {
        UpdateAndUploadWaves();
        UpdateGlitchIntensity();
        HandleScoreDecay();
        TryCleanupAfterOwnerDeath();
    }

    public void TriggerScreenDistortion() {
        waveScore = Mathf.Min(waveScore + 10, maxScore);
        _lastHitTime = Time.time;
        _currentDynamicDuration = Mathf.Lerp(1.0f, 3.0f, (float)waveScore / maxScore);
        _glitchTimer = 0f;
        if (_glitchFeature != null) _glitchFeature.SetActive(true);
    }

    private void UpdateGlitchIntensity() {
        if (_glitchFeature == null || _glitchTimer > _currentDynamicDuration) return;

        _glitchTimer += Time.deltaTime;
        float intensity = 0f;
        float maxIntensity = Mathf.Lerp(0.3f, 1.0f, (float)waveScore / maxScore);

        if (_glitchTimer <= 0.5f) {
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

    private void HandleScoreDecay() {
        if (waveScore <= 0) return;

        if (Time.time - _lastHitTime >= scoreDecayDelay) {
            if (Time.frameCount % 60 == 0) {
                waveScore = Mathf.Max(waveScore - 1, 0);
            }
        }
    }

    public void CreateWave() {
        if (_ownerDead) return;
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

    public void SetOwnerTransform(Transform owner)
    {
        enemyTransform = owner;
    }

    public void NotifyOwnerDead()
    {
        _ownerDead = true;

        if (_activeWaves.Count == 0 && !IsDistortionRunning())
        {
            CleanupAndDestroy();
        }
    }

    private void UpdateAndUploadWaves()
    {
        if (rippleMaterial == null) return;

        for (int i = _activeWaves.Count - 1; i >= 0; i--)
        {
            Wave wave = _activeWaves[i];
            wave.Elapsed += Time.deltaTime;
            float progress = wave.Elapsed / effectDuration;
            wave.Radius = progress * maxRadius;
            if (!_ownerDead && enemyTransform != null)
            {
                wave.Center = GetEnemyScreenPos();
                wave.WorldPos = enemyTransform.position;
            }

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

    private Vector4 GetEnemyScreenPos()
    {
        if (enemyTransform == null || _mainCam == null) return new Vector4(0.5f, 0.5f, 0, 0);
        Vector3 viewPos = _mainCam.WorldToViewportPoint(enemyTransform.position);
        return new Vector4(viewPos.x, viewPos.y, 0, 0);
    }

    private void TryCleanupAfterOwnerDeath()
    {
        if (!_ownerDead || _isCleaningUp) return;
        if (_activeWaves.Count > 0) return;
        if (IsDistortionRunning()) return;

        CleanupAndDestroy();
    }

    private bool IsDistortionRunning()
    {
        return _glitchFeature != null && _glitchFeature.isActive;
    }

    private void CleanupAndDestroy()
    {
        if (_isCleaningUp) return;
        _isCleaningUp = true;

        _activeWaves.Clear();

        if (rippleMaterial != null)
        {
            rippleMaterial.SetInt("_ActiveWaveCount", 0);
            rippleMaterial.SetVectorArray("_WaveCenters", _centersArray);
            rippleMaterial.SetFloatArray("_WaveRadii", _radiiArray);
            rippleMaterial.SetFloatArray("_WaveStrengths", _strengthsArray);
        }

        if (_glitchFeature != null) _glitchFeature.SetActive(false);
        if (_glitchMaterial != null) _glitchMaterial.SetFloat(IntensityID, 0f);

        Destroy(gameObject);
    }

    private void OnDisable()
    {
        if (_glitchFeature != null) _glitchFeature.SetActive(false);
        if (_glitchMaterial != null) _glitchMaterial.SetFloat(IntensityID, 0f);

        if (rippleMaterial != null)
        {
            rippleMaterial.SetInt("_ActiveWaveCount", 0);
        }
    }
}
