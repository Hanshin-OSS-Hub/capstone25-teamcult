using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements - Panels & Animator")]
    public GameObject settingsPanel; 
    private Animator settingsAnimator;
    private bool isSettingsOpen = false;

    [Header("UI Elements - Sliders & Buttons")]
    public Slider volumeSlider;
    public Slider brightnessSlider;
    public Button settingsButton;
    public Button closeSettingsButton;
    public Button quitGameButton;

   

    [Header("Health & Brightness Overlay")]
    public Image heartImage;
    public Image brightnessOverlay;
    public AudioMixer mainMixer;

    private const string VOLUME_KEY = "MasterVolume";
    private const string BRIGHTNESS_KEY = "MasterBrightness";

    void Awake()
    {
        if (settingsPanel != null)
        {
            settingsAnimator = settingsPanel.GetComponent<Animator>();
        }
    }

    void Start()
    {
        // 리스너 연결
        brightnessSlider.onValueChanged.AddListener(SetBrightness);
        settingsButton.onClick.AddListener(OpenSettingsPanel); 
        closeSettingsButton.onClick.AddListener(CloseSettingsPanel); 
        quitGameButton.onClick.AddListener(QuitGame);
        if (PlayerPrefs.HasKey(BRIGHTNESS_KEY))
        {
            float savedBrightness = PlayerPrefs.GetFloat(BRIGHTNESS_KEY);
            brightnessSlider.value = savedBrightness; 
            SetBrightness(savedBrightness); 
        }
        else
        {
            SetBrightness(brightnessSlider.value);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            isSettingsOpen = false;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (!isSettingsOpen) OpenSettingsPanel();
            else CloseSettingsPanel();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (TabController.instance.mainPanel.activeSelf)
            {
                TabController.instance.ToggleWindow();
                return;
            }

            if (isSettingsOpen) CloseSettingsPanel();
        }
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

        settingsPanel.SetActive(false);
        Time.timeScale = 1f;

        if (settingsAnimator != null)
        {
            settingsAnimator.ResetTrigger("Open");
            settingsAnimator.SetTrigger("Close"); 
        }

        Invoke("DisableSettingsPanel", 0.5f);
    }

    private void DisableSettingsPanel()
    {
        if (!isSettingsOpen) settingsPanel.SetActive(false);
    }



    public void SetBrightness(float brightness)
    {
        float normalizedBrightness = brightness / brightnessSlider.maxValue;
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