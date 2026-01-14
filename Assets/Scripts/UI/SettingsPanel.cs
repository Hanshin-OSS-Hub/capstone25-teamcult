using UnityEngine;

public class SettingsPanelManager : MonoBehaviour
{
    public GameObject settingsPanel;
    private Animator animator;
    private bool isOpen = false;
    void Start()
    {
        // SettingsPanel에 붙어있는 Animator를 가져옵니다.
        if (settingsPanel != null)
        {
            animator = settingsPanel.GetComponent<Animator>();
        }

        // 처음에 창이 꺼져있다면
        if (!isOpen && settingsPanel != null) settingsPanel.SetActive(false);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isOpen) OpenSettings();
            else CloseSettings();
        }
    }
    void Awake()
    {
        animator = GetComponent<Animator>();
        // 시작할 때 꺼져있어야 하므로 비활성화 (선택 사항)
        // transform.localScale = Vector3.zero; 
    }

    public void OpenSettings()
    {
        gameObject.SetActive(true); // 오브젝트를 켭니다.
        animator.SetTrigger("Open");
        isOpen = true;
    }

    public void CloseSettings()
    {
        if (isOpen)
        {
            animator.SetTrigger("Close");
            isOpen = false;
            // 애니메이션이 끝난 후 비활성화하고 싶다면 Invoke 사용
            Invoke("DisablePanel", 0.3f);
            animator.ResetTrigger("Open");
            animator.SetTrigger("Close");

            // 애니메이션이 완전히 끝날 시간을 고려해 끕니다. (Has Exit Time과 맞춤)
            Invoke("DisablePanel", 0.5f);
        }
    }

    private void DisablePanel()
    {
        if (!isOpen) gameObject.SetActive(false);
    }
}