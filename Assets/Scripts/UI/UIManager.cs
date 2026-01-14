using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements - Panels & Animator")]
    public GameObject settingsPanel; // 'SettingsPanel' 오브젝트 연결
    private Animator settingsAnimator;
    private bool isSettingsOpen = false;

    [Header("UI Elements - Sliders & Buttons")]
    public Slider volumeSlider;
    public Slider brightnessSlider;
    public Button settingsButton;
    public Button closeSettingsButton;
    public Button quitGameButton;

    [Header("Speaker Icon")]
    public Image speakerIcon;
    public Sprite spriteMute;
    public Sprite spriteLow;
    public Sprite spriteMedium;
    public Sprite spriteHigh;

    [Header("Health & Brightness Overlay")]
    public Image heartImage;
    public Image brightnessOverlay;
    public AudioMixer mainMixer;

    private const string VOLUME_KEY = "MasterVolume";
    private const string BRIGHTNESS_KEY = "MasterBrightness";

    void Awake()
    {
        // 시작 시 애니메이터를 미리 가져옵니다.
        if (settingsPanel != null)
        {
            settingsAnimator = settingsPanel.GetComponent<Animator>();
        }
    }

    void Start()
    {
        // 리스너 연결
        volumeSlider.onValueChanged.AddListener(SetVolume);
        brightnessSlider.onValueChanged.AddListener(SetBrightness);
        settingsButton.onClick.AddListener(OpenSettingsPanel); // Open 함수로 변경
        closeSettingsButton.onClick.AddListener(CloseSettingsPanel); // Close 함수로 변경
        quitGameButton.onClick.AddListener(QuitGame);

        LoadSettings();

        // 초기 상태 설정: 패널은 꺼두고 상태변수 초기화
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            isSettingsOpen = false;
        }
    }

    void Update()
    {
        // ESC 키 입력 감지 (통합된 로직)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isSettingsOpen) OpenSettingsPanel();
            else CloseSettingsPanel();
        }
    }

    // --- 설정 패널 제어 (애니메이션 포함) ---
    public void OpenSettingsPanel()
    {
        if (isSettingsOpen) return;

        isSettingsOpen = true;
        settingsPanel.SetActive(true); // 우선 오브젝트를 켭니다.
        Time.timeScale = 0f;

        if (settingsAnimator != null)
        {
            settingsAnimator.ResetTrigger("Close");
            settingsAnimator.SetTrigger("Open"); // 열기 애니메이션 실행
        }
        
    }

    public void CloseSettingsPanel()
    {
        if (!isSettingsOpen) return;
        isSettingsOpen = false;

        settingsPanel.SetActive(false);
        Time.timeScale = 1f;

        if (settingsAnimator != null)
        {
            settingsAnimator.ResetTrigger("Open");
            settingsAnimator.SetTrigger("Close"); // 닫기 애니메이션 실행
        }

        // 애니메이션이 끝난 후(0.5초 뒤) 오브젝트를 비활성화합니다.
        Invoke("DisableSettingsPanel", 0.5f);
    }

    private void DisableSettingsPanel()
    {
        // Invoke 실행 시점에 다시 열렸을 수도 있으므로 체크
        if (!isSettingsOpen) settingsPanel.SetActive(false);
    }

    // --- 기존 기능들 (기능 유지) ---
    private void LoadSettings()
    {
        float volume = PlayerPrefs.GetFloat(VOLUME_KEY, 1f);
        volumeSlider.value = volume;
        SetVolume(volume);

        float brightness = PlayerPrefs.GetFloat(BRIGHTNESS_KEY, 1f);
        brightnessSlider.value = brightness;
        SetBrightness(brightness);
    }

    public void SetVolume(float volume)
    {
        mainMixer.SetFloat("MasterVolume", volume > 0 ? Mathf.Log10(volume) * 20 : -80);
        PlayerPrefs.SetFloat(VOLUME_KEY, volume);

        if (speakerIcon == null) return;
        if (volume <= 0.0001f) speakerIcon.sprite = spriteMute;
        else if (volume <= 0.4f) speakerIcon.sprite = spriteLow;
        else if (volume <= 0.8f) speakerIcon.sprite = spriteMedium;
        else speakerIcon.sprite = spriteHigh;
    }

    public void SetBrightness(float brightness)
    {
        float alpha = 1.0f - brightness;
        if (brightnessOverlay != null) brightnessOverlay.color = new Color(0, 0, 0, alpha);
        PlayerPrefs.SetFloat(BRIGHTNESS_KEY, brightness);
    }

    public void UpdateHealthUI(float healthPercentage)
    {
        if (heartImage != null) heartImage.fillAmount = healthPercentage;
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}