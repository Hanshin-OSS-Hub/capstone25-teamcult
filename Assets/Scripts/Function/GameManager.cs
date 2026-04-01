using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Game Control")]
    public bool isLive = true;
    public float gameTime;
    public float maxGameTime = 20 * 60f;
    public int killCount = 0;

    [Header("UI References")]
    public TMP_Text timerText;
    public GameObject gameOverPanel;   // GameOverPanel 오브젝트 연결
    public Button btnRestart;          // 다시하기 버튼
    public Button btnMainMenu;         // 메인메뉴 버튼

    [Header("Scene Names")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Revive")]
    public int reviveCount = 0;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        Time.timeScale = 1;

        // 버튼 이벤트 연결
        if (btnRestart != null)
            btnRestart.onClick.AddListener(Retry);

        if (btnMainMenu != null)
            btnMainMenu.onClick.AddListener(GoToMainMenu);

        // 시작 시 패널 숨김
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    void Update()
    {
        if (!isLive)
        {
            if (Input.GetKeyDown(KeyCode.R)) Retry();
            return;
        }

        gameTime += Time.deltaTime;

        if (timerText != null)
        {
            int min = (int)(gameTime / 60);
            int sec = (int)(gameTime % 60);
            timerText.text = string.Format("{0:D2}:{1:D2}", min, sec);
        }
    }

    public void GameOver()
    {
        isLive = false;

        // 시간 정지 (플레이어 움직임 막기)
        Time.timeScale = 0f;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Debug.Log("[GameManager] GameOverPanel ON!");
        }
        else
        {
            Debug.LogError("[GameManager] gameOverPanel 슬롯이 비어있음!");
        }

        if (PlayerStats.instance != null)
        {
            PlayerStats.instance.currentGold = 0;
            if (TabController.instance != null)
                TabController.instance.UpdateGoldUI(0);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log($"게임 오버! 생존시간: {gameTime:F1}초, 처치 수: {killCount}");
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        reviveCount = 0;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        reviveCount = 0;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void AddRevive(int amount = 1)
    {
        reviveCount += amount;
    }

    public bool HasRevive() => reviveCount > 0;

    public void UseRevive()
    {
        if (reviveCount > 0) reviveCount--;
    }
}