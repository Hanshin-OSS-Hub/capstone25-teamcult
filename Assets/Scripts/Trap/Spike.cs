using UnityEngine;

public class Spike : MonoBehaviour
{
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

    void OnTriggerEnter2D(Collider2D other) { 
        if (other.CompareTag("Player")) {
            if (use == 0) { return; }
            
            PlayerHealth player = other.GetComponentInParent<PlayerHealth>();

            if (player != null) {
                player.TakeDamage(damage);
                
                // =========================================================
                // ★ 전기 함정 발동 사운드 추가
                // =========================================================
                if (SFXManager.Instance != null) {
                    SFXManager.Instance.PlaySFX(SFXType.Trap_Electric);
                }
                
                Debug.Log("함정 발동! 플레이어 체력 감소");
            }

            --use;
            if (trapType == TrapType.Once) { Off(); }
        }
    }

    private void Awake() {
        trapCollider = GetComponent<BoxCollider2D>();
        trapRenderer = GetComponent<SpriteRenderer>();
    }

    public void On() {
        if (trapCollider != null) trapCollider.enabled = true;
        if (trapRenderer != null) trapRenderer.enabled = true;
    }

    public void Off() {
        if (trapCollider != null) trapCollider.enabled = false;
        if (trapRenderer != null) trapRenderer.enabled = false;
    }
}