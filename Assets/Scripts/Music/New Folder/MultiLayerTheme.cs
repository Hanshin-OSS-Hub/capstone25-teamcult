using UnityEngine;

[CreateAssetMenu(fileName = "NewTheme", menuName = "Audio/Multi-Layer Theme")]
public class MultiLayerTheme : ScriptableObject
{
    public string themeName; // 예: "DeepHouse_Zone", "DarkFantasy_Zone"

    [Header("Layer Loops (Index: 0=Explore, 1=Tension, 2=Combat)")]
    public AudioClip[] drums;  // 단계별 드럼 루프
    public AudioClip[] basses; // 단계별 베이스 루프
    public AudioClip[] leads;  // 단계별 리드 루프
    
    [Header("FX Sounds")]
    public AudioClip backgroundFX; // 깔아두는 배경음 (Loop)
    public AudioClip attackFX;     // 공격 시 재생할 효과음 (OneShot)
}