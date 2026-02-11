using UnityEngine;
using System.Collections.Generic;

public class LayeredBgmPlayer : MonoBehaviour
{
    [Header("--- Samples (BPM 130) ---")]
    public List<AudioClip> drumSamples;
    public List<AudioClip> bassSamples;
    public List<AudioClip> leadSamples;
    public List<AudioClip> ambientSamples; // 새로 추가된 Ambient
    public List<AudioClip> fxSamples;

    [Header("--- Settings ---")]
    [Range(0f, 1f)] public float masterVolume = 0.8f;
    public double bpm = 130.0;
    public bool playOnStart = true;

    [Header("--- Variation Settings ---")]
    public bool useVariation = true; // 변주 사용 여부
    private double nextVariationTime;
    private double fourBarsDuration; // 4마디의 길이(초)

    // 내부 관리용
    private Dictionary<string, AudioSource> audioSources = new Dictionary<string, AudioSource>();
    
    // 루프되는 레이어들 (Ambient 추가됨)
    private string[] loopLayers = { "Drum", "Bass", "Lead", "Ambient" }; 
    private string fxLayer = "Fx";

    void Awake()
    {
        // 중복 방지
        foreach(Transform child in transform) Destroy(child.gameObject);

        // 1. 루프 레이어 생성 (Drum, Bass, Lead, Ambient)
        foreach (string layer in loopLayers)
        {
            CreateAudioSource(layer, true);
        }

        // 2. FX 레이어 생성 (One Shot용)
        CreateAudioSource(fxLayer, false);

        // 3. 4마디 시간 계산: (60초 / BPM) * 4박자 * 4마디
        fourBarsDuration = (60.0 / bpm) * 4.0 * 4.0;
    }

    void CreateAudioSource(string name, bool isLoop)
    {
        GameObject childObj = new GameObject("Layer_" + name);
        childObj.transform.SetParent(this.transform);
        
        AudioSource source = childObj.AddComponent<AudioSource>();
        source.loop = isLoop;
        source.playOnAwake = false;
        
        audioSources.Add(name, source);
    }

    void Start()
    {
        if (playOnStart)
        {
            PlayRandomMix();
        }
    }

    void Update()
    {
        // 1. 볼륨 동기화
        foreach (var source in audioSources.Values)
        {
            source.volume = masterVolume;
        }

        // 2. FX 클릭 재생 (좌클릭 시 One Shot)
        if (Input.GetMouseButtonDown(0))
        {
            PlayFxOneShot();
        }

        // 3. 자동 변주 (4마디마다 실행)
        if (useVariation && AudioSettings.dspTime >= nextVariationTime)
        {
            ToggleRandomLayer();
            nextVariationTime += fourBarsDuration; // 다음 4마디 뒤로 예약
        }

        // 4. 리셋 테스트 (스페이스바)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayRandomMix();
        }
    }

    // 변주 함수: 루프 레이어 중 하나를 랜덤으로 Mute/Unmute
    void ToggleRandomLayer()
    {
        // 루프 레이어 중 하나 선택
        string targetLayer = loopLayers[Random.Range(0, loopLayers.Length)];
        AudioSource source = audioSources[targetLayer];

        // Mute 상태를 반대로 뒤집음 (켜져있으면 끄고, 꺼져있으면 킴)
        source.mute = !source.mute;

        // 로그로 확인 (선택 사항)
        // Debug.Log($"[Variation] {targetLayer} is now {(source.mute ? "Muted" : "Unmuted")}");
    }

    void PlayFxOneShot()
    {
        AudioSource fxSource = audioSources[fxLayer];
        if (fxSource.clip != null)
        {
            fxSource.PlayOneShot(fxSource.clip, masterVolume);
        }
    }

    public void PlayRandomMix()
    {
        // 모든 소리 멈춤
        foreach(var src in audioSources.Values) {
            src.Stop();
            src.mute = false; // 리셋 시 모든 트랙 다시 켜기
        }

        // 샘플 랜덤 할당
        AssignRandomClip("Drum", drumSamples);
        AssignRandomClip("Bass", bassSamples);
        AssignRandomClip("Lead", leadSamples);
        AssignRandomClip("Ambient", ambientSamples);
        AssignRandomClip("Fx", fxSamples);

        // 시작 시간 동기화 (DSP Time)
        double startTime = AudioSettings.dspTime + 0.1;
        nextVariationTime = startTime + fourBarsDuration; // 첫 변주는 4마디 뒤에 시작

        // 루프 레이어 재생
        foreach (var layer in loopLayers)
        {
            AudioSource source = audioSources[layer];
            if (source.clip != null)
            {
                source.PlayScheduled(startTime);
            }
        }
    }

    private void AssignRandomClip(string layerName, List<AudioClip> clips)
    {
        if (clips != null && clips.Count > 0)
        {
            audioSources[layerName].clip = clips[Random.Range(0, clips.Count)];
        }
    }
}