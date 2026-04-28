using UnityEngine;
using System.Collections;

public class LightningVisual : MonoBehaviour
{
    public static void Spawn(Vector3 start, Vector3 end)
    {
        GameObject obj = new GameObject("LightningVisual");
        LightningVisual lv = obj.AddComponent<LightningVisual>();
        lv.StartCoroutine(lv.Run(start, end));
    }

    IEnumerator Run(Vector3 start, Vector3 end)
    {
        Vector3 dir = end - start;
        Vector3 perp = new Vector3(-dir.y, dir.x, 0f).normalized;

        // È­¸é ¹øÂ½ + Å¸ÀÏ¸Ê ¹à°Ô
        ElementalManager em = Object.FindObjectOfType<ElementalManager>();
        if (em != null) em.TriggerLightningFlash();

        // FireVignette »ö»ó + HDR·Î ¹à°Ô
        LineRenderer aura = MakeLine(new Color(4.0f, 2.5f, 0.0f, 1f), 0.4f, 0.2f);   // ÁÖÈ²
        LineRenderer glow = MakeLine(new Color(4.0f, 3.5f, 0.2f, 1f), 0.18f, 0.09f);  // ³ë¶û
        LineRenderer core = MakeLine(new Color(5.0f, 5.0f, 2.0f, 1f), 0.06f, 0.03f);  // Èò/³ë¶û 




        SetPositions(aura, start, end, perp, 20, 0.45f);
        SetPositions(glow, start, end, perp, 20, 0.42f);
        SetPositions(core, start, end, perp, 20, 0.38f);
        aura.startWidth = 0.6f; aura.endWidth = 0.3f;
        glow.startWidth = 0.25f; glow.endWidth = 0.12f;
        core.startWidth = 0.08f; core.endWidth = 0.04f;
        SetAlpha(aura, 1f);
        SetAlpha(glow, 1f);
        SetAlpha(core, 1f);

        yield return new WaitForSeconds(0.04f);

        float lifetime = 0.28f;
        float elapsed = 0f;
        float step = 0.02f;

        while (elapsed < lifetime)
        {
            float fade = Mathf.Lerp(1f, 0f, Mathf.Pow(elapsed / lifetime, 0.55f));
            float flicker = Random.Range(0.7f, 1.3f);
            float f = fade * flicker;

            SetPositions(aura, start, end, perp, 20, 0.35f);
            SetPositions(glow, start, end, perp, 20, 0.32f);
            SetPositions(core, start, end, perp, 20, 0.28f);

            aura.startWidth = 0.5f * f; aura.endWidth = 0.25f * f;
            glow.startWidth = 0.22f * f; glow.endWidth = 0.11f * f;
            core.startWidth = 0.07f * f; core.endWidth = 0.03f * f;

            SetAlpha(aura, f * 0.7f);
            SetAlpha(glow, f * 0.9f);
            SetAlpha(core, f);

            elapsed += step;
            yield return new WaitForSeconds(step);
        }

        Destroy(gameObject);
    }

    LineRenderer MakeLine(Color col, float startW, float endW)
    {
        GameObject obj = new GameObject();
        obj.transform.SetParent(transform);
        obj.transform.localPosition = new Vector3(0, 0, -2f);

        LineRenderer lr = obj.AddComponent<LineRenderer>();
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_ZWrite", 0);
        mat.color = Color.white;

        lr.material = mat;
        lr.startColor = col;
        lr.endColor = col;
        lr.startWidth = startW;
        lr.endWidth = endW;
        lr.positionCount = 20;
        lr.useWorldSpace = true;
        lr.numCapVertices = 6;
        lr.numCornerVertices = 6;
        lr.sortingLayerName = "Overhead";
        lr.sortingOrder = 32001;
        return lr;
    }

    void SetPositions(LineRenderer lr, Vector3 start, Vector3 end, Vector3 perp, int count, float disp)
    {
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / (count - 1);
            Vector3 pos = Vector3.Lerp(start, end, t);
            float mid = Mathf.Sin(t * Mathf.PI);
            float off = Random.Range(-disp, disp) * mid;
            float hf = Random.Range(-disp * 0.25f, disp * 0.25f);
            lr.SetPosition(i, pos + perp * (off + hf));
        }
    }

    void SetAlpha(LineRenderer lr, float alpha)
    {
        Color sc = lr.startColor; sc.a = alpha; lr.startColor = sc;
        Color ec = lr.endColor; ec.a = alpha; lr.endColor = ec;
    }
}