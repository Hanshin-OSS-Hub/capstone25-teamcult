using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HeartAbilityManager : MonoBehaviour
{
    [Header("설정")]
    public Sprite normalHeart; 

    private PlayerHealth playerHealth;
    private bool isAbilityActive = false;
    private float savedHealth = 0;
    private Coroutine effectRoutine; 

    void Start()
    {
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth == null) Debug.LogError("? PlayerHealth 스크립트를 찾을 수 없습니다!");
    }

    void Update()
    {
        if (isAbilityActive && playerHealth != null)
        {
            if (playerHealth.currentHealth < savedHealth)
            {
                DeactivateAbility();
            }
        }
    }

    public void ActivateAbility(string type)
    {
        if (playerHealth == null) return;

        isAbilityActive = true;
        savedHealth = playerHealth.currentHealth;

        if (effectRoutine != null) StopCoroutine(effectRoutine);

        if (type == "Fire")
        {
            effectRoutine = StartCoroutine(FireEffect());
        }
        else if (type == "Ice")
        {
            effectRoutine = StartCoroutine(IceEffect());
        }
    }

    public void DeactivateAbility()
    {
        isAbilityActive = false;
        if (effectRoutine != null) StopCoroutine(effectRoutine);

        if (playerHealth != null && playerHealth.hearts != null)
        {
            foreach (Image img in playerHealth.hearts)
            {
                if (img != null)
                {
                    img.sprite = normalHeart;       
                    img.color = Color.white;        
                    img.transform.localScale = Vector3.one; 
                }
            }
        }
    }

    IEnumerator FireEffect()
    {
        Color fireRed = new Color(0.9f, 0.1f, 0f);
        Color fireOrange = new Color(1f, 0.6f, 0f);
        Color fireYellow = new Color(1f, 1f, 0.2f);

        while (true)
        {
            if (playerHealth.hearts != null)
            {
                float t = Time.time * 15f;

                foreach (Image img in playerHealth.hearts)
                {
                    if (img == null) continue;

                    float offset = img.GetInstanceID() * 0.1f;

                    float noiseVal = Mathf.PerlinNoise(t, offset);

                    if (noiseVal < 0.4f) 
                    {
                        img.color = Color.Lerp(fireRed, fireOrange, noiseVal * 2.5f);
                    }
                    else 
                    {
                        img.color = Color.Lerp(fireOrange, fireYellow, (noiseVal - 0.4f) * 1.6f);
                    }

                    float scaleNoise = Mathf.PerlinNoise(t + 50f, offset);
                    float scale = 1.05f + (scaleNoise * 0.2f);

                    img.transform.localScale = new Vector3(scale, scale, 1f);
                }
            }
            yield return null; 
        }
    }

    IEnumerator IceEffect()
    {
        while (true)
        {
            if (playerHealth.hearts != null)
            {
                foreach (Image img in playerHealth.hearts)
                {
                    if (img == null) continue;

                    img.color = Color.Lerp(Color.cyan, Color.blue, Mathf.PingPong(Time.time * 2f, 0.3f));

                    float scale = 1.0f + Mathf.Sin(Time.time * 2f) * 0.05f;
                    img.transform.localScale = new Vector3(scale, scale, 1f);
                }
            }
            yield return null;
        }
    }
}