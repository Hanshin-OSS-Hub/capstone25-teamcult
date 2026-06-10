using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [Header("Continue ¹öÆ°")]
    public Button continueButton;
    public Image continueImage;
    public Sprite continueSprite;
    public Sprite continueLockSprite;

    [Header("¾À ÀÌ¸§")]
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

    public void OnClickContinue()
    {
        if (SaveManager.instance == null || !SaveManager.instance.HasSavedRun())
            return;

        Time.timeScale = 1f;
        PlayerPrefs.SetInt("IsContinue", 1);
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnClickMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}