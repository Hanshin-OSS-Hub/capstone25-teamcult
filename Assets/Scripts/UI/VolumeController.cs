using UnityEngine;
using UnityEngine.UI; 

public class VolumeController : MonoBehaviour
{
    [Header("UI Reference")]
    public Slider volumeSlider; 

    void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);

        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        AudioListener.volume = savedVolume;
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;

        PlayerPrefs.SetFloat("MasterVolume", volume);

        Debug.Log($"현재 볼륨: {volume * 100}%");
    }
}