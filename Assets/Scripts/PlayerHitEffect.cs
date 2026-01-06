using UnityEngine;
using System.Collections;

public class PlayerHitEffect : MonoBehaviour
{
    [Header("Settings")]
    public int flashCount = 3;         // 몇 번 깜빡일지
    public float flashInterval = 0.1f; // 깜빡이는 속도

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void TakeDamage()
    {
        // 스프라이트 렌더러가 없거나 이미 깜빡이는 중이면 실행 안 함
        if (spriteRenderer == null) return;

        // 코루틴(깜빡임 로직) 시작
        StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        for (int i = 0; i < flashCount; i++)
        {
            // 1. 끄고 (투명)
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(flashInterval);

            // 2. 켜고 (원상복구)
            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(flashInterval);
        }

        // 끝나면 확실하게 켜두기
        spriteRenderer.enabled = true;
    }
}