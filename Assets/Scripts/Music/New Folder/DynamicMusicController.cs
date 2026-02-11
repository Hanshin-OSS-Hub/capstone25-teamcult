using UnityEngine;

public class DynamicMusicController : MonoBehaviour
{
    // 1. 오디오 레이어 연결
    public AudioSource audio_Kick;
    public AudioSource audio_Bass;
    public AudioSource audio_Synth;
    public AudioSource audio_HiPerc;

    // 2. 위협 수준(면적 합계) 임계값
    public float bassThreshold = 1.0f;
    public float synthThreshold = 5.0f;
    public float hiPercThreshold = 10.0f;
    
    // 3. 페이드 속도
    public float fadeSpeed = 1.5f;

    private float currentThreat = 0.0f;
    private Camera mainCamera; // ◀◀ 카메라를 담을 변수 추가

    void Start()
    {
        // "MainCamera" 태그가 붙은 카메라를 자동으로 찾아옵니다.
        mainCamera = Camera.main; // ◀◀ 이 줄 추가

        // 킥은 볼륨 1, 나머지는 0으로 시작
        if (audio_Kick != null) audio_Kick.volume = 1.0f;
        if (audio_Bass != null) audio_Bass.volume = 0.0f;
        if (audio_Synth != null) audio_Synth.volume = 0.0f;
        if (audio_HiPerc != null) audio_HiPerc.volume = 0.0f;
    }

    void Update()
    {
        // mainCamera를 찾지 못했으면 오류 방지
        if (mainCamera == null) return; 

        currentThreat = CalculateTotalThreat();
        UpdateMusicLayers();
    }

    // "Enemy" 태그가 붙은 오브젝트 중 '화면에 보이는 것'의 면적(x*y)만 합산
    private float CalculateTotalThreat()
    {
        float totalThreat = 0;
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemyObject in enemies)
        {
            if (enemyObject.activeInHierarchy)
            {
                // ◀◀ --- '화면 안에 있는지' 검사 로직 시작 ---
                
                // 1. 적의 월드 위치를 뷰포트 (0~1) 좌표로 변환
                Vector3 viewportPos = mainCamera.WorldToViewportPoint(enemyObject.transform.position);

                // 2. (x, y)가 0과 1 사이이고, 카메라 앞에(z > 0) 있는지 확인
                bool isVisible = viewportPos.x >= 0 && viewportPos.x <= 1 &&
                                 viewportPos.y >= 0 && viewportPos.y <= 1 &&
                                 viewportPos.z > 0;

                // ◀◀ --- 검사 로직 끝 ---

                // 3. '화면에 보일 때만' 위협 수준을 더함
                if (isVisible) 
                {
                    Vector3 scale = enemyObject.transform.localScale;
                    float threatFromSize = scale.x * scale.y;
                    totalThreat += threatFromSize;
                }
            }
        }
        return totalThreat;
    }

    // 계산된 위협 수준에 따라 음악 볼륨 조절
    private void UpdateMusicLayers()
    {
        if (audio_Kick != null) audio_Kick.volume = 1.0f;

        float targetBassVolume = (currentThreat >= bassThreshold) ? 1.0f : 0.0f;
        float targetSynthVolume = (currentThreat >= synthThreshold) ? 1.0f : 0.0f;
        float targetHiPercVolume = (currentThreat >= hiPercThreshold) ? 1.0f : 0.0f;

        if (audio_Bass != null)
            audio_Bass.volume = Mathf.Lerp(audio_Bass.volume, targetBassVolume, Time.deltaTime * fadeSpeed);
        
        if (audio_Synth != null)
            audio_Synth.volume = Mathf.Lerp(audio_Synth.volume, targetSynthVolume, Time.deltaTime * fadeSpeed);
        
        if (audio_HiPerc != null)
            audio_HiPerc.volume = Mathf.Lerp(audio_HiPerc.volume, targetHiPercVolume, Time.deltaTime * fadeSpeed);
    }
}