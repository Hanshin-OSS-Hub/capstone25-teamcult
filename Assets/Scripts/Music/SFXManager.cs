using UnityEngine;
using System.Collections.Generic;

public enum SFXType 
{ 
    Gold, ChestOpen, ElevatorOpen, ElevatorClose, 
    DoorOpen, DoorClose, PlayerHit, PlayerDeath, 
    PlayerAttack_1, PlayerAttack_2, PlayerAttack_3, PlayerReload_3,
    EnemyHit, EnemyDeath, BossGreeting, BossAttack, BossHit, BossDeath,
    EnemyEncounter, StageClear, HeartObtain 
}

[System.Serializable]
public struct SFXData
{
    public SFXType type;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume;
}

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    [Header("효과음 리스트")]
    public List<SFXData> sfxList = new List<SFXData>();

    private AudioSource[] sfxSources;
    private Dictionary<SFXType, SFXData> sfxDictionary;

    void Awake()
    {
        Instance = this;
        
        // ★ [수정] 씬이 시작되자마자 딕셔너리를 미리 채워둡니다.
        sfxDictionary = new Dictionary<SFXType, SFXData>();
        foreach (var sfx in sfxList)
        {
            if (!sfxDictionary.ContainsKey(sfx.type) && sfx.clip != null)
            {
                sfxDictionary.Add(sfx.type, sfx);
            }
        }

        sfxSources = new AudioSource[10];
        for (int i = 0; i < sfxSources.Length; i++)
        {
            sfxSources[i] = gameObject.AddComponent<AudioSource>();
            sfxSources[i].playOnAwake = false;
        }
    }

    public void PlaySFX(SFXType type)
    {
        if (sfxDictionary.TryGetValue(type, out SFXData data))
        {
            // ★ [디버그 로그] 콘솔창에 "총소리(타입명) : 발사음(파일네임)"이 찍힙니다.
            Debug.Log($"<color=yellow>[SFX 재생]</color> 요청타입: {type}, 재생파일: {data.clip.name}");

            foreach (var source in sfxSources)
            {
                if (!source.isPlaying)
                {
                    source.PlayOneShot(data.clip, data.volume);
                    return;
                }
            }
        }
        else
        {
            Debug.LogWarning($"[SFX 오류] {type}에 해당하는 설정이 딕셔너리에 없습니다!");
        }
    }
}