using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리를 위해 필요합니다.

public class MenuManager : MonoBehaviour {
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