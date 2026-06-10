using UnityEngine;
using System.Collections.Generic;

public class SimpleBgmLoop : MonoBehaviour
{
    [Header("드래그 앤 드롭으로 샘플 채우기")]
    public List<AudioClip> drumSamples;
    public List<AudioClip> bassSamples;
    public List<AudioClip> leadSamples;
    public List<AudioClip> fxSamples;

    [Header("설정")]
    [Range(0f, 1f)] public float masterVolume = 0.8f;
    public bool playOnStart = true;

    private Dictionary<string, AudioSource> audioSources = new Dictionary<string, AudioSource>();
    private string[] layerNames = { "Drum", "Bass", "Lead", "Fx" };

    void Awake()
    {
        foreach (string layer in layerNames)
        {
            GameObject childObj = new GameObject(layer + "_Source");
            childObj.transform.SetParent(this.transform);
            
            AudioSource source = childObj.AddComponent<AudioSource>();
            source.loop = true;
            source.playOnAwake = false;
            
            audioSources.Add(layer, source);
        }
    }

    void Start()
    {
        if (playOnStart)
        {
            PlayNewMix();
        }
    }

    void Update()
    {
        foreach (var source in audioSources.Values)
        {
            source.volume = masterVolume;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayNewMix();
        }
    }

    public void PlayNewMix()
    {
        StopAll();

        SetClip("Drum", drumSamples);
        SetClip("Bass", bassSamples);
        SetClip("Lead", leadSamples);
        SetClip("Fx", fxSamples);

        double startTime = AudioSettings.dspTime + 0.1;
        
        foreach (var source in audioSources.Values)
        {
            if (source.clip != null)
            {
                source.PlayScheduled(startTime);
            }
        }
    }

    private void SetClip(string layer, List<AudioClip> clips)
    {
        if (clips != null && clips.Count > 0)
        {
            audioSources[layer].clip = clips[Random.Range(0, clips.Count)];
        }
    }

    private void StopAll()
    {
        foreach (var source in audioSources.Values)
        {
            source.Stop();
        }
    }
}