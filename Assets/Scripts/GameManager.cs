using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // 씬 관리
using TMPro; // 텍스트메쉬프로

public class GameManager : MonoBehaviour
{
    public static GameManager instance; // 싱글톤 (어디서든 부를 수 있게)

    [Header("Game Control")]
    public bool isLive = true;       // 게임 상태
    public float gameTime;           // 생존 시간
    public float maxGameTime = 20 * 60f; // 최대 시간 (20분)
    public int killCount = 0;        // ★ [추가] 처치한 적 수

    [Header("UI References")]
    public TMP_Text timerText;       // 타이머 UI 연결
    public GameObject gameOverPanel; // 게임오버 패널 연결

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        Time.timeScale = 1; // 시간 흐르게 설정
    }

    void Update()
    {
        if (!isLive)
        {
            // 게임 오버 상태에서 R키 누르면 재시작
            if (Input.GetKeyDown(KeyCode.R))
            {
                Retry();
            }
            return;
        }

        // 1. 시간 증가
        gameTime += Time.deltaTime;

        // 2. 타이머 UI 갱신 (00:00)
        if (timerText != null)
        {
            int min = (int)(gameTime / 60);
            int sec = (int)(gameTime % 60);
            timerText.text = string.Format("{0:D2}:{1:D2}", min, sec);
        }
    }

    // 플레이어 사망 시 호출
    public void GameOver()
    {
        isLive = false;

        // 게임오버 창 띄우기
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Debug.Log($"게임 오버! 생존시간: {gameTime:F1}초, 처치 수: {killCount}");
    }

    // 재시작 (R키)
    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}