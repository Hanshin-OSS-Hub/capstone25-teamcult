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

    [Header("Scene")]
    public string gameSceneName = "createMap";

    private string runSavePath => Application.persistentDataPath + "/run_save.json";
    private string oopartsSavePath => Application.persistentDataPath + "/ooparts_save.json";

    void Start()
    {
        // 마석 데이터 있으면 Continue 활성화
        bool hasOoparts = File.Exists(oopartsSavePath);
        if (continueButton != null)
            continueButton.interactable = hasOoparts;

        if (confirmPopup != null)
            confirmPopup.SetActive(false);

        if (newGameButton != null) newGameButton.onClick.AddListener(OnNewGame);
        if (continueButton != null) continueButton.onClick.AddListener(OnContinue);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettings);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuit);

        if (confirmYesButton != null) confirmYesButton.onClick.AddListener(OnConfirmNewGame);
        if (confirmNoButton != null) confirmNoButton.onClick.AddListener(OnCancelNewGame);
    }

    // 뉴게임 - 마석 있으면 팝업으로 확인
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
        // 런 + 마석 전부 초기화
        if (File.Exists(runSavePath)) File.Delete(runSavePath);
        if (File.Exists(oopartsSavePath)) File.Delete(oopartsSavePath);

        PlayerPrefs.SetInt("IsContinue", 0);
        PlayerPrefs.Save();

        SceneManager.LoadScene(gameSceneName);
    }

    // Continue - 마석 유지한 채 게임 시작
    void OnContinue()
    {
        PlayerPrefs.SetInt("IsContinue", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene(gameSceneName);
    }

    void OnSettings()
    {
        Debug.Log("[MainMenu] 설정 (미구현)");
    }

    void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}