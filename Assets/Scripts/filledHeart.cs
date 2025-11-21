using UnityEngine;
using UnityEngine.UI;

public enum HeartAttribute {
    Normal,
    Fire,
    Ice,
    Poison,
    Electric
}

public class filledHeart : MonoBehaviour {

    // 💡 HP는 public 필드로 유지됩니다.
    [Header("Heart Stats")]
    public int HP;

    private Image heartImage;

    [Header("Attribute Settings")]
    // 💡 현재 하트의 속성: private [SerializeField]로 유지
    [SerializeField] private HeartAttribute currentAttribute = HeartAttribute.Normal;

    // 속성별 색상 설정 (밝은 톤)
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color fireColor = new Color(1f, 0.5f, 0.5f);
    [SerializeField] private Color iceColor = new Color(0.5f, 0.5f, 1f);
    [SerializeField] private Color poisonColor = new Color(0.5f, 1f, 0.5f);
    [SerializeField] private Color electricColor = new Color(0.5f, 1f, 1f);


    void Awake() {
        heartImage = GetComponent<Image>();

        if (heartImage == null) {
            Debug.LogError($"FilledHeart 오브젝트({gameObject.name})에 Image 컴포넌트가 없습니다. UI 객체인지 확인하세요.");
        }

        // 게임 시작 시 색상을 적용
        UpdateColorByAttribute();
    }

    // 💡 에디터에서만 호출되는 특수 메서드
    private void OnValidate() {
        // 💡 1. Image 컴포넌트 참조가 아직 없으면 미리 가져옵니다.
        // OnValidate는 Awake보다 먼저 호출될 수 있으므로 Null 체크 후 GetComponent를 사용합니다.
        if (heartImage == null) {
            heartImage = GetComponent<Image>();
        }

        // 💡 2. 인스펙터에서 currentAttribute 값이 변경될 때마다 색상을 즉시 업데이트합니다.
        // 이 코드가 없으면, 인스펙터에서 값을 바꿔도 Play 버튼을 눌러야 적용됩니다.
        UpdateColorByAttribute();
    }


    /// <summary>
    /// 외부에서 속성을 변경하고 색상을 업데이트하는 Public 함수
    /// </summary>
    public void SetAttribute(HeartAttribute newAttribute) {
        currentAttribute = newAttribute;
        UpdateColorByAttribute();
    }


    /// <summary>
    /// 현재 속성(currentAttribute)에 따라 하트 이미지의 색상을 변경하는 핵심 메서드
    /// </summary>
    private void UpdateColorByAttribute() {
        if (heartImage == null) return;

        Color targetColor;

        switch (currentAttribute) {
            case HeartAttribute.Normal:
                targetColor = normalColor;
                break;
            case HeartAttribute.Fire:
                targetColor = fireColor;
                break;
            case HeartAttribute.Ice:
                targetColor = iceColor;
                break;
            case HeartAttribute.Poison:
                targetColor = poisonColor;
                break;
            case HeartAttribute.Electric:
                targetColor = electricColor;
                break;
            default:
                targetColor = normalColor;
                break;
        }

        heartImage.color = targetColor;
    }
}