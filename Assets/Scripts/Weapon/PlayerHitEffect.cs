using UnityEngine;
using System.Collections;

public class PlayerHitEffect : MonoBehaviour
{
    [Header("Settings")]
    public int flashCount = 3;         
    public float flashInterval = 0.1f; 

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void TakeDamage()
    {
        if (BattleStateBGM.Instance != null)
        {
            BattleStateBGM.Instance.TriggerGlitch();
        }
        
        if (spriteRenderer == null) return;
        StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        for (int i = 0; i < flashCount; i++)
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(flashInterval);
            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(flashInterval);
        }
        spriteRenderer.enabled = true;
    }
}