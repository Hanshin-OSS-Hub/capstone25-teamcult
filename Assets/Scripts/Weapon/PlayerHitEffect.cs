using UnityEngine;
using System.Collections;

public class PlayerHitEffect : MonoBehaviour
{
    [Header("Settings")]
    public int flashCount = 3;         // �� �� ��������
    public float flashInterval = 0.1f; // �����̴� �ӵ�

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void TakeDamage()
    {
        if (MusicDirector.Instance != null)
        {
            MusicDirector.Instance.OnPlayerDamaged();
        }
        
        
        // ��������Ʈ �������� ���ų� �̹� �����̴� ���̸� ���� �� ��
        if (spriteRenderer == null) return;

        // �ڷ�ƾ(������ ����) ����
        StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        for (int i = 0; i < flashCount; i++)
        {
            // 1. ���� (����)
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(flashInterval);

            // 2. �Ѱ� (���󺹱�)
            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(flashInterval);
        }

        // ������ Ȯ���ϰ� �ѵα�
        spriteRenderer.enabled = true;
    }
}