using UnityEngine;
using UnityEngine.Rendering.Universal; 

public class FlashlightController : MonoBehaviour
{
    [Header("Settings")]
    public float angleOffset = -90f; 
    public KeyCode toggleKey = KeyCode.F; 

    private Light2D myLight; 

    void Start()
    {
        myLight = GetComponent<Light2D>();

        if (myLight != null)
        {
            myLight.enabled = false;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (myLight != null)
            {
                myLight.enabled = !myLight.enabled;
            }
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        if (h != 0 || v != 0)
        {
            float angle = Mathf.Atan2(v, h) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle + angleOffset));
        }
    }
}