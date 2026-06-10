using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class MainMenuManager : MonoBehaviour
{
    [Header("Buttons")]
    public Button newGameButton;
    public Button continueButton;
    public Button settingsButton;
    public Button quitButton;

    [Header("새 게임 확인 팝업 (선택사항)")]
    public GameObject confirmPopup;
    public Button confirmYesButton;
    public Button confirmNoButton;

    [Header("Settings UI")]
    public GameObject settingsPanel;
    public Button closeSettingsButton;
    public Slider brightnessSlider;

    [Header("Scene")]
    public string gameSceneName = "Demo_B1";

    private Animator settingsAnimator;
    private bool isSettingsOpen = false;
    private const string BRIGHTNESS_KEY = "MasterBrightness";

    private string runSavePath => Application.persistentDataPath + "/run_save.json";
    private string oopartsSavePath => Application.persistentDataPath + "/ooparts_save.json";

    void Awake()
    {
        if (settingsPanel != null)
        {
            settingsAnimator = settingsPanel.GetComponent<Animator>();
        }
    }

    void Start()
    {
        //마석 데이터 있으면 couroutine 활성화
        bool hasOoparts = File.Exists(oopartsSavePath);
        if (continueButton != null)
            continueButton.interactable = hasOoparts;

        if (confirmPopup != null)
            confirmPopup.SetActive(false);

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            isSettingsOpen = false;
        }

        if (newGameButton != null) newGameButton.onClick.AddListener(OnNewGame);
        if (continueButton != null) continueButton.onClick.AddListener(OnContinue);
        if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettingsPanel);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuit);

        if (confirmYesButton != null) confirmYesButton.onClick.AddListener(OnConfirmNewGame);
        if (confirmNoButton != null) confirmNoButton.onClick.AddListener(OnCancelNewGame);

        if (closeSettingsButton != null) closeSettingsButton.onClick.AddListener(CloseSettingsPanel);

        //if (brightnessSlider != null)
        //{
        //    brightnessSlider.onValueChanged.AddListener(SetBrightness);
        //    if (PlayerPrefs.HasKey(BRIGHTNESS_KEY))
        //    {
        //        float savedBrightness = PlayerPrefs.GetFloat(BRIGHTNESS_KEY);
        //        brightnessSlider.value = savedBrightness;
        //        SetBrightness(savedBrightness);
        //    }
        //    else
        //    {
        //        SetBrightness(brightnessSlider.value);
        //    }
        //}
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isSettingsOpen) CloseSettingsPanel();
        }
    }
    //뉴게임 - 마석 있으면 팝업으로 확인
    void OnNewGame()
    {
        bool hasOoparts = File.Exists(oopartsSavePath);
        if (hasOoparts && confirmPopup != null)
        {
            confirmPopup.SetActive(true);
            return;
        }
        StartNewGame();
    }

    void OnConfirmNewGame()
    {
        if (confirmPopup != null) confirmPopup.SetActive(false);
        StartNewGame();
    }

    void OnCancelNewGame()
    {
        if (confirmPopup != null) confirmPopup.SetActive(false);
    }

    void StartNewGame()
    {
        if (File.Exists(runSavePath)) File.Delete(runSavePath);
        if (File.Exists(oopartsSavePath)) File.Delete(oopartsSavePath);

        PlayerPrefs.SetInt("IsContinue", 0);
        PlayerPrefs.Save();

        SceneManager.LoadScene(gameSceneName);
    }
    //Continue - 마석유지한체 게임시작
    void OnContinue()
    {
        PlayerPrefs.SetInt("IsContinue", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenSettingsPanel()
    {
        if (isSettingsOpen) return;

        isSettingsOpen = true;
        settingsPanel.SetActive(true);
        Time.timeScale = 0f;

        if (settingsAnimator != null)
        {
            settingsAnimator.ResetTrigger("Close");
            settingsAnimator.SetTrigger("Open");
        }
    }

    public void CloseSettingsPanel()
    {
        if (!isSettingsOpen) return;
        isSettingsOpen = false;
        Time.timeScale = 1f;

        if (settingsAnimator != null)
        {
            settingsAnimator.ResetTrigger("Open");
            settingsAnimator.SetTrigger("Close");
            Invoke("DisableSettingsPanel", 0.5f);
        }
        else
        {
            settingsPanel.SetActive(false);
        }
    }

    private void DisableSettingsPanel()
    {
        if (!isSettingsOpen && settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    //public void SetBrightness(float brightness)
    //{
    //    float alpha = 1.0f - brightness;
    //    if (brightnessOverlay != null) brightnessOverlay.color = new Color(0, 0, 0, alpha);
    //    PlayerPrefs.SetFloat(BRIGHTNESS_KEY, brightness);
    //}

    void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}