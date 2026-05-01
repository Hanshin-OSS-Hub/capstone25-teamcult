using UnityEngine;

public class MaSeok : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 포인트 +1
            OopartsTreeManager.instance.AddPoint(1);

            // 마석 오브젝트 제거
            Destroy(gameObject);
        }
    }
}