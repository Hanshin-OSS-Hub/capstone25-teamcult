using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightFlicker : MonoBehaviour
{
    private Light2D myLight; 
    public float minIntensity = 0.8f; 
    public float maxIntensity = 1.2f; 
    public float flickerSpeed = 10f;  

    void Start()
    {
        myLight = GetComponent<Light2D>();
    }

    void Update()
    {
        if (myLight == null) return;

        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
        myLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
    }
}