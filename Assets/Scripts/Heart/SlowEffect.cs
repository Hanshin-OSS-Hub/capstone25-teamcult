using UnityEngine;
using System.Collections;

public class SlowEffect : MonoBehaviour
{
    public float slowPercent = 50f;
    public float duration = 2f;

    private EnemyStats enemyStats;
    private MeleeEnemy meleeEnemy;
    private RangedEnemy rangedEnemy;    // ? 추가
    private float originalSpeed;
    private bool applied = false;
    private GameObject iceCircle;
    private MeshRenderer iceMR;

    void Start()
    {
        enemyStats = GetComponent<EnemyStats>();
        meleeEnemy = GetComponent<MeleeEnemy>();
        rangedEnemy = GetComponent<RangedEnemy>(); // ? 추가

        if (enemyStats == null && meleeEnemy == null && rangedEnemy == null)
        {
            Destroy(this);
            return;
        }

        if (!applied)
        {
            applied = true;

            if (meleeEnemy != null)
            {
                originalSpeed = meleeEnemy.moveSpeed;
                meleeEnemy.moveSpeed *= (1f - slowPercent / 100f);
            }
            else if (rangedEnemy != null) // ? 추가
            {
                originalSpeed = rangedEnemy.moveSpeed;
                rangedEnemy.moveSpeed *= (1f - slowPercent / 100f);
            }
            else if (enemyStats != null)
            {
                originalSpeed = enemyStats.moveSpeed;
                enemyStats.moveSpeed *= (1f - slowPercent / 100f);
            }

            Debug.Log($"[슬로우] 이동속도 {slowPercent}% 감소");
            CreateIceCircle();
            StartCoroutine(RemoveSlow());
        }
    }

    void CreateIceCircle()
    {
        iceCircle = new GameObject("IceCircle");
        iceCircle.transform.SetParent(transform);
        iceCircle.transform.localPosition = new Vector3(0f, -0.5f, 0f);
        iceCircle.transform.localScale = new Vector3(2.0f, 0.8f, 1f);

        MeshFilter mf = iceCircle.AddComponent<MeshFilter>();
        MeshRenderer mr = iceCircle.AddComponent<MeshRenderer>();

        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3( 0.5f, -0.5f, 0),
            new Vector3(-0.5f,  0.5f, 0),
            new Vector3( 0.5f,  0.5f, 0)
        };
        mesh.triangles = new int[] { 0, 2, 1, 2, 3, 1 };
        mesh.uv = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        mf.mesh = mesh;

        Material mat = new Material(Shader.Find("Custom/FireVignette"));

        ElementalManager em = GetComponentInParent<ElementalManager>();
        if (em == null) em = FindFirstObjectByType<ElementalManager>();
        if (em != null && em.noiseTex != null)
            mat.SetTexture("_NoiseTex", em.noiseTex);

        mat.SetFloat("_EffectType", 1f);
        mat.SetColor("_CoreColor", new Color(0.8f, 0.97f, 1f, 0.9f));
        mat.SetColor("_EdgeColor", new Color(0.3f, 0.7f, 1f, 0.85f));
        mat.SetVector("_ScrollSpeed", new Vector4(0.02f, 0.05f, 0, 0));
        mat.SetFloat("_Radius", 0.85f);
        mat.SetFloat("_Softness", 0.08f);
        mat.SetFloat("_DistortPower", 0.35f);
        mat.SetFloat("_Progress", 0f);

        mr.material = mat;
        mr.sortingLayerName = "Player";
        mr.sortingOrder = 97;

        iceMR = mr;
        StartCoroutine(AnimateIceCircle());
    }

    IEnumerator AnimateIceCircle()
    {
        float elapsed = 0f;
        float introTime = 0.4f;
        while (elapsed < introTime)
        {
            elapsed += Time.deltaTime;
            float p = Mathf.Clamp01(elapsed / introTime);
            if (iceMR != null) iceMR.material.SetFloat("_Progress", p);
            yield return null;
        }

        while (iceCircle != null && applied)
        {
            float pulse = (Mathf.Sin(Time.time * 2f) + 1f) * 0.5f;
            float radius = Mathf.Lerp(0.82f, 0.88f, pulse);
            if (iceMR != null) iceMR.material.SetFloat("_Radius", radius);
            yield return null;
        }
    }

    IEnumerator RemoveSlow()
    {
        yield return new WaitForSeconds(duration);

        applied = false;

        if (meleeEnemy != null)
            meleeEnemy.moveSpeed = originalSpeed;
        else if (rangedEnemy != null) // ? 추가
            rangedEnemy.moveSpeed = originalSpeed;
        else if (enemyStats != null)
            enemyStats.moveSpeed = originalSpeed;

        Debug.Log("[슬로우] 해제");

        StartCoroutine(FadeOutCircle());
        yield return new WaitForSeconds(0.4f);
        Destroy(this);
    }

    IEnumerator FadeOutCircle()
    {
        float elapsed = 0f;
        float fadeTime = 0.4f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float p = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
            if (iceMR != null) iceMR.material.SetFloat("_Progress", p);
            yield return null;
        }
        if (iceCircle != null)
            Destroy(iceCircle);
    }

    void OnDestroy()
    {
        if (iceCircle != null)
            Destroy(iceCircle);
    }
}