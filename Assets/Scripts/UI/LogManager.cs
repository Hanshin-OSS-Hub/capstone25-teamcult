using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class LogManager : MonoBehaviour {
    public static LogManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject logEntryPrefab;
    [SerializeField] private GameObject floatingLogEntryPrefab;
    [SerializeField] private Transform contentParent;
    [SerializeField] private Transform floatingContentParent;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private CanvasGroup logWindowGroup;

    [Header("Log Settings")]
    [SerializeField] private int maxLogLines = 100;

    [Header("Floating Log Settings")]
    [SerializeField] private float floatingVisibleTime = 1f;
    [SerializeField] private float floatingFadeTime = 1f;

    private readonly Queue<GameObject> logQueue = new Queue<GameObject>();

    private bool isLogWindowVisible = false;
    private Coroutine scrollCoroutine;

    void Awake() {
        if (Instance == null) {
            Instance = this;

            DontDestroyOnLoad(gameObject);

            ClearLog();
            ApplyLogWindowVisibility();
        }
        else {
            Destroy(gameObject);
            return;
        }
    }

    private void Start() {
        AddLog("시작");
    }

    private int testLogIndex = 1;
    private void Update() {
        // Enter로 로그 창 토글
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) {
            ToggleLogWindow();
        }

        // K로 테스트 로그 추가
        if (Input.GetKeyDown(KeyCode.K)) {
            AddLog($"{testLogIndex++}. 로그 테스트중");
        }
    }

    public void ClearLog() {
        while (logQueue.Count > 0) {
            GameObject oldLog = logQueue.Dequeue();

            if (oldLog != null) {
                Destroy(oldLog);
            }
        }
    }

    public void AddLog(string message) {
        AddPermanentLog(message);
        AddFloatingLog(message);

        if (isLogWindowVisible) {
            StartScrollToBottom();
        }
    }

    private void AddPermanentLog(string message) {
        if (logEntryPrefab == null || contentParent == null) {
            Debug.LogWarning("LogManager: logEntryPrefab 또는 contentParent가 비어 있습니다.");
            return;
        }

        GameObject newLog = Instantiate(logEntryPrefab, contentParent);
        SetLogText(newLog, message);

        logQueue.Enqueue(newLog);

        while (logQueue.Count > maxLogLines) {
            GameObject oldLog = logQueue.Dequeue();

            if (oldLog != null) {
                Destroy(oldLog);
            }
        }
    }

    private void AddFloatingLog(string message) {
        if (floatingLogEntryPrefab == null || floatingContentParent == null) {
            return;
        }

        GameObject floatingLog = Instantiate(floatingLogEntryPrefab, floatingContentParent);

        SetLogText(floatingLog, message);

        CanvasGroup canvasGroup = floatingLog.GetComponent<CanvasGroup>();

        if (canvasGroup == null) {
            canvasGroup = floatingLog.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        StartCoroutine(FloatingLogRoutine(floatingLog, canvasGroup));
    }

    private void SetLogText(GameObject logObject, string message) {
        TextMeshProUGUI text = logObject.GetComponent<TextMeshProUGUI>();

        if (text == null) {
            text = logObject.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (text != null) {
            text.text = message;
        }
        else {
            Debug.LogWarning("LogManager: logEntryPrefab에서 TextMeshProUGUI를 찾지 못했습니다.");
        }
    }

    private IEnumerator FloatingLogRoutine(GameObject floatingLog, CanvasGroup canvasGroup) {
        yield return new WaitForSeconds(floatingVisibleTime);

        float elapsed = 0f;

        while (elapsed < floatingFadeTime) {
            elapsed += Time.deltaTime;

            float t = elapsed / floatingFadeTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        if (floatingLog != null) {
            Destroy(floatingLog);
        }
    }

    private void ToggleLogWindow() {
        isLogWindowVisible = !isLogWindowVisible;

        ApplyLogWindowVisibility();

        if (isLogWindowVisible) {
            StartScrollToBottom();
        }
    }

    private void ApplyLogWindowVisibility() {
        if (logWindowGroup != null) {
            logWindowGroup.alpha = isLogWindowVisible ? 1f : 0f;
            logWindowGroup.interactable = isLogWindowVisible;
            logWindowGroup.blocksRaycasts = isLogWindowVisible;
        }

        if (floatingContentParent != null) {
            CanvasGroup floatingAreaGroup = floatingContentParent.GetComponent<CanvasGroup>();

            if (floatingAreaGroup == null) {
                floatingAreaGroup = floatingContentParent.gameObject.AddComponent<CanvasGroup>();
            }

            floatingAreaGroup.alpha = isLogWindowVisible ? 0f : 1f;
            floatingAreaGroup.interactable = false;
            floatingAreaGroup.blocksRaycasts = false;
        }
    }

    private void StartScrollToBottom() {
        if (scrollCoroutine != null) {
            StopCoroutine(scrollCoroutine);
        }

        scrollCoroutine = StartCoroutine(ScrollToBottom());
    }

    private IEnumerator ScrollToBottom() {
        yield return new WaitForEndOfFrame();

        Canvas.ForceUpdateCanvases();

        if (scrollRect != null) {
            scrollRect.verticalNormalizedPosition = 0f;
        }

        scrollCoroutine = null;
    }
}