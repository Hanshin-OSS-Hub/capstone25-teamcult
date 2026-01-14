using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio; // 오디오 믹서 제어를 위해 추가

public class GameSettingManager : MonoBehaviour
{
    [Header("Enemy Volume Settings")]
    public Image volumeFillImage;   // 룬 문자 Fill Image
    public AudioMixer enemyMixer;    // 적 소리 제어용 믹서
    public Color volLowColor = new Color(0.3f, 0, 0); // 어두운 빨강
    public Color volHighColor = Color.red;            // 밝은 빨강

    [Header("Map Brightness Settings")]
    public Image brightnessFillImage; // 눈동자 Fill Image
    public Image screenOverlay;       // 화면을 덮는 검은색 패널 (Alpha 조절용)
    public Color brightMinColor = new Color(0.2f, 0.2f, 0.2f); // 어두운 철
    public Color brightMaxColor = new Color(0.8f, 0.7f, 0.6f); // 밝은 철

    // 슬라이더나 외부 노브 핸들에서 호출 (0 ~ 1 사이 값)
    public void UpdateEnemyVolume(float value)
    {
        // 1. UI 시각화 (차오르는 연출 + 색상 변경)
        volumeFillImage.fillAmount = value;
        volumeFillImage.color = Color.Lerp(volLowColor, volHighColor, value);

        // 2. 실제 오디오 믹서 값 조절 (데시벨 변환 로직)
        float dB = Mathf.Log10(Mathf.Max(0.0001f, value)) * 20f;
        enemyMixer.SetFloat("EnemyVol", dB);

        Debug.Log("입력된 값: " + value); // 콘솔창에 숫자가 뜨는지 확인
        volumeFillImage.fillAmount = value; // 이 줄이 실행되어야 이미지가 차오릅니다.
    }

    public void UpdateMapBrightness(float value)
    {
        // 1. UI 시각화 (차오르는 연출 + 색상 변경)
        brightnessFillImage.fillAmount = value;
        brightnessFillImage.color = Color.Lerp(brightMinColor, brightMaxColor, value);

        // 2. 실제 화면 밝기(Overlay Alpha) 조절
        if (screenOverlay != null)
        {
            // 노브가 1(최대)이면 Alpha가 0(투명), 노브가 0(최소)이면 Alpha가 0.8(어두움)
            float alpha = (1f - value) * 0.8f;
            Color c = screenOverlay.color;
            c.a = alpha;
            screenOverlay.color = c;
        }
    }
}