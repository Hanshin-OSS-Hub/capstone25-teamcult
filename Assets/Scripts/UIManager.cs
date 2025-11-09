using UnityEngine;
using UnityEngine.UI; // UI 요소(Slider, Button, Image) 사용
using UnityEngine.Audio; // AudioMixer 사용

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider volumeSlider;
    public Slider brightnessSlider;
    public Button settingsButton;

    [Header("Health")]
    public Image heartImage; // 1-1에서 설정한 'Filled' 하트 이미지

    [Header("Panels")]
    public GameObject settingsPanel; // 설정 버튼으로 켜고 끌 패널

    [Header("Settings Panel Buttons")]
    public Button closeSettingsButton; // 설정 패널의 '닫기' 버튼
    public Button quitGameButton;      // 설정 패널의 '게임 종료' 버튼

    [Header("Mixer & Brightness")]
    public AudioMixer mainMixer; // 오디오 믹서
    public Image brightnessOverlay; // 밝기 조절용 오버레이

    // PlayerPrefs(저장)을 위한 키 이름
    private const string VOLUME_KEY = "MasterVolume";
    private const string BRIGHTNESS_KEY = "MasterBrightness";

    void Start()
    {
        // 리스너(Listener) 연결
        volumeSlider.onValueChanged.AddListener(SetVolume);
        brightnessSlider.onValueChanged.AddListener(SetBrightness);

        // 버튼 클릭 리스너 연결
        settingsButton.onClick.AddListener(ToggleSettingsPanel);
        closeSettingsButton.onClick.AddListener(CloseSettingsPanel); // 닫기 버튼 연결
        quitGameButton.onClick.AddListener(QuitGame);             // 종료 버튼 연결

        // 저장된 설정 불러오기
        LoadSettings();

        // 설정 패널은 처음엔 닫아둠
        settingsPanel.SetActive(false);
    }

    private void LoadSettings()
    {
        // 볼륨 불러오기
        float volume = PlayerPrefs.GetFloat(VOLUME_KEY, 1f);
        volumeSlider.value = volume;
        SetVolume(volume);

        // 밝기 불러오기
        float brightness = PlayerPrefs.GetFloat(BRIGHTNESS_KEY, 1f);
        brightnessSlider.value = brightness;
        SetBrightness(brightness);
    }

    // --- 생명력 UI 업데이트 ---
    public void UpdateHealthUI(float healthPercentage)
    {
        // healthPercentage는 0.0f ~ 1.0f 사이의 값
        if (heartImage != null)
        {
            heartImage.fillAmount = healthPercentage;
        }
    }

    // --- 볼륨 및 밝기 ---
    public void SetVolume(float volume)
    {
        mainMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat(VOLUME_KEY, volume);
    }

    public void SetBrightness(float brightness)
    {
        float alpha = 1.0f - brightness;
        brightnessOverlay.color = new Color(0, 0, 0, alpha);
        PlayerPrefs.SetFloat(BRIGHTNESS_KEY, brightness);
    }

    // --- 설정 패널 제어 ---
    public void ToggleSettingsPanel()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    // 닫기 버튼이 호출할 함수
    public void CloseSettingsPanel()
    {
        settingsPanel.SetActive(false);
    }

    // 게임 종료 버튼이 호출할 함수
    public void QuitGame()
    {
        Debug.Log("Quitting game..."); // 에디터 테스트용 로그
        Application.Quit();

        // (참고) 유니티 에디터에서는 Application.Quit()이 작동하지 않습니다.
        // 빌드된 게임에서만 작동합니다.
    }
}