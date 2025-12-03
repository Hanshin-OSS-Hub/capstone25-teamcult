using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // 씬 관리를 위해 필요합니다.

public class MenuManager : MonoBehaviour {
    public Image backgroundImage;

    // 메인 배경으로 사용할 Sprite (유니티 인스펙터 창에서 할당)
    public Sprite mainBackgroundSprite;

    // (선택 사항) 설정을 누르면 바뀔 배경 Sprite
    public Sprite settingsBackgroundSprite;
    private void Start()
    {
        // 씬이 시작될 때 배경 이미지를 메인 배경으로 초기화합니다.
        if (backgroundImage != null && mainBackgroundSprite != null)
        {
            backgroundImage.sprite = mainBackgroundSprite;
        }
    }

    public void StartGame() {
        LoadSceneByName("demo");
    }

    // 씬 이동
    public void LoadSceneByName(string sceneName) {
        SceneManager.LoadScene(sceneName);
    }

    // 게임을 종료하는 함수입니다.
    public void ExitGame() {
// 유니티 에디터에서 플레이 중일 때
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
// 빌드된 애플리케이션일 때
#else
        Application.Quit();
#endif
    }
}