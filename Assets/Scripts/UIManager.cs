using UnityEngine;
using UnityEngine.UI; // UI 요소(Slider, Button, Image) 사용
using UnityEngine.Audio; // AudioMixer 사용
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;
public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider volumeSlider;
    public Slider brightnessSlider;
    public Button settingsButton;
    [Header("Speaker Icon")]
    public Image speakerIcon; // 1. Hierarchy의 'SpeakerIcon'을 연결할 슬롯
    public Sprite spriteMute;
    public Sprite spriteLow;
    public Sprite spriteMedium;
    public Sprite spriteHigh;

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
    public void DeselectUIElement()
    {
        // 현재 선택된 UI(슬라이더)를 null로 만들어 포커스를 해제
        EventSystem.current.SetSelectedGameObject(null);
    }
    // --- 볼륨 및 밝기 ---
    public void SetVolume(float volume)
    {
        mainMixer.SetFloat("MasterVolume", volume > 0 ? Mathf.Log10(volume) * 20 : -80); // -80이 무음
        PlayerPrefs.SetFloat(VOLUME_KEY, volume);
        if (speakerIcon == null) return; // 아이콘이 없으면 종료

        if (volume <= 0.0001f) // 0에 가까우면 (음소거)
        {
            speakerIcon.sprite = spriteMute;
        }
        else if (volume <= 0.4f) // 40% 이하 (낮음)
        {
            speakerIcon.sprite = spriteLow;
        }
        else if (volume <= 0.8f) // 80% 이하 (중간)
        {
            speakerIcon.sprite = spriteMedium;
        }
        else // 80% 초과 (높음)
        {
            speakerIcon.sprite = spriteHigh;
        }
    }

    public void SetBrightness(float brightness)
    {
        float alpha = 1.0f - brightness;
        if (brightnessOverlay == null) { Debug.Log("brightnessOverlay == null"); }
        else { brightnessOverlay.color = new Color(0, 0, 0, alpha); }
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