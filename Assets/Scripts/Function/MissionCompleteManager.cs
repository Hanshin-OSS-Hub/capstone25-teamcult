using UnityEngine;
using UnityEngine.SceneManagement;

public class MissionCompleteManager : MonoBehaviour
{
    public void GoToMainMenu()
    {
        Debug.Log("메인 메뉴로 이동합니다.");
        SceneManager.LoadScene("MainMenu");
    }

    public void ExitGame()
    {
        Debug.Log("게임을 종료합니다.");
    }
}