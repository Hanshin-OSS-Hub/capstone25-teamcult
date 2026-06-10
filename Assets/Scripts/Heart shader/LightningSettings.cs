using UnityEngine;

public class LightningSettings : MonoBehaviour
{
    [Header("번개 색상 (HDR 값 높일수록 밝아짐)")]
    public Color auraColor = new Color(0f, 0.5f, 2.0f, 1f);
    public Color glowColor = new Color(0.3f, 0.9f, 2.0f, 1f);
    public Color coreColor = new Color(4.0f, 4.0f, 6.0f, 1f);

    [Header("번개 굵기")]
    public float auraWidth = 1.4f;
    public float glowWidth = 0.65f;
    public float coreWidth = 0.18f;

    [Header("번개 구불구불함 (높을수록 더 구불)")]
    [Range(0f, 1f)]
    public float displacement = 0.35f;

    [Header("깜빡임 (0=없음, 0.5=많음)")]
    [Range(0f, 0.5f)]
    public float flickerAmount = 0.3f;

    [Header("지속 시간 (초)")]
    public float lifetime = 0.28f;

    [Header("Bloom 강도 (번개 칠 때만 올라감)")]
    public float bloomIntensity = 8f;

    [Header("광원 설정")]
    public bool useLights = true;
    public Color lightColor = new Color(0.3f, 0.7f, 1f);
    public float lightIntensity = 8f;
    public float lightRadius = 4f;
    public string lightSortingLayer = "Overhead"; 
}