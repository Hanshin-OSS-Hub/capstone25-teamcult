using UnityEngine;

public class ElementalItem : MonoBehaviour
{
    [Header("Item Settings")]
    public string elementType = "Fire";
    public Item heartItem;

    private bool isPlayerInRange = false;
    private bool hasBeenPickedUp = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isPlayerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isPlayerInRange = false;
    }

    void Update()
    {
        if (!hasBeenPickedUp && isPlayerInRange && Input.GetKeyDown(KeyCode.Z))
        {
            Pickup();
        }
    }

    void Pickup()
    {
        if (heartItem == null)
        {
            Debug.LogError("heartItem이 연결되지 않았습니다!");
            return;
        }

        if (TabController.instance.AddItem(heartItem.Clone()))
        {
            hasBeenPickedUp = true;

            if (elementType == "Fire")
            {
                SFXManager.Instance.PlaySFX(SFXType.HeartObtain_Fire);
                LogManager.Instance.AddLog("화염 하트를 획득했습니다.");

                GameObject pfxObj = new GameObject("AshPFX");
                pfxObj.transform.position = transform.position;
                HeartPickupParticle pfx = pfxObj.AddComponent<HeartPickupParticle>();
                pfx.Play(transform.position);
                Destroy(pfxObj, 3f);
            }
            else if (elementType == "Ice")
            {
                SFXManager.Instance.PlaySFX(SFXType.HeartObtain_Ice);
                LogManager.Instance.AddLog("빙결 하트를 획득했습니다.");
            }
            else if (elementType == "Lightning")
            {
                SFXManager.Instance.PlaySFX(SFXType.HeartObtain_Lightning);
                LogManager.Instance.AddLog("번개 하트를 획득했습니다.");
            }

            Destroy(gameObject);
        }
        else
        {
            Debug.Log("인벤토리가 가득 찼습니다!");
        }
    }
}