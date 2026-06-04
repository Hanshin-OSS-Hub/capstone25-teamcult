using UnityEngine;
using System.Collections.Generic;

public enum SFXType 
{ 
    Gold, ChestOpen, ElevatorOpen, ElevatorClose, 
    DoorOpen, DoorClose, PlayerHit, PlayerDeath, 
    PlayerAttack_1, PlayerAttack_2, PlayerAttack_3, PlayerReload_3,
    EnemyHit, EnemyDeath, BossGreeting, BossAttack, BossHit, BossDeath,
    EnemyEncounter, StageClear, ItemEquip, GameOver, 
    StateChange_Tension, StateChange_Combat,
    HeartObtain,
    HeartObtain_Fire, HeartObtain_Ice, HeartObtain_Lightning,
    Hit_Fire, Hit_Ice, Hit_Lightning,
    EnemyHit_Slime, EnemyAttack_Mage, Trap_Electric
}

[System.Serializable]
public struct SFXData
{
    public SFXType type;
    public AudioClip clip;
    [Range(0f, 2f)] public float volume; // 소리를 더 키울 수 있도록 1.0 -> 2.0으로 확장
}

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    [Header("효과음 리스트")]
    public List<SFXData> sfxList = new List<SFXData>();

    private AudioSource[] sfxSources;
    private Dictionary<SFXType, SFXData> sfxDictionary;
    private int lastFrame = -1;
    private int playCountThisFrame = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        sfxDictionary = new Dictionary<SFXType, SFXData>();
        
        foreach (var sfx in sfxList)
        {
            if (!sfxDictionary.ContainsKey(sfx.type) && sfx.clip != null)
            {
                sfxDictionary.Add(sfx.type, sfx);

                if (sfx.clip.loadState == AudioDataLoadState.Unloaded)
                {
                    sfx.clip.LoadAudioData();
                }
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
        //if (lastFrame != Time.frameCount)
        //{
        //    if (playCountThisFrame > 0)
        //    {
        //        Debug.Log($"[SFX] Frame {lastFrame}: PlaySFX called {playCountThisFrame} times");
        //    }

        //    lastFrame = Time.frameCount;
        //    playCountThisFrame = 0;
        //}

        playCountThisFrame++;

        if (sfxDictionary.TryGetValue(type, out SFXData data))
        {
            foreach (var source in sfxSources)
            {
                if (!source.isPlaying)
                {
                    //Debug.Log(
                    //    $"[SFX LOAD CHECK] Frame {Time.frameCount}: type={type}, clip={data.clip.name}, " +
                    //    $"loadState={data.clip.loadState}, preload={data.clip.preloadAudioData}"
                    //);
                    source.PlayOneShot(data.clip, data.volume);
                    return;
                }
            }
        }
    }

    // =========================================================
    // ★ [볼륨 자동 평준화 도구] 에디터에서 우클릭으로 실행!
    // =========================================================
    [ContextMenu("오디오 자동 분석 및 볼륨 맞춤")]
    public void AutoNormalizeVolumes()
    {
        float targetPeak = 0.6f; // 모든 효과음이 맞출 목표 기준 볼륨

        for (int i = 0; i < sfxList.Count; i++)
        {
            SFXData sfx = sfxList[i];
            if (sfx.clip == null) continue;

            try
            {
                // 오디오 클립의 파형 데이터를 배열로 추출
                float[] samples = new float[sfx.clip.samples * sfx.clip.channels];
                sfx.clip.GetData(samples, 0);

                // 음원 내에서 가장 시끄러운 부분(Peak) 찾기
                float maxPeak = 0f;
                foreach (float sample in samples)
                {
                    float abs = Mathf.Abs(sample);
                    if (abs > maxPeak) maxPeak = abs;
                }

                // 피크 수치에 비례하여 볼륨을 자동으로 계산
                if (maxPeak > 0)
                {
                    // 너무 튀는 소리는 줄이고, 작은 소리는 증폭시킴
                    sfx.volume = Mathf.Clamp(targetPeak / maxPeak, 0.1f, 2.0f);
                }
                
                sfxList[i] = sfx; // 리스트에 계산된 볼륨 적용
            }
            catch
            {
                Debug.LogWarning($"[분석 실패] {sfx.clip.name} 파일은 압축 방식 때문에 분석할 수 없습니다. 수동으로 조절해 주세요.");
            }
        }
        Debug.Log("<color=green>모든 효과음의 파형 분석 및 볼륨 평준화가 완료되었습니다!</color>");
    }
}
