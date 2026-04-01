using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("UI References")]
    public Button btnStart;
    public Button btnQuit;

    [Header("Scene Name")]
    public string gameSceneName = "createMap";

    void Awake()
    {
        if (btnStart != null)
            btnStart.onClick.AddListener(OnClickStart);

        if (btnQuit != null)
            btnQuit.onClick.AddListener(OnClickQuit);
    }

    public void OnClickStart()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnClickQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}