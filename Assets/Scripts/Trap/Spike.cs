using UnityEngine;

public class Spike : MonoBehaviour
{
    // todo : 코루틴 이용해서 계속 가시위에 있으면 계속 대미지?
    [SerializeField] int damage = 1;
    [Tooltip("1 : 한번만 작동. -1 : 계속 작동")]
    [SerializeField] int use = 1;
    private BoxCollider2D trapCollider;
    private SpriteRenderer trapRenderer;
    private TrapType trapType;
    public void Init(TrapType type) {
        trapType = type;
        if (type == TrapType.Once) { use = 1; }
        else if (type == TrapType.Repeat) { use = -1; }
        else if (type == TrapType.Forever) { use = -1; }
    }
    void OnTriggerEnter2D(Collider2D other) { // 총알 스크립트 복사, 가시에 닿았을때
        // 플레이어 태그 확인
        if (other.CompareTag("Player")) {
            if (use == 0) { return; }
            // ★ 중요: 맞은 부위(팔, 무기 등)의 부모님(몸통)에게서 스크립트를 찾습니다.
            // 이게 있어야 충돌이 씹히지 않습니다.
            PlayerHealth player = other.GetComponentInParent<PlayerHealth>();

            if (player != null) {
                player.TakeDamage(damage);
                Debug.Log("가시 대미지! 플레이어 체력 감소");
            }

            // 맞았으면 사용 표시
            // 2번 밟았을때 꺼지게 하는 용도, use가 음수면 계속 작동
            --use;
            if (trapType == TrapType.Once) { Off(); }
        }
    }

    private void Awake() {
        // 컴포넌트 미리 가져오기 (성능 최적화)
        trapCollider = GetComponent<BoxCollider2D>();
        trapRenderer = GetComponent<SpriteRenderer>();
    }

    // 함정 활성화 (가시가 튀어나옴)
    public void On() {
        if (trapCollider != null) trapCollider.enabled = true;
        if (trapRenderer != null) trapRenderer.enabled = true;

        // 필요하다면 여기서 애니메이션이나 소리 재생 가능
        Debug.Log("함정 ON: 공격 가능 상태");
    }

    // 함정 비활성화 (가시가 들어감)
    public void Off() {
        if (trapCollider != null) trapCollider.enabled = false;
        if (trapRenderer != null) trapRenderer.enabled = false;

        Debug.Log("함정 OFF: 안전한 상태");
    }
}
