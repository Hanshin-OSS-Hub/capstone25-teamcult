using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [Header("Game Control")]
    public bool isLive = true;
    public bool isUIOpen = false;
    public float gameTime;
    public float maxGameTime = 20 * 60f;
    public int killCount = 0;
    [Header("UI References")]
    public TMP_Text timerText;
    public GameObject gameOverPanel;
    [Header("Scene Names")]
    public string mainMenuSceneName = "MainMenu";
    [Header("Revive")]
    public int reviveCount = 0;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        Time.timeScale = 1;
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
        Time.timeScale = 0f;

        // 세이브는 지우지 않음 (Continue를 위해 유지)

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
            PlayerStats.instance.SetGold(0);
            if (TabController.instance != null)
                TabController.instance.UpdateGoldUI(0);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log($"게임 오버! 생존시간: {gameTime:F1}초, 처치 수: {killCount}");
    }

    // 처음부터 (세이브 삭제하고 새로)
    public void Retry()
    {
        Time.timeScale = 1f;
        reviveCount = 0;
        if (SaveManager.instance != null) SaveManager.instance.DeleteRun();
        PlayerPrefs.SetInt("IsContinue", 0);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 메인메뉴
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        reviveCount = 0;
        // 세이브는 유지 (메인에서 이어하기 가능하게). 지우려면 아래 주석 해제
        // if (SaveManager.instance != null) SaveManager.instance.DeleteRun();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void AddRevive(int amount = 1) { reviveCount += amount; }
    public bool HasRevive() => reviveCount > 0;
    public void UseRevive() { if (reviveCount > 0) reviveCount--; }
}