using UnityEngine;
using UnityEngine.UI;

public class WeaponSwapUI : MonoBehaviour
{
    [Header("Weapon Slots")]
    public Image weaponSlot1; // 1번 슬롯 UI 이미지
    public Image weaponSlot2; // 2번 슬롯 UI 이미지

    [Header("Weapon Sprites")]
    public Sprite weapon1Sprite; // 1번 무기 스프라이트
    public Sprite weapon2Sprite; // 2번 무기 스프라이트

    [Header("Visual Settings")]
    public Color activeColor = Color.white; // 활성화 시 색상 (선명함)
    public Color inactiveColor = new Color(0.6f, 0.6f, 0.6f, 0.8f); // 비활성화 시 색상 (어두움)
    public Vector3 activeScale = Vector3.one * 1.2f; // 활성화 시 크기 (1.2배)
    public Vector3 inactiveScale = Vector3.one;      // 비활성화 시 크기 (기본)

    [Header("Input")]
    public KeyCode swapKey = KeyCode.Q; // 무기 교체 키

    // 현재 1번 무기가 활성화되어 있는지 추적
    private bool isWeapon1Active = true;

    void Start()
    {
        // 1. 초기 무기 스프라이트 할당
        weaponSlot1.sprite = weapon1Sprite;
        weaponSlot2.sprite = weapon2Sprite;

        // 2. 초기 UI 상태 설정 (1번 활성화)
        UpdateWeaponUI();
    }

    void Update()
    {
        // 1번 키를 눌렀을 때
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (!isWeapon1Active) // 1번이 활성화가 아닐 때만 실행
            {
                SetActiveWeapon(true);
            }
        }
        // 2번 키를 눌렀을 때
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (isWeapon1Active) // 2번이 활성화가 아닐 때만 실행
            {
                SetActiveWeapon(false);
            }
        }
        // 교체 키(Q)를 눌렀을 때
        else if (Input.GetKeyDown(swapKey))
        {
            SwapWeapons();
        }
    }

    // 무기 활성화/비활성화 (1, 2번 키)
    void SetActiveWeapon(bool activateWeapon1)
    {
        isWeapon1Active = activateWeapon1;
        UpdateWeaponUI();
        // (여기에 실제 플레이어의 무기 교체 로직을 연동)
    }

    // 무기 교체 (Q 키)
    void SwapWeapons()
    {
        isWeapon1Active = !isWeapon1Active; // 상태 반전
        UpdateWeaponUI();
        // (여기에 실제 플레이어의 무기 교체 로직을 연동)
    }

    // UI 시각 효과 업데이트
    void UpdateWeaponUI()
    {
        if (isWeapon1Active)
        {
            // 1번 활성화, 2번 비활성화
            weaponSlot1.color = activeColor;
            weaponSlot1.transform.localScale = activeScale;

            weaponSlot2.color = inactiveColor;
            weaponSlot2.transform.localScale = inactiveScale;
        }
        else
        {
            // 1번 비활성화, 2번 활성화
            weaponSlot1.color = inactiveColor;
            weaponSlot1.transform.localScale = inactiveScale;

            weaponSlot2.color = activeColor;
            weaponSlot2.transform.localScale = activeScale;
        }
    }
}