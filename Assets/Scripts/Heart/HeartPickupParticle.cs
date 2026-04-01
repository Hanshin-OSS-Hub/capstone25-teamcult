using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HeartPickupParticle : MonoBehaviour
{
    [Header("파티클 설정")]
    public int particleCount = 25;
    public float lifetime = 1.5f;
    public float riseSpeed = 150f;
    public float spreadForce = 100f;

    public void Play(Vector3 worldPosition)
    {
        Canvas targetCanvas = FindHeartCanvas();
        if (targetCanvas == null)
        {
            Debug.LogWarning("Heart Canvas를 찾을 수 없습니다!");
            return;
        }
        StartCoroutine(SpawnParticles(worldPosition, targetCanvas));
    }

    Canvas FindHeartCanvas()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (var c in canvases)
        {
            if (c.name == "Heart Canvas") return c;
        }
        // 못찾으면 가장 높은 sortingOrder 캔버스 반환
        Canvas top = null;
        int maxOrder = -9999;
        foreach (var c in canvases)
        {
            if (c.sortingOrder > maxOrder)
            {
                maxOrder = c.sortingOrder;
                top = c;
            }
        }
        return top;
    }

    Vector2 WorldToCanvasPosition(Vector3 worldPos, Canvas canvas, Camera cam)
    {
        Vector2 screenPos = cam.WorldToScreenPoint(worldPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            screenPos, cam, out Vector2 localPos);
        return localPos;
    }

    IEnumerator SpawnParticles(Vector3 worldPos, Canvas canvas)
    {
        Camera cam = Camera.main;
        Vector2 canvasPos = WorldToCanvasPosition(worldPos, canvas, cam);

        for (int i = 0; i < particleCount; i++)
        {
            GameObject p = new GameObject("AshParticle");
            p.transform.SetParent(canvas.transform, false);
            p.transform.SetAsLastSibling(); // ScreenEffectPanel보다 위에 그려짐

            RectTransform rt = p.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = canvasPos + new Vector2(
                Random.Range(-20f, 20f),
                Random.Range(-20f, 20f));

            Image img = p.AddComponent<Image>();
            img.sprite = CreateAshSprite();

            float gray = Random.Range(0.05f, 0.25f);
            img.color = new Color(gray, gray, gray, 1f);

            float size = Random.Range(3f, 10f);
            rt.sizeDelta = new Vector2(size, size);

            Vector2 dir = new Vector2(
                Random.Range(-1f, 1f),
                Random.Range(0.2f, 1f)
            ).normalized;

            float speed = Random.Range(50f, spreadForce);
            float life = Random.Range(lifetime * 0.6f, lifetime);
            float rotSpeed = Random.Range(-180f, 180f);

            StartCoroutine(MoveAsh(p, img, rt, dir, speed, life, rotSpeed));
        }
        yield break;
    }

    IEnumerator MoveAsh(GameObject p, Image img, RectTransform rt,
        Vector2 dir, float speed, float life, float rotSpeed)
    {
        if (p == null) yield break;

        float timer = 0f;
        Color c = img.color;

        while (timer < life && p != null)
        {
            timer += Time.deltaTime;
            float t = timer / life;

            float currentSpeed = speed * (1f - t * 0.8f);
            rt.anchoredPosition += dir * currentSpeed * Time.deltaTime;
            rt.anchoredPosition += Vector2.up * riseSpeed * (1f - t) * Time.deltaTime;

            rt.Rotate(0f, 0f, rotSpeed * Time.deltaTime);

            c.a = Mathf.Lerp(1f, 0f, Mathf.Clamp01((t - 0.4f) * 1.7f));
            img.color = c;

            yield return null;
        }

        if (p != null) Destroy(p);
    }

    Sprite CreateAshSprite()
    {
        int size = Random.value > 0.5f ? 4 : 6;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                bool isCorner = (x == 0 && y == 0) ||
                                (x == size - 1 && y == 0) ||
                                (x == 0 && y == size - 1) ||
                                (x == size - 1 && y == size - 1);
                tex.SetPixel(x, y, new Color(1, 1, 1, isCorner ? 0f : 1f));
            }
        }
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}