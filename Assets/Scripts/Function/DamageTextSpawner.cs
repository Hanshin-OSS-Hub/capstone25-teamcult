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

        Vector3 spawnPos = position + new Vector3(Random.Range(-0.2f, 0.2f), 0.5f, 0f);
        GameObject obj = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);
        DamageText dt = obj.GetComponent<DamageText>();
        if (dt != null)
        {
            Color color = Color.white;
            if (isCrit) color = new Color(1f, 0.8f, 0f);   
            if (isBurn) color = new Color(1f, 0.4f, 0.1f); 
            dt.Setup(damage, color);
        }
    }

    public void SpawnMiss(Vector3 position)
    {
        if (damageTextPrefab == null) return;

        Vector3 spawnPos = position + new Vector3(Random.Range(-0.2f, 0.2f), 0.5f, 0f);
        GameObject obj = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);
        DamageText dt = obj.GetComponent<DamageText>();
        if (dt != null)
            dt.SetupText("MISS", new Color(0.7f, 0.7f, 0.7f)); 
    }
}