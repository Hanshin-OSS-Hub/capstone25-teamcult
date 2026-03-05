using UnityEngine;
using TMPro; // TextMeshPro 사용

public class DamageText : MonoBehaviour
{
    public float moveSpeed = 2f;      // 위로 올라가는 속도
    public float destroyTime = 1f;    // 사라지기까지 걸리는 시간

    private TextMeshPro textMesh;
    private Color textColor;

    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    void Start()
    {
        textColor = textMesh.color;
        // 지정된 시간 뒤에 이 오브젝트를 파괴합니다.
        Destroy(gameObject, destroyTime);
    }

    void Update()
    {
        // 1. 위로 스르륵 이동
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);

        // 2. 서서히 투명해지게 만들기 (Fade Out)
        textColor.a = Mathf.Lerp(textColor.a, 0, Time.deltaTime * 3f);
        textMesh.color = textColor;
    }

    // 데미지 숫자를 세팅해주는 함수 (적 스크립트에서 부를 겁니다)
    public void Setup(int damageAmount)
    {
        textMesh.text = damageAmount.ToString();
    }
}