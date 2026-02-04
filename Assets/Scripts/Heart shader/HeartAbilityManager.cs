using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HeartAbilityManager : MonoBehaviour
{
    [Header("설정")]
    public Sprite normalHeart; // 원래 기본 하트 이미지 (꼭 넣어주세요!)

    // 내부 변수들
    private PlayerHealth playerHealth;
    private bool isAbilityActive = false;
    private float savedHealth = 0;
    private Coroutine effectRoutine; // 효과를 관리하는 변수

    void Start()
    {
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth == null) Debug.LogError("? PlayerHealth 스크립트를 찾을 수 없습니다!");
    }

    void Update()
    {
        // 능력이 켜져있는데 체력이 깎이면 능력 해제
        if (isAbilityActive && playerHealth != null)
        {
            if (playerHealth.currentHealth < savedHealth)
            {
                DeactivateAbility();
            }
        }
    }

    // 아이템 먹었을 때 호출
    public void ActivateAbility(string type)
    {
        if (playerHealth == null) return;

        isAbilityActive = true;
        savedHealth = playerHealth.currentHealth;

        // 이미 실행 중인 효과가 있다면 끄기
        if (effectRoutine != null) StopCoroutine(effectRoutine);

        if (type == "Fire")
        {
            // ?? 불꽃 효과 코루틴 시작 (활활 타오르는 버전)
            effectRoutine = StartCoroutine(FireEffect());
        }
        else if (type == "Ice")
        {
            // ?? 얼음 효과 코루틴 시작
            effectRoutine = StartCoroutine(IceEffect());
        }
    }

    // 능력 해제 (원래대로 복구)
    public void DeactivateAbility()
    {
        isAbilityActive = false;
        if (effectRoutine != null) StopCoroutine(effectRoutine);

        if (playerHealth != null && playerHealth.hearts != null)
        {
            foreach (Image img in playerHealth.hearts)
            {
                if (img != null)
                {
                    img.sprite = normalHeart;       // 그림 복구
                    img.color = Color.white;        // 색깔 하얗게 복구
                    img.transform.localScale = Vector3.one; // 크기 원래대로 (1,1,1)
                }
            }
        }
    }

    // ?? 수정된 불꽃 효과: 활활 타오르는 느낌 (불규칙한 떨림과 색상 변화)
    IEnumerator FireEffect()
    {
        // 불꽃 색상 정의 (진한 빨강, 밝은 주황, 강렬한 노랑)
        Color fireRed = new Color(0.9f, 0.1f, 0f);
        Color fireOrange = new Color(1f, 0.6f, 0f);
        Color fireYellow = new Color(1f, 1f, 0.2f);

        while (true)
        {
            if (playerHealth.hearts != null)
            {
                // *중요* Time.time을 변수에 담아 조금 빠르게 흐르게 합니다.
                float t = Time.time * 15f;

                foreach (Image img in playerHealth.hearts)
                {
                    if (img == null) continue;

                    // 하트마다 약간 다른 타이밍을 주기 위한 오프셋 (GetInstanceID 사용)
                    float offset = img.GetInstanceID() * 0.1f;

                    // --- 1. 색깔: 불규칙하게 타오름 (노이즈 사용) ---
                    // PerlinNoise는 0~1 사이의 불규칙한 값을 만듭니다.
                    float noiseVal = Mathf.PerlinNoise(t, offset);

                    if (noiseVal < 0.4f) // 어두운 부분 (빨강~주황)
                    {
                        img.color = Color.Lerp(fireRed, fireOrange, noiseVal * 2.5f);
                    }
                    else // 밝은 부분 (주황~노랑) - 여기가 순간적으로 번쩍임
                    {
                        img.color = Color.Lerp(fireOrange, fireYellow, (noiseVal - 0.4f) * 1.6f);
                    }

                    // --- 2. 크기: 두근거림(Sin) 대신 불규칙한 떨림(Jitter) ---
                    // 노이즈를 이용해 크기가 1.05배에서 1.25배 사이를 불규칙하게 오갑니다.
                    // t에 50f를 더해 색깔 변화와는 조금 다른 패턴으로 움직이게 함
                    float scaleNoise = Mathf.PerlinNoise(t + 50f, offset);
                    float scale = 1.05f + (scaleNoise * 0.2f);

                    img.transform.localScale = new Vector3(scale, scale, 1f);
                }
            }
            yield return null; // 매 프레임 반복
        }
    }

    // ?? 얼음 효과: 시원한 하늘색 + 천천히 숨쉬는 느낌
    IEnumerator IceEffect()
    {
        while (true)
        {
            if (playerHealth.hearts != null)
            {
                foreach (Image img in playerHealth.hearts)
                {
                    if (img == null) continue;

                    // 1. 색깔: 하늘색(Cyan)으로 고정하되 약간 밝기 조절
                    img.color = Color.Lerp(Color.cyan, Color.blue, Mathf.PingPong(Time.time * 2f, 0.3f));

                    // 2. 크기: 아주 천천히 움직임
                    float scale = 1.0f + Mathf.Sin(Time.time * 2f) * 0.05f;
                    img.transform.localScale = new Vector3(scale, scale, 1f);
                }
            }
            yield return null;
        }
    }
}