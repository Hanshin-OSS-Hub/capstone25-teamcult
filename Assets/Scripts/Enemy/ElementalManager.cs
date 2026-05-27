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

    [Header("타이머 설정")]
    public float abilityDuration = 120f; // 2분

    [Header("Lightning Chain")]
    public int lightningHitCounter = 0;

    [Header("Noise Texture")]
    public Texture2D noiseTex;

    private Tilemap[] allMaps;
    private Material screenMat;
    private bool isChainFlashing = false;
    public bool isAbilityActive = false;
    public string currentType = "";
    public float abilityTimer = 0f;


    private struct ElementalConfig
    {
        public float effectType;
        public Color coreColor;
        public Color edgeColor;
        public Vector2 scrollSpeed;
        public float radius;
        public float softness;
        public float distortPower;
    }

    void Start()
    {
        InitScreenEffect();
        InitHeartSprite();
        allMaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
        if (mainCamera == null) mainCamera = Camera.main;
    }

    void Update()
    {
        UpdatePlayerPosToShader();

        if (isAbilityActive)
        {
            abilityTimer -= Time.deltaTime;
            if (abilityTimer <= 0f)
            {
                DeactivateAbility();
                if (HeartSlotController.instance != null)
                    HeartSlotController.instance.ClearHeart();
            }
        }
    }

    void InitScreenEffect()
    {
        if (screenEffectImage == null) return;
        screenEffectImage.gameObject.SetActive(false);
        if (screenEffectImage.material != null)
        {
            screenMat = new Material(screenEffectImage.material);
            screenEffectImage.material = screenMat;
            if (noiseTex == null)
                noiseTex = screenMat.GetTexture("_NoiseTex") as Texture2D;
        }
    }

    void InitHeartSprite()
    {
        if (defaultHeartSprite == null && playerHealth != null &&
            playerHealth.hearts.Length > 0 && playerHealth.hearts[0] != null)
            defaultHeartSprite = playerHealth.hearts[0].sprite;
    }

    void UpdatePlayerPosToShader()
    {
        if (screenMat == null || playerTransform == null || mainCamera == null) return;
        Vector3 vp = mainCamera.WorldToViewportPoint(playerTransform.position);
        screenMat.SetVector("_PlayerPos", new Vector4(vp.x, vp.y, 0, 0));
    }

    public void ActivateAbility(string type)
    {
        if (isAbilityActive && currentType == type) return;
        if (isAbilityActive && currentType != type) DeactivateAbility();

        SetElementalFlag(type, true);

        if (BattleStateBGM.Instance != null) BattleStateBGM.Instance.ApplyElementalEffect(type);
        if (playerHealth == null) return;

        allMaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
        isAbilityActive = true;
        currentType = type;
        abilityTimer = abilityDuration;

        if (screenEffectImage != null) screenEffectImage.gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(AbilityLoop(type));
    }

    public void DeactivateAbility()
    {
        if (!isAbilityActive) return;

        isAbilityActive = false;
        currentType = "";
        SetElementalFlag("", false);

        if (BattleStateBGM.Instance != null) BattleStateBGM.Instance.ClearElementalEffect();
        if (screenEffectImage != null) screenEffectImage.gameObject.SetActive(false);

        ResetHearts();
        ResetTilemaps();
    }

    void SetElementalFlag(string type, bool value)
    {
        hasFireHeart = type == "Fire" && value;
        hasIceHeart = type == "Ice" && value;
        hasLightningHeart = type == "Lightning" && value;
    }

    void ResetHearts()
    {
        if (playerHealth?.hearts == null) return;
        foreach (var img in playerHealth.hearts)
        {
            if (img == null) continue;
            img.color = Color.white;
            img.transform.localScale = Vector3.one;
            img.transform.localRotation = Quaternion.identity;
            if (defaultHeartSprite != null) img.sprite = defaultHeartSprite;
        }
    }

    void ResetTilemaps()
    {
        if (allMaps == null) return;
        foreach (var map in allMaps)
            if (map != null) map.color = Color.white;
    }

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
        ApplyLightningFlashShader(1f);

        Color flashColor = new Color(2.0f, 1.8f, 0.8f);
        SetTilemapColor(flashColor);

        yield return new WaitForSeconds(0.05f);

        Color targetMapColor = isAbilityActive && currentType == "Lightning"
            ? Color.Lerp(Color.white, new Color(1.0f, 0.6f, 0.0f, 0.85f), 0.25f)
            : Color.white;

        float t = 0f, duration = 0.12f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;
            ApplyLightningFlashShader(Mathf.Lerp(1f, 0f, p), Mathf.Lerp(0.5f, 0f, p), Mathf.Lerp(0.3f, 0.10f, p));
            SetTilemapColor(Color.Lerp(flashColor, targetMapColor, p));
            yield return null;
        }

        ApplyLightningFlashShader(0f);
        if (!wasActive && screenEffectImage != null) screenEffectImage.gameObject.SetActive(false);
        isChainFlashing = false;
    }

    void ApplyLightningFlashShader(float flash, float boom = 0f, float radius = 0.3f)
    {
        if (screenMat == null) return;
        screenMat.SetFloat("_EffectType", 3f);
        screenMat.SetFloat("_LightningFlash", flash);
        screenMat.SetFloat("_LightningStrike", flash > 0 ? 1f : 0f);
        screenMat.SetFloat("_Progress", 1f);
        screenMat.SetFloat("_Radius", radius);
        screenMat.SetColor("_CoreColor", new Color(1.0f, 1.0f, 0.8f));
        screenMat.SetColor("_EdgeColor", new Color(1.0f, 0.6f, 0.0f));
        screenMat.SetFloat("_BoomFlash", boom);
    }

    IEnumerator AbilityLoop(string type)
    {
        ElementalConfig cfg = GetConfig(type);

        if (screenMat != null)
        {
            screenMat.SetFloat("_EffectType", cfg.effectType);
            screenMat.SetVector("_ScrollSpeed", cfg.scrollSpeed);
            screenMat.SetFloat("_DistortPower", cfg.distortPower);
        }

        Sprite[] heartSprites = GetHeartSprites(type);
        float tintStrength = GetMapTintStrength(type);
        SetTilemapColor(Color.Lerp(Color.white, cfg.edgeColor, tintStrength));

        float currentRadius = 0f, currentSoftness = 0.3f;

        while (isAbilityActive)
        {
            if (!isChainFlashing)
            {
                float lerpt = Time.deltaTime * animSpeed;
                currentRadius = Mathf.Lerp(currentRadius, cfg.radius, lerpt);
                currentSoftness = Mathf.Lerp(currentSoftness, cfg.softness, lerpt);

                if (screenMat != null)
                {
                    screenMat.SetFloat("_Radius", currentRadius);
                    screenMat.SetFloat("_Softness", currentSoftness);
                    screenMat.SetColor("_CoreColor", cfg.coreColor);
                    screenMat.SetColor("_EdgeColor", cfg.edgeColor);
                }
            }

            AnimateHearts(heartSprites);
            yield return null;
        }
    }

    void AnimateHearts(Sprite[] heartSprites)
    {
        if (playerHealth?.hearts == null || heartSprites == null || heartSprites.Length == 0) return;
        int frame = Mathf.FloorToInt(Time.time * animSpeed) % heartSprites.Length;
        foreach (var img in playerHealth.hearts)
            if (img != null) img.sprite = heartSprites[frame];
    }

    void SetTilemapColor(Color color)
    {
        if (allMaps == null) return;
        foreach (var map in allMaps)
            if (map != null) map.color = color;
    }

    Sprite[] GetHeartSprites(string type) => type switch
    {
        "Fire" => fireHeartSprites,
        "Ice" => iceHeartSprites,
        "Poison" => poisonHeartSprites,
        "Lightning" => lightningHeartSprites,
        "Holy" => holyHeartSprites,
        "Grass" => grassHeartSprites,
        _ => null
    };

    float GetMapTintStrength(string type) => type switch
    {
        "Lightning" => 0.25f,
        "Fire" => 0.20f,
        "Ice" => 0.20f,
        "Poison" => 0.25f,
        "Holy" => 0.35f,
        "Grass" => 0.30f,
        _ => 0.15f
    };

    ElementalConfig GetConfig(string type) => type switch
    {
        "Fire" => new ElementalConfig
        {
            effectType = 0f,
            scrollSpeed = new Vector2(0.1f, 1.5f),
            coreColor = new Color(1f, 0.8f, 0.1f, 1f),
            edgeColor = new Color(1f, 0.2f, 0f, 1f),
            radius = 0.2f,
            softness = 0.05f,
            distortPower = 1.2f
        },
        "Ice" => new ElementalConfig
        {
            effectType = 1f,
            scrollSpeed = new Vector2(0.02f, 0.05f),
            coreColor = new Color(0.8f, 0.97f, 1f, 0.9f),
            edgeColor = new Color(0.3f, 0.7f, 1f, 0.85f),
            radius = 0.88f,
            softness = 0.08f,
            distortPower = 0.35f
        },
        "Poison" => new ElementalConfig
        {
            effectType = 2f,
            scrollSpeed = new Vector2(0.06f, 0.1f),
            coreColor = new Color(0.35f, 0.55f, 0.15f, 0.6f),
            edgeColor = new Color(0.15f, 0.05f, 0.25f, 0.6f),
            radius = 0.18f,
            softness = 0.3f,
            distortPower = 0.18f
        },
        "Lightning" => new ElementalConfig
        {
            effectType = 3f,
            scrollSpeed = new Vector2(0.3f, 1.4f),
            coreColor = new Color(1.0f, 1.0f, 0.8f, 1.0f),
            edgeColor = new Color(1.0f, 0.6f, 0.0f, 0.85f),
            radius = 0.10f,
            softness = 0.04f,
            distortPower = 0.18f
        },
        "Holy" => new ElementalConfig
        {
            effectType = 4f,
            scrollSpeed = new Vector2(0.02f, 0.04f),
            coreColor = new Color(1.0f, 1.0f, 0.85f, 0.5f),
            edgeColor = new Color(1.0f, 0.75f, 0.1f, 0.55f),
            radius = 0.65f,
            softness = 0.45f,
            distortPower = 0.02f
        },
        "Grass" => new ElementalConfig
        {
            effectType = 5f,
            scrollSpeed = new Vector2(0.02f, 0.15f),
            coreColor = new Color(0.8f, 1.0f, 0.4f, 0.9f),
            edgeColor = new Color(0.1f, 0.5f, 0.15f, 0.7f),
            radius = 0.55f,
            softness = 0.35f,
            distortPower = 0.08f
        },
        _ => default
    };
}