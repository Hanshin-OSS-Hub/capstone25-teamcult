using UnityEngine;

public class ElementalItem : MonoBehaviour
{
    [Header("Item Settings")]
    [Tooltip("Type exactly: Fire, Ice, or Poison")]
    public string elementType = "Fire";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (elementType == "Fire") { 
                SFXManager.Instance.PlaySFX(SFXType.HeartObtain_Fire);
                LogManager.Instance.AddLog($"È­¿° ÇÏÆ®¸¦ È¹µæÇß½À´Ï´Ù.");
            }
            else if (elementType == "Ice") { 
                SFXManager.Instance.PlaySFX(SFXType.HeartObtain_Ice);
                LogManager.Instance.AddLog($"ºù°á ÇÏÆ®¸¦ È¹µæÇß½À´Ï´Ù.");
            }
            else if (elementType == "Lightning") { 
                SFXManager.Instance.PlaySFX(SFXType.HeartObtain_Lightning);
                LogManager.Instance.AddLog($"¹ø°³ ÇÏÆ®¸¦ È¹µæÇß½À´Ï´Ù.");
            }

            ElementalManager manager = FindFirstObjectByType<ElementalManager>();

            if (manager != null)
            {
                manager.ActivateAbility(elementType);
                HeartSlotController.instance.SetHeart(elementType); 
            }

           
            if (elementType == "Fire")
            {
                GameObject pfxObj = new GameObject("AshPFX");
                pfxObj.transform.position = transform.position;
                HeartPickupParticle pfx = pfxObj.AddComponent<HeartPickupParticle>();
                pfx.Play(transform.position);
                Destroy(pfxObj, 3f);
            }

            Destroy(gameObject);
        }
    }
}