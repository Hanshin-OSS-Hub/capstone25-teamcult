using UnityEngine;
public class MaSeok : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (OopartsTreeManager.instance != null)
                OopartsTreeManager.instance.AddPoint(1);

            // 마석 먹는 순간 저장 → 게임오버 시 Continue 활성화됨
            if (SaveManager.instance != null)
                SaveManager.instance.SaveRun();

            Destroy(gameObject);
        }
    }
}