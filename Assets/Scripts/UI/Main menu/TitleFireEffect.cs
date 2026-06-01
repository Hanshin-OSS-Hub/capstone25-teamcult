using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class TitleFireEffect : MonoBehaviour
{
    [Header("씬 이름")]
    public string gameSceneName = "createMap";

    [Header("타이틀 텍스트")]
    public string titleText = "BLIND ECHO";
    public string subTitleText = "ROGUELIKE  -  ENTER THE DARK";

    [Header("타이밍")]
    public float initialDelay = 0.5f;
    public float charAnimDuration = 1.2f;
    public float subTextDelay = 0.4f;
    public float subTextFadeDuration = 1.0f;
    public float pressKeyDelay = 0.5f;

    [Header("타이틀 글자 스타일")]
    public float titleFontSize = 110f;
    public float charSpacing = 72f;
    public float titleYPos = 140f;
    public float charRiseOffset = 60f;
    public Color titleFinalColor = new Color(0.95f, 0.88f, 0.70f, 1f);

    [Header("서브타이틀 스타일")]
    public float subFontSize = 18f;
    public Color subTextColor = new Color(0.45f, 0.38f, 0.30f, 1f);
    public float subTextYPos = 60f;
    public float subCharacterSpacing = 12f;

    [Header("PRESS ANY KEY 스타일")]
    public float pressKeyFontSize = 22f;
    public Color pressKeyColor = new Color(0.80f, 0.70f, 0.50f, 1f);
    public float pressKeyYPos = -80f;
    public float blinkSpeed = 1.2f;

    [Header("먼지 파티클")]
    public int dustParticleCount = 80;
    public float dustSpreadX = 800f;
    public float dustSpreadY = 500f;
    public Color dustColor = new Color(0.55f, 0.40f, 0.20f, 0.12f);
    public float dustMinSize = 1f;
    public float dustMaxSize = 3f;

    private List<TextMeshProUGUI> charTexts = new List<TextMeshProUGUI>();
    private List<RectTransform> charRects = new List<RectTransform>();
    private List<Vector2> charFinalPositions = new List<Vector2>();
    private TextMeshProUGUI subText;
    private TextMeshProUGUI pressKeyText;
    private TextMeshProUGUI versionText;
    private GameObject canvasGO;
    private bool canStart = false;

    void Start()
    {
        BuildUI();
        CreateDustParticles();
        CreateVignette();
        StartCoroutine(TitleSequence());
    }

    void Update()
    {
        // 실시간 위치 반영
        for (int i = 0; i < charRects.Count; i++)
        {
            charRects[i].anchoredPosition = new Vector2(charFinalPositions[i].x, titleYPos);
            charFinalPositions[i] = new Vector2(charFinalPositions[i].x, titleYPos);
        }

        if (subText != null)
            subText.rectTransform.anchoredPosition = new Vector2(0, subTextYPos);

        if (pressKeyText != null)
            pressKeyText.rectTransform.anchoredPosition = new Vector2(0, pressKeyYPos);

        // 아무 키나 누르면 시작
        if (canStart && Input.anyKeyDown)
        {
            SceneManager.LoadScene(gameSceneName);
        }
    }

    void BuildUI()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        canvasGO = canvas != null ? canvas.gameObject : gameObject;

        float totalWidth = 0f;
        foreach (char c in titleText)
            totalWidth += (c == ' ') ? charSpacing * 0.6f : charSpacing;

        float startX = -totalWidth / 2f + charSpacing / 2f;
        float currentX = startX;

        foreach (char c in titleText)
        {
            if (c == ' ')
            {
                currentX += charSpacing * 0.6f;
                continue;
            }

            GameObject go = new GameObject("Char_" + c, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(canvasGO.transform, false);

            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = c.ToString();
            tmp.fontSize = titleFontSize;
            tmp.color = new Color(titleFinalColor.r, titleFinalColor.g, titleFinalColor.b, 0f);
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;

            Vector2 finalPos = new Vector2(currentX, titleYPos);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(currentX, titleYPos - charRiseOffset);
            rt.sizeDelta = new Vector2(charSpacing + 10f, 120f);

            charTexts.Add(tmp);
            charRects.Add(rt);
            charFinalPositions.Add(finalPos);

            currentX += charSpacing;
        }

        // 서브타이틀
        subText = CreateTMP(canvasGO, "SubText", subTitleText, subFontSize,
            new Color(subTextColor.r, subTextColor.g, subTextColor.b, 0f));
        subText.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        subText.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        subText.rectTransform.anchoredPosition = new Vector2(0, subTextYPos);
        subText.rectTransform.sizeDelta = new Vector2(800, 40);
        subText.characterSpacing = subCharacterSpacing;
        subText.alignment = TextAlignmentOptions.Center;

        // 구분선
        CreateDivider(canvasGO, new Vector2(0, subTextYPos - 18f), new Vector2(320, 1));

        // PRESS ANY KEY 텍스트
        pressKeyText = CreateTMP(canvasGO, "PressKeyText", "PRESS ANY KEY", pressKeyFontSize,
            new Color(pressKeyColor.r, pressKeyColor.g, pressKeyColor.b, 0f));
        pressKeyText.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        pressKeyText.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        pressKeyText.rectTransform.anchoredPosition = new Vector2(0, pressKeyYPos);
        pressKeyText.rectTransform.sizeDelta = new Vector2(500, 50);
        pressKeyText.characterSpacing = 8f;
        pressKeyText.alignment = TextAlignmentOptions.Center;

        // 버전 텍스트
        versionText = CreateTMP(canvasGO, "VersionText", "v0.1 ALPHA", 13f,
            new Color(0.3f, 0.25f, 0.20f, 0f));
        versionText.rectTransform.anchorMin = new Vector2(1f, 0f);
        versionText.rectTransform.anchorMax = new Vector2(1f, 0f);
        versionText.rectTransform.anchoredPosition = new Vector2(-30f, 20f);
        versionText.rectTransform.sizeDelta = new Vector2(120, 30);
        versionText.alignment = TextAlignmentOptions.Right;
    }

    void CreateDivider(GameObject parent, Vector2 position, Vector2 size)
    {
        GameObject go = new GameObject("Divider", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent.transform, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        var img = go.GetComponent<Image>();
        img.color = new Color(0.45f, 0.32f, 0.15f, 0.5f);
    }

    void CreateVignette()
    {
        GameObject go = new GameObject("Vignette", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(canvasGO.transform, false);
        go.transform.SetAsFirstSibling();

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var img = go.GetComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.45f);
        img.raycastTarget = false;
    }

    TextMeshProUGUI CreateTMP(GameObject parent, string name, string text,
        float fontSize, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent.transform, false);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        return tmp;
    }

    void CreateDustParticles()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        GameObject parent = canvas != null ? canvas.gameObject : gameObject;

        for (int i = 0; i < dustParticleCount; i++)
        {
            GameObject go = new GameObject("Dust_" + i, typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);

            float size = Random.Range(dustMinSize, dustMaxSize);
            rt.sizeDelta = new Vector2(size, size);
            rt.anchoredPosition = new Vector2(
                Random.Range(-dustSpreadX / 2f, dustSpreadX / 2f),
                Random.Range(-dustSpreadY / 2f, dustSpreadY / 2f));

            var img = go.AddComponent<Image>();
            float alpha = Random.Range(0.05f, 0.18f);
            img.color = new Color(dustColor.r, dustColor.g, dustColor.b, alpha);
            img.raycastTarget = false;

            StartCoroutine(FloatDust(rt));
        }
    }

    IEnumerator FloatDust(RectTransform rt)
    {
        float speed = Random.Range(8f, 25f);
        float drift = Random.Range(-8f, 8f);
        float delay = Random.Range(0f, 4f);
        yield return new WaitForSeconds(delay);

        while (rt != null)
        {
            rt.anchoredPosition += new Vector2(drift * Time.deltaTime * 0.08f, speed * Time.deltaTime);
            if (rt.anchoredPosition.y > dustSpreadY / 2f)
            {
                rt.anchoredPosition = new Vector2(
                    Random.Range(-dustSpreadX / 2f, dustSpreadX / 2f),
                    -dustSpreadY / 2f);
            }
            yield return null;
        }
    }

    IEnumerator TitleSequence()
    {
        yield return new WaitForSeconds(initialDelay);

        for (int i = 0; i < charTexts.Count; i++)
        {
            StartCoroutine(RiseInChar(charTexts[i], charRects[i], charFinalPositions[i], charAnimDuration));
        }

        yield return new WaitForSeconds(charAnimDuration);

        yield return new WaitForSeconds(subTextDelay);
        yield return StartCoroutine(FadeInText(subText, subTextFadeDuration));

        StartCoroutine(FadeInTMP(versionText, 0.8f));

        yield return new WaitForSeconds(pressKeyDelay);

        // PRESS ANY KEY 페이드인 후 깜빡이기
        yield return StartCoroutine(FadeInTMP(pressKeyText, 0.6f));
        canStart = true;
        StartCoroutine(BlinkText(pressKeyText));
    }

    IEnumerator BlinkText(TextMeshProUGUI tmp)
    {
        while (true)
        {
            yield return StartCoroutine(FadeText(tmp, 1f, 0.2f, blinkSpeed));
            yield return StartCoroutine(FadeText(tmp, 0.2f, 1f, blinkSpeed));
        }
    }

    IEnumerator FadeText(TextMeshProUGUI tmp, float fromAlpha, float toAlpha, float duration)
    {
        if (tmp == null) yield break;
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            Color c = tmp.color;
            c.a = Mathf.Lerp(fromAlpha, toAlpha, t);
            tmp.color = c;
            yield return null;
        }
    }

    IEnumerator RiseInChar(TextMeshProUGUI tmp, RectTransform rt, Vector2 finalPos, float duration)
    {
        if (tmp == null) yield break;
        float timer = 0f;
        Vector2 startPos = new Vector2(finalPos.x, finalPos.y - charRiseOffset);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            rt.anchoredPosition = Vector2.Lerp(startPos, finalPos, smooth);
            tmp.color = new Color(titleFinalColor.r, titleFinalColor.g, titleFinalColor.b, smooth);
            yield return null;
        }

        rt.anchoredPosition = finalPos;
        tmp.color = titleFinalColor;
    }

    IEnumerator FadeInText(TextMeshProUGUI tmp, float duration)
    {
        if (tmp == null) yield break;
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            Color c = tmp.color;
            c.a = Mathf.Clamp01(timer / duration);
            tmp.color = c;
            yield return null;
        }
        tmp.color = new Color(subTextColor.r, subTextColor.g, subTextColor.b, 1f);
    }

    IEnumerator FadeInTMP(TextMeshProUGUI tmp, float duration)
    {
        if (tmp == null) yield break;
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            Color c = tmp.color;
            c.a = Mathf.Clamp01(timer / duration);
            tmp.color = c;
            yield return null;
        }
        Color fc = tmp.color;
        fc.a = 1f;
        tmp.color = fc;
    }
}