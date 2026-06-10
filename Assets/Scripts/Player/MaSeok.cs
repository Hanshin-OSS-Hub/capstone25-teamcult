using UnityEngine;

public class MaSeok : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (OopartsTreeManager.instance != null)
                OopartsTreeManager.instance.AddPoint(1);

            if (LogManager.Instance != null)
                LogManager.Instance.AddLog("마석을 획득했습니다. O키를 눌러 능력을 해금하세요.");

            if (SaveManager.instance != null)
                SaveManager.instance.SaveRun();

            Destroy(gameObject);
        }
    }
}