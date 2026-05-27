using UnityEngine;
public class MaSeok : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (OopartsTreeManager.instance != null)
                OopartsTreeManager.instance.AddPoint(1);
            Destroy(gameObject);
        }
    }
}