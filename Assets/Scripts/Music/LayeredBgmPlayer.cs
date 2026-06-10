using UnityEngine;
using System.Collections.Generic;

public class LayeredBgmPlayer : MonoBehaviour
{
    [Header("--- Samples (BPM 130) ---")]
    public List<AudioClip> drumSamples;
    public List<AudioClip> bassSamples;
    public List<AudioClip> leadSamples;
    public List<AudioClip> ambientSamples; 
    public List<AudioClip> fxSamples;

    [Header("--- Settings ---")]
    [Range(0f, 1f)] public float masterVolume = 0.8f;
    public double bpm = 130.0;
    public bool playOnStart = true;

    [Header("--- Variation Settings ---")]
    public bool useVariation = true; // 변주 사용 여부
    private double nextVariationTime;
    private double fourBarsDuration; 

    private Dictionary<string, AudioSource> audioSources = new Dictionary<string, AudioSource>();
    
    private string[] loopLayers = { "Drum", "Bass", "Lead", "Ambient" }; 
    private string fxLayer = "Fx";

    void Awake()
    {
        foreach(Transform child in transform) Destroy(child.gameObject);

        foreach (string layer in loopLayers)
        {
            CreateAudioSource(layer, true);
        }

        CreateAudioSource(fxLayer, false);

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
        foreach (var source in audioSources.Values)
        {
            source.volume = masterVolume;
        }

        if (Input.GetMouseButtonDown(0))
        {
            PlayFxOneShot();
        }
        if (useVariation && AudioSettings.dspTime >= nextVariationTime)
        {
            ToggleRandomLayer();
            nextVariationTime += fourBarsDuration;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayRandomMix();
        }
    }

    void ToggleRandomLayer()
    {
        string targetLayer = loopLayers[Random.Range(0, loopLayers.Length)];
        AudioSource source = audioSources[targetLayer];

        source.mute = !source.mute;

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
        foreach(var src in audioSources.Values) {
            src.Stop();
            src.mute = false; 
        }

        AssignRandomClip("Drum", drumSamples);
        AssignRandomClip("Bass", bassSamples);
        AssignRandomClip("Lead", leadSamples);
        AssignRandomClip("Ambient", ambientSamples);
        AssignRandomClip("Fx", fxSamples);

        double startTime = AudioSettings.dspTime + 0.1;
        nextVariationTime = startTime + fourBarsDuration; 

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