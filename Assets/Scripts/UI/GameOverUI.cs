using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [Header("Continue 버튼")]
    public Button continueButton;
    public Image continueImage;
    public Sprite continueSprite;
    public Sprite continueLockSprite;

    [Header("씬 이름")]
    public string gameSceneName = "createMap";
    public string mainMenuSceneName = "MainMenu";

    void OnEnable()
    {
        RefreshContinueButton();
    }

    void RefreshContinueButton()
    {
        bool hasSave = (SaveManager.instance != null) && SaveManager.instance.HasSavedRun();

        if (continueButton != null)
            continueButton.interactable = hasSave;

        if (continueImage != null)
            continueImage.sprite = hasSave ? continueSprite : continueLockSprite;
    }

    // 재시작: 완전 초기화 (런 + 마석 전부 삭제)
    public void OnClickRetry()
    {
        Time.timeScale = 1f;
        PlayerPrefs.SetInt("IsContinue", 0);

        if (SaveManager.instance != null)
            SaveManager.instance.DeleteRun();

        if (OopartsSaveManager.instance != null)
            OopartsSaveManager.instance.ResetAll();

        SceneManager.LoadScene(gameSceneName);
    }

    // 이어하기: 세이브 유지하고 복원
    public void OnClickContinue()
    {
        if (SaveManager.instance == null || !SaveManager.instance.HasSavedRun())
            return;

        Time.timeScale = 1f;
        PlayerPrefs.SetInt("IsContinue", 1);
        SceneManager.LoadScene(gameSceneName);
    }

    // 메인 메뉴로
    public void OnClickMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}