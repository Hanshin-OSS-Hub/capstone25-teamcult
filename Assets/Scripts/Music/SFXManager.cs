using UnityEngine;
using System.Collections.Generic;

// ★ EnemyEncounter, StageClear, HeartObtain 추가
public enum SFXType 
{ 
    Gold, ChestOpen, ElevatorOpen, ElevatorClose, 
    DoorOpen, DoorClose, PlayerHit, PlayerDeath, PlayerAttack, EnemyHit, EnemyDeath,
    BossGreeting, BossAttack, BossHit, BossDeath,
    EnemyEncounter, StageClear, HeartObtain 
}

[System.Serializable]
public struct SFXData
{
    public SFXType type;
    public AudioClip clip;
}

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    [Header("효과음 리스트 (여기서 파일들을 매핑하세요)")]
    public List<SFXData> sfxList = new List<SFXData>();

    private AudioSource[] sfxSources;
    private Dictionary<SFXType, AudioClip> sfxDictionary;

    void Awake()
    {
        Instance = this;
        sfxDictionary = new Dictionary<SFXType, AudioClip>();

        foreach (var sfx in sfxList)
        {
            if (!sfxDictionary.ContainsKey(sfx.type) && sfx.clip != null)
            {
                sfxDictionary.Add(sfx.type, sfx.clip);
            }
        }

        sfxSources = new AudioSource[5];
        for (int i = 0; i < sfxSources.Length; i++)
        {
            sfxSources[i] = gameObject.AddComponent<AudioSource>();
            sfxSources[i].loop = false;
            sfxSources[i].playOnAwake = false;
        }
    }

    public void PlaySFX(SFXType type)
    {
        if (sfxDictionary.TryGetValue(type, out AudioClip clip))
        {
            foreach (var source in sfxSources)
            {
                if (!source.isPlaying)
                {
                    source.PlayOneShot(clip);
                    return;
                }
            }
            sfxSources[0].PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"[SFX 매니저] {type} 효과음이 인스펙터에 할당되지 않았습니다!");
        }
    }
}