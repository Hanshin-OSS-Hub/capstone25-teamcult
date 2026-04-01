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
    public float initialDelay = 0.2f;
    public float charAnimDuration = 0.4f;
    public float subTextDelay = 0.1f;
    public float subTextFadeDuration = 0.4f;
    public float btnStartDelay = 0.1f;
    public float btnStartFadeDuration = 0.3f;
    public float btnQuitDelay = 0.1f;
    public float btnQuitFadeDuration = 0.3f;

    [Header("타이틀 글자 스타일")]
    public float titleFontSize = 60f;
    public float charSpacing = 48f;
    public float titleYPos = 120f;
    public float charRiseOffset = 30f;
    public Color titleFinalColor = new Color(0.91f, 0.88f, 0.82f, 1f);

    [Header("서브타이틀 스타일")]
    public float subFontSize = 18f;
    public Color subTextColor = new Color(0.47f, 0.43f, 0.38f, 1f);
    public float subTextYPos = 50f;
    public float subCharacterSpacing = 8f;

    [Header("버튼 - START")]
    public string btnStartLabel = "START GAME";
    public Color btnStartColor = new Color(0.78f, 0.66f, 0.43f, 1f);
    public Vector2 btnStartPosition = new Vector2(0, -40);
    public Vector2 btnStartSize = new Vector2(280, 56);

    [Header("버튼 - QUIT")]
    public string btnQuitLabel = "QUIT";
    public Color btnQuitColor = new Color(0.35f, 0.35f, 0.35f, 1f);
    public Vector2 btnQuitPosition = new Vector2(0, -110);
    public Vector2 btnQuitSize = new Vector2(280, 46);

    [Header("먼지 파티클")]
    public int dustParticleCount = 40;
    public float dustSpreadX = 600f;
    public float dustSpreadY = 400f;
    public Color dustColor = new Color(0.6f, 0.5f, 0.35f, 0.18f);

    private List<TextMeshProUGUI> charTexts = new List<TextMeshProUGUI>();
    private List<RectTransform> charRects = new List<RectTransform>();
    private List<Vector2> charFinalPositions = new List<Vector2>();
    private GameObject btnStart;
    private GameObject btnQuit;
    private TextMeshProUGUI subText;
    private GameObject canvasGO;

    void Start()
    {
        BuildUI();
        CreateDustParticles();
        StartCoroutine(TitleSequence());
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

            Vector2 finalPos = new Vector2(currentX, titleYPos);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(currentX, titleYPos - charRiseOffset);
            rt.sizeDelta = new Vector2(charSpacing + 10f, 80f);

            charTexts.Add(tmp);
            charRects.Add(rt);
            charFinalPositions.Add(finalPos);

            currentX += charSpacing;
        }

        subText = CreateTMP(canvasGO, "SubText", subTitleText, subFontSize,
            new Color(subTextColor.r, subTextColor.g, subTextColor.b, 0f));
        subText.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        subText.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        subText.rectTransform.anchoredPosition = new Vector2(0, subTextYPos);
        subText.rectTransform.sizeDelta = new Vector2(700, 40);
        subText.characterSpacing = subCharacterSpacing;
        subText.alignment = TextAlignmentOptions.Center;

        btnStart = CreateButton(canvasGO, "BtnStart", btnStartLabel,
            btnStartColor, btnStartPosition, btnStartSize);
        btnStart.GetComponent<Button>().onClick.AddListener(() =>
            SceneManager.LoadScene(gameSceneName));

        btnQuit = CreateButton(canvasGO, "BtnQuit", btnQuitLabel,
            btnQuitColor, btnQuitPosition, btnQuitSize);
        btnQuit.GetComponent<Button>().onClick.AddListener(() =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        });

        btnStart.SetActive(false);
        btnQuit.SetActive(false);
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

    GameObject CreateButton(GameObject parent, string name, string label,
        Color bgColor, Vector2 position, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent.transform, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        var img = go.GetComponent<Image>();
        img.color = bgColor;

        var labelTMP = CreateTMP(go, "Label", label, 20f, Color.white);
        labelTMP.rectTransform.anchorMin = Vector2.zero;
        labelTMP.rectTransform.anchorMax = Vector2.one;
        labelTMP.rectTransform.offsetMin = Vector2.zero;
        labelTMP.rectTransform.offsetMax = Vector2.zero;
        labelTMP.characterSpacing = 4f;
        labelTMP.fontStyle = FontStyles.Bold;

        go.AddComponent<CanvasGroup>();

        return go;
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
            rt.anchoredPosition = new Vector2(
                Random.Range(-dustSpreadX / 2f, dustSpreadX / 2f),
                Random.Range(-dustSpreadY / 2f, dustSpreadY / 2f));
            rt.sizeDelta = new Vector2(2f, 2f);

            var img = go.AddComponent<Image>();
            img.color = dustColor;

            StartCoroutine(FloatDust(rt));
        }
    }

    IEnumerator FloatDust(RectTransform rt)
    {
        float speed = Random.Range(10f, 35f);
        float drift = Random.Range(-15f, 15f);
        float delay = Random.Range(0f, 3f);
        yield return new WaitForSeconds(delay);

        while (rt != null)
        {
            rt.anchoredPosition += new Vector2(drift * Time.deltaTime * 0.1f, speed * Time.deltaTime);
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

        // 모든 글자 동시에 올라옴
        for (int i = 0; i < charTexts.Count; i++)
        {
            StartCoroutine(RiseInChar(charTexts[i], charRects[i], charFinalPositions[i], charAnimDuration));
        }

        // 애니메이션 끝날때까지 대기
        yield return new WaitForSeconds(charAnimDuration);

        yield return new WaitForSeconds(subTextDelay);
        yield return StartCoroutine(FadeInText(subText, subTextFadeDuration));

        yield return new WaitForSeconds(btnStartDelay);
        if (btnStart != null)
        {
            btnStart.SetActive(true);
            StartCoroutine(FadeInCanvasGroup(btnStart, btnStartFadeDuration));
        }
        yield return new WaitForSeconds(btnQuitDelay);
        if (btnQuit != null)
        {
            btnQuit.SetActive(true);
            StartCoroutine(FadeInCanvasGroup(btnQuit, btnQuitFadeDuration));
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

    IEnumerator FadeInCanvasGroup(GameObject go, float duration)
    {
        CanvasGroup cg = go.GetComponent<CanvasGroup>();
        if (cg == null) cg = go.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            cg.alpha = Mathf.Clamp01(timer / duration);
            yield return null;
        }
        cg.alpha = 1f;
    }
}