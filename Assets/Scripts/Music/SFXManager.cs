using UnityEngine;
using System.Collections.Generic;

public enum SFXType 
{ 
    Gold, ChestOpen, ElevatorOpen, ElevatorClose, 
    DoorOpen, DoorClose, PlayerHit, PlayerDeath, 
    // ★ 무기별 공격음 및 재장전음 추가
    PlayerAttack_1, PlayerAttack_2, PlayerAttack_3, PlayerReload_3,
    EnemyHit, EnemyDeath, BossGreeting, BossAttack, BossHit, BossDeath,
    EnemyEncounter, StageClear, HeartObtain 
}

[System.Serializable]
public struct SFXData
{
    public SFXType type;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume; // ★ 개별 볼륨 설정을 위한 슬라이더
}

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    [Header("효과음 리스트")]
    public List<SFXData> sfxList = new List<SFXData>();

    private AudioSource[] sfxSources;
    private Dictionary<SFXType, SFXData> sfxDictionary; // ★ 클립 대신 데이터 전체를 저장

    void Awake() { Instance = this; }

    void Start()
    {
        sfxDictionary = new Dictionary<SFXType, SFXData>();
        foreach (var sfx in sfxList)
        {
            if (!sfxDictionary.ContainsKey(sfx.type) && sfx.clip != null)
            {
                sfxDictionary.Add(sfx.type, sfx);
            }
        }

        sfxSources = new AudioSource[10]; // 스피커 개수를 조금 더 늘렸습니다.
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
            foreach (var source in sfxSources)
            {
                if (!source.isPlaying)
                {
                    // ★ 재생 시 설정된 개별 볼륨(data.volume)을 적용합니다.
                    source.PlayOneShot(data.clip, data.volume);
                    return;
                }
            }
        }
    }
}