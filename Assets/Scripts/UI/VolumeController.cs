using UnityEngine;
using UnityEngine.UI; // UI 컴포넌트를 제어하기 위해 필요합니다.

public class VolumeController : MonoBehaviour
{
    [Header("UI Reference")]
    public Slider volumeSlider; // 유니티 에디터에서 드래그 앤 드롭으로 연결할 슬라이더

    void Start()
    {
        // 1. 게임 시작 시 이전 볼륨 설정을 불러옵니다 (저장된 게 없으면 1.0으로 설정)
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);

        // 2. 슬라이더의 현재 값을 불러온 볼륨값으로 맞춥니다.
        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
            // 슬라이더 값이 바뀔 때 실행될 함수를 등록합니다.
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        // 3. 실제 오디오 리스너의 볼륨을 설정합니다.
        AudioListener.volume = savedVolume;
    }

    // 슬라이더를 움직일 때마다 호출되는 함수입니다.
    public void SetVolume(float volume)
    {
        // 전체 시스템 볼륨 변경 (0.0 ~ 1.0 사이 값)
        AudioListener.volume = volume;

        // 현재 설정한 볼륨값을 저장 (게임을 껐다 켜도 유지됨)
        PlayerPrefs.SetFloat("MasterVolume", volume);

        Debug.Log($"현재 볼륨: {volume * 100}%");
    }
}