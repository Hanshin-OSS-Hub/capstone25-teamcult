using UnityEngine;

[CreateAssetMenu(fileName = "HeartType_", menuName = "Game/Heart Type Definition")]
public class HeartTypeDefinition : ScriptableObject
{
    public HeartType type;
    public Sprite heartSprite;
    public Material heartMaterial;
    public string heartName = "새 하트";
    public string description = "설명을 입력하세요.";
}