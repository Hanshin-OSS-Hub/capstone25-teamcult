using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class LogManager : MonoBehaviour {
    public static LogManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject logEntryPrefab;
    [SerializeField] private Transform contentParent;
    [SerializeField] private ScrollRect scrollRect;

    [Header("Settings")]
    [SerializeField] private int maxLogLines = 100;

    private Queue<GameObject> logQueue = new Queue<GameObject>();

    void Awake() {
        if (Instance == null) {
            Instance = this;

            DontDestroyOnLoad(gameObject);

            ClearLog();
        }
        else {
            Destroy(gameObject);
            return;
        }
    }

    private void Start() {
        AddLog("시작");
    }

    /// <summary>
    /// 로그를 모두 비우고, 지정된 횟수만큼 공백 로그를 추가합니다.
    /// </summary>
    public void ClearLog() { // int cnt = 10
        // 1. 기존 큐에 있는 오브젝트 모두 파괴 및 큐 비우기
        while (logQueue.Count > 0) {
            GameObject oldLog = logQueue.Dequeue();
            Destroy(oldLog);
        }

        // 잘 안되서 유기
        //// 2. 매개변수(cnt)만큼 공백 로그 추가
        //for (int i = 0; i < cnt; i++) {
        //    AddLog(" "); // 공백 문자열 추가
        //}
    }

    public void AddLog(string message) {
        // 1. 새로운 로그 생성 및 큐에 삽입
        GameObject newLog = Instantiate(logEntryPrefab, contentParent);
        newLog.GetComponent<TextMeshProUGUI>().text = message;
        logQueue.Enqueue(newLog);

        // 2. 큐의 개수 제한 (while로 방어적 코드 작성)
        while (logQueue.Count > maxLogLines) {
            GameObject oldLog = logQueue.Dequeue();
            Destroy(oldLog);
        }

        // 3. 자동 스크롤
        if (gameObject.activeInHierarchy) { // 활성화 상태일 때만 코루틴 실행
            StopAllCoroutines();
            StartCoroutine(ScrollToBottom());
        }
    }

    IEnumerator ScrollToBottom() {
        yield return new WaitForEndOfFrame();
        if (scrollRect != null) {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    ////// 급조한 테스트용 코드, 테스트할때 주석 풀고 사용할것
    //private int k = 1; // 점의 개수 및 카운트 변수

    //void Update() {
    //    // Q 키를 눌렀을 때 실행
    //    if (Input.GetKeyDown(KeyCode.Q)) {
    //        string dots = new string('.', k); // k개의 점 생성
    //        string testMessage = $"{k}번째 테스트 로그입니다{dots}";
    //        AddLog(testMessage);
    //        k++; // 다음 로그를 위해 k 증가

    //    }
    //    // 리셋
    //    if (Input.GetKeyDown(KeyCode.R)) {
    //        ClearLog();
    //        k = 1; // 카운트 초기화
    //    }
    //}
}