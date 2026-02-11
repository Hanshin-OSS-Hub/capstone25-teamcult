using UnityEngine;

public enum EnemySoundType
{
    Normal,
    Heavy,
    Speed,
    Weird
}

public class EnemyAudioProfile : MonoBehaviour
{
    public EnemySoundType type = EnemySoundType.Normal;
}