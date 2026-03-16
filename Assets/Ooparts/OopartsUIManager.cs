using UnityEngine;
using System.Collections;

public class OopartsUIManager : MonoBehaviour
{
    [Header("오파츠 패널")]
    public GameObject oopartsPanel;

    [Header("여는 키 설정 (기본 O)")]
    public KeyCode toggleKey = KeyCode.O;

    [Header("페이드 속도")]
    public float fadeDuration = 0.3f;

    private CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;

    private void Start()
    {
        if (oopartsPanel != null)
        {
            canvasGroup = oopartsPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = oopartsPanel.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 0f;
            oopartsPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (oopartsPanel != null)
            {
                if (oopartsPanel.activeSelf)
                    StartFade(false);
                else
                    StartFade(true);
            }
        }
    }

    void StartFade(bool open)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        // 패널 닫을 때 툴팁도 숨기기
        if (!open)
            OopartsTooltip.instance?.Hide();

        fadeCoroutine = StartCoroutine(Fade(open));
    }

    IEnumerator Fade(bool open)
    {
        if (open)
        {
            oopartsPanel.SetActive(true);
            canvasGroup.alpha = 0f;
        }

        float start = canvasGroup.alpha;
        float end = open ? 1f : 0f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, end, elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = end;

        if (!open)
            oopartsPanel.SetActive(false);
    }
}