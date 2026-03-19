using UnityEngine;

public class DamageTextSpawner : MonoBehaviour
{
    public static DamageTextSpawner Instance;

    public GameObject damageTextPrefab;

    void Awake()
    {
        Instance = this;
    }

    public void Spawn(int damage, Vector3 position, bool isCrit = false, bool isBurn = false)
    {
        if (damageTextPrefab == null) return;

        // 살짝 랜덤 위치
        Vector3 spawnPos = position + new Vector3(Random.Range(-0.2f, 0.2f), 0.5f, 0f);
        GameObject obj = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);

        DamageText dt = obj.GetComponent<DamageText>();
        if (dt != null)
        {
            Color color = Color.white;
            if (isCrit) color = new Color(1f, 0.8f, 0f);   // 치명타 → 노란색
            if (isBurn) color = new Color(1f, 0.4f, 0.1f); // 화상 → 주황색
            dt.Setup(damage, color);
        }
    }
}