using UnityEngine;
using System.Collections;

public class BurnEffect : MonoBehaviour
{
    public float damage = 1f;
    public float tickInterval = 0.5f;
    public float duration = 3f;

    private EnemyHealth enemyHealth;

    void Start()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        if (enemyHealth == null)
        {
            Destroy(this);
            return;
        }
        StartCoroutine(BurnTick());
    }

    IEnumerator BurnTick()
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage((int)damage);

                // ? 화상 데미지 주황색으로 표시
                if (enemyHealth.damageTextPrefab != null)
                {
                    Vector3 spawnPos = enemyHealth.transform.position + new Vector3(Random.Range(-0.3f, 0.3f), 1.5f, 0f);
                    GameObject textObj = Instantiate(enemyHealth.damageTextPrefab, spawnPos, Quaternion.identity);
                    DamageText dt = textObj.GetComponent<DamageText>();
                    if (dt != null) dt.Setup((int)damage, new Color(1f, 0.4f, 0.1f));
                }

                Debug.Log($"[화상] {damage} 데미지 / 남은시간: {duration - elapsed:F1}초");
            }
        }
        Debug.Log("[화상] 종료");
        Destroy(this);
    }
}