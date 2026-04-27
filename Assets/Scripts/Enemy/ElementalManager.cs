using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using System.Collections;

public class ElementalManager : MonoBehaviour
{
    [Header("References")]
    public PlayerHealth playerHealth;
    public Image screenEffectImage;
    public Camera mainCamera;
    public Transform playerTransform;

    [Header("Elemental Heart Sprites")]
    public Sprite[] fireHeartSprites;
    public Sprite[] iceHeartSprites;
    public Sprite[] poisonHeartSprites;
    public Sprite[] lightningHeartSprites;
    public Sprite[] holyHeartSprites;
    public Sprite[] grassHeartSprites;

    [Header("General Settings")]
    public Sprite defaultHeartSprite;
    public float animSpeed = 10f;

    [Header("Elemental Status")]
    public bool hasFireHeart = false;
    public bool hasIceHeart = false;
    public bool hasLightningHeart = false;

    [Header("Noise Texture")]
    public Texture2D noiseTex;

    private Tilemap[] allMaps;
    private float savedHealth;
    private bool isAbilityActive = false;
    private Material screenMat;
    private string currentType = "";
    private bool isChainFlashing = false;

    void Start()
    {
        if (screenEffectImage != null)
        {
            screenEffectImage.gameObject.SetActive(false);
            if (screenEffectImage.material != null)
            {
                screenMat = new Material(screenEffectImage.material);
                screenEffectImage.material = screenMat;
                if (noiseTex == null)
                    noiseTex = screenMat.GetTexture("_NoiseTex") as Texture2D;
            }
        }
        if (defaultHeartSprite == null && playerHealth != null && playerHealth.hearts.Length > 0)
            if (playerHealth.hearts[0] != null) defaultHeartSprite = playerHealth.hearts[0].sprite;

        allMaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
        if (mainCamera == null) mainCamera = Camera.main;
    }

    void Update()
    {
        if (isAbilityActive)
            if (playerHealth.currentHealth <= savedHealth - 2.0f) DeactivateAbility();

        if (screenMat != null && playerTransform != null && mainCamera != null)
        {
            Vector3 vp = mainCamera.WorldToViewportPoint(playerTransform.position);
            screenMat.SetVector("_PlayerPos", new Vector4(vp.x, vp.y, 0, 0));
        }
    }

    public void ActivateAbility(string type)
    {
        if (isAbilityActive && currentType == type) { Debug.Log($"[{type}] 이미 활성화 됨"); return; }
        if (isAbilityActive && currentType != type) DeactivateAbility();

        if (type == "Fire") hasFireHeart = true;
        if (type == "Ice") hasIceHeart = true;
        if (type == "Lightning") hasLightningHeart = true;

        if (BattleStateBGM.Instance != null) BattleStateBGM.Instance.ApplyElementalEffect(type);

        if (playerHealth == null) return;
        allMaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
        isAbilityActive = true;
        currentType = type;
        savedHealth = playerHealth.currentHealth;

        if (screenEffectImage != null) screenEffectImage.gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(AbilityLoop(type));
    }

    public void DeactivateAbility()
    {
        if (!isAbilityActive) return;
        isAbilityActive = false;
        currentType = "";
        hasFireHeart = false;
        hasIceHeart = false;
        hasLightningHeart = false;

        if (BattleStateBGM.Instance != null) BattleStateBGM.Instance.ClearElementalEffect();

        if (screenEffectImage != null) screenEffectImage.gameObject.SetActive(false);
        if (playerHealth.hearts != null)
            foreach (var img in playerHealth.hearts)
                if (img != null)
                {
                    img.color = Color.white;
                    img.transform.localScale = Vector3.one;
                    img.transform.localRotation = Quaternion.identity;
                    if (defaultHeartSprite != null) img.sprite = defaultHeartSprite;
                }
        if (allMaps != null)
            foreach (var map in allMaps)
                if (map != null) map.color = Color.white;
    }

    // ── 체인 번개 번쩍임 (노란 테마) ─────────────────────────────
    public void TriggerLightningFlash()
    {
        if (isChainFlashing) return;
        StartCoroutine(LightningChainFlash());
    }

    IEnumerator LightningChainFlash()
    {
        isChainFlashing = true;

        bool wasActive = screenEffectImage != null && screenEffectImage.gameObject.activeSelf;
        if (screenEffectImage != null) screenEffectImage.gameObject.SetActive(true);

        if (screenMat != null)
        {
            screenMat.SetFloat("_EffectType", 3f);
            screenMat.SetFloat("_LightningFlash", 1.0f);
            screenMat.SetFloat("_LightningStrike", 1f);
            screenMat.SetFloat("_Progress", 1f);
            screenMat.SetFloat("_Radius", 0.3f);
            screenMat.SetColor("_CoreColor", new Color(1.0f, 1.0f, 0.8f)); // 흰/노랑
            screenMat.SetColor("_EdgeColor", new Color(1.0f, 0.6f, 0.0f)); // 주황
            screenMat.SetFloat("_BoomFlash", 0.5f);
        }

        // 타일맵 노란/흰 번쩍
        Color flashColor = new Color(2.0f, 1.8f, 0.8f);
        if (allMaps != null)
            foreach (var map in allMaps)
                if (map != null) map.color = flashColor;

        yield return new WaitForSeconds(0.05f);

        float t = 0f;
        float duration = 0.12f;

        Color targetMapColor = isAbilityActive && currentType == "Lightning"
            ? Color.Lerp(Color.white, new Color(1.0f, 0.6f, 0.0f, 0.85f), 0.25f)
            : Color.white;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;

            if (screenMat != null)
            {
                screenMat.SetFloat("_LightningFlash", Mathf.Lerp(1f, 0f, p));
                screenMat.SetFloat("_BoomFlash", Mathf.Lerp(0.5f, 0f, p));
                screenMat.SetFloat("_Radius", Mathf.Lerp(0.3f, 0.10f, p));
            }

            if (allMaps != null)
                foreach (var map in allMaps)
                    if (map != null) map.color = Color.Lerp(flashColor, targetMapColor, p);

            yield return null;
        }

        if (screenMat != null)
        {
            screenMat.SetFloat("_LightningFlash", 0f);
            screenMat.SetFloat("_BoomFlash", 0f);
        }

        if (!wasActive && screenEffectImage != null)
            screenEffectImage.gameObject.SetActive(false);

        isChainFlashing = false;
    }

    float GetMapTintStrength(string type)
    {
        if (type == "Lightning") return 0.25f;
        if (type == "Fire") return 0.20f;
        if (type == "Ice") return 0.20f;
        if (type == "Poison") return 0.25f;
        if (type == "Holy") return 0.35f;
        if (type == "Grass") return 0.30f;
        return 0.15f;
    }

    IEnumerator AbilityLoop(string type)
    {
        float effectType = 0f, targetRadius = 0f, targetSoftness = 0.3f, distortPower = 0.1f;
        Color targetCore = Color.white, targetEdge = Color.white;
        Vector2 scrollSpeed = Vector2.zero;

        if (type == "Fire")
        {
            effectType = 0f; targetCore = new Color(1f, 0.8f, 0.1f, 1f); targetEdge = new Color(1f, 0.2f, 0f, 1f);
            scrollSpeed = new Vector2(0.1f, 1.5f); targetRadius = 0.2f; targetSoftness = 0.05f; distortPower = 1.2f;
        }
        else if (type == "Ice")
        {
            effectType = 1f; targetCore = new Color(0.8f, 0.97f, 1f, 0.9f); targetEdge = new Color(0.3f, 0.7f, 1f, 0.85f);
            scrollSpeed = new Vector2(0.02f, 0.05f); targetRadius = 0.88f; targetSoftness = 0.08f; distortPower = 0.35f;
        }
        else if (type == "Poison")
        {
            effectType = 2f; targetCore = new Color(0.35f, 0.55f, 0.15f, 0.6f); targetEdge = new Color(0.15f, 0.05f, 0.25f, 0.6f);
            scrollSpeed = new Vector2(0.06f, 0.1f); targetRadius = 0.18f; targetSoftness = 0.3f; distortPower = 0.18f;
        }
        else if (type == "Lightning")
        {
            effectType = 3f;
            targetCore = new Color(1.0f, 1.0f, 0.8f, 1.0f); // 흰/노랑
            targetEdge = new Color(1.0f, 0.6f, 0.0f, 0.85f); // 주황
            scrollSpeed = new Vector2(0.3f, 1.4f); targetRadius = 0.10f; targetSoftness = 0.04f; distortPower = 0.18f;
        }
        else if (type == "Holy")
        {
            effectType = 4f; targetCore = new Color(1.0f, 1.0f, 0.85f, 0.5f); targetEdge = new Color(1.0f, 0.75f, 0.1f, 0.55f);
            scrollSpeed = new Vector2(0.02f, 0.04f); targetRadius = 0.65f; targetSoftness = 0.45f; distortPower = 0.02f;
        }
        else if (type == "Grass")
        {
            effectType = 5f; targetCore = new Color(0.8f, 1.0f, 0.4f, 0.9f); targetEdge = new Color(0.1f, 0.5f, 0.15f, 0.7f);
            scrollSpeed = new Vector2(0.02f, 0.15f); targetRadius = 0.55f; targetSoftness = 0.35f; distortPower = 0.08f;
        }

        if (screenMat != null)
        {
            screenMat.SetFloat("_EffectType", effectType);
            screenMat.SetVector("_ScrollSpeed", scrollSpeed);
            screenMat.SetFloat("_DistortPower", distortPower);
        }

        Sprite[] heartSprites = type == "Fire" ? fireHeartSprites :
                                type == "Ice" ? iceHeartSprites :
                                type == "Poison" ? poisonHeartSprites :
                                type == "Lightning" ? lightningHeartSprites :
                                type == "Holy" ? holyHeartSprites :
                                type == "Grass" ? grassHeartSprites : null;

        float tintStrength = GetMapTintStrength(type);
        Color mapTint = Color.Lerp(Color.white, targetEdge, tintStrength);
        if (allMaps != null)
            foreach (var map in allMaps)
                if (map != null) map.color = mapTint;

        float currentRadius = 0f;
        float currentSoftness = 0.3f;

        while (isAbilityActive)
        {
            if (!isChainFlashing)
            {
                float lerpt = Time.deltaTime * animSpeed;
                currentRadius = Mathf.Lerp(currentRadius, targetRadius, lerpt);
                currentSoftness = Mathf.Lerp(currentSoftness, targetSoftness, lerpt);

                if (screenMat != null)
                {
                    screenMat.SetFloat("_Radius", currentRadius);
                    screenMat.SetFloat("_Softness", currentSoftness);
                    screenMat.SetColor("_CoreColor", targetCore);
                    screenMat.SetColor("_EdgeColor", targetEdge);
                }
            }

            if (playerHealth != null && playerHealth.hearts != null && heartSprites != null && heartSprites.Length > 0)
            {
                int frame = Mathf.FloorToInt(Time.time * animSpeed) % heartSprites.Length;
                foreach (var img in playerHealth.hearts)
                    if (img != null) img.sprite = heartSprites[frame];
            }

            yield return null;
        }
    }
}