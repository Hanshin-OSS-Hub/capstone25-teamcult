using UnityEngine;
using UnityEngine.UI;

public class WeaponSwapUI : MonoBehaviour
{
    [Header("Weapon Slots")]
    public Image weaponSlot1; 
    public Image weaponSlot2; 

    [Header("Weapon Sprites")]
    public Sprite weapon1Sprite; 
    public Sprite weapon2Sprite; 

    [Header("Visual Settings")]
    public Color activeColor = Color.white; 
    public Color inactiveColor = new Color(0.6f, 0.6f, 0.6f, 0.8f); 
    public Vector3 activeScale = Vector3.one * 1.2f; 
    public Vector3 inactiveScale = Vector3.one;     

    [Header("Input")]
    public KeyCode swapKey = KeyCode.Q; 

    private bool isWeapon1Active = true;

    void Start()
    {
        weaponSlot1.sprite = weapon1Sprite;
        weaponSlot2.sprite = weapon2Sprite;

        UpdateWeaponUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (!isWeapon1Active) 
            {
                SetActiveWeapon(true);
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (isWeapon1Active) 
            {
                SetActiveWeapon(false);
            }
        }
        else if (Input.GetKeyDown(swapKey))
        {
            SwapWeapons();
        }
    }

    void SetActiveWeapon(bool activateWeapon1)
    {
        isWeapon1Active = activateWeapon1;
        UpdateWeaponUI();
    }

    void SwapWeapons()
    {
        isWeapon1Active = !isWeapon1Active; 
        UpdateWeaponUI();
    }

    void UpdateWeaponUI()
    {
        if (isWeapon1Active)
        {
            weaponSlot1.color = activeColor;
            weaponSlot1.transform.localScale = activeScale;

            weaponSlot2.color = inactiveColor;
            weaponSlot2.transform.localScale = inactiveScale;
        }
        else
        {
            weaponSlot1.color = inactiveColor;
            weaponSlot1.transform.localScale = inactiveScale;

            weaponSlot2.color = activeColor;
            weaponSlot2.transform.localScale = activeScale;
        }
    }
}