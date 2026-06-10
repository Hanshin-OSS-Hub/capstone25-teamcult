using UnityEngine;

public class SettingsPanelManager : MonoBehaviour
{
    public GameObject settingsPanel;
    private Animator animator;
    private bool isOpen = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        if (settingsPanel != null)
        {
            animator = settingsPanel.GetComponent<Animator>();
        }
        if (!isOpen && settingsPanel != null) settingsPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (!isOpen) OpenSettings();
            else CloseSettings();
        }
    }

    public void OpenSettings()
    {
        gameObject.SetActive(true);
        animator.SetTrigger("Open");
        isOpen = true;
    }

    public void CloseSettings()
    {
        if (isOpen)
        {
            animator.SetTrigger("Close");
            isOpen = false;
            Invoke("DisablePanel", 0.3f);
            animator.ResetTrigger("Open");
            animator.SetTrigger("Close");
            Invoke("DisablePanel", 0.5f);
        }
    }

    private void DisablePanel()
    {
        if (!isOpen) gameObject.SetActive(false);
    }

    public void SaveGame()
    {
        if (SaveManager.instance != null)
        {
            SaveManager.instance.SaveRun();
            Debug.Log("[Settings] 수동 저장 완료");
        }
    }
}