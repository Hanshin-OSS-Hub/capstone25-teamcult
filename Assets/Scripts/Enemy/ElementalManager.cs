using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using System.Collections;

public class ElementalManager : MonoBehaviour
{
    [Header("References")]
    public PlayerHealth playerHealth;
    public Image screenEffectImage;

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

    private Tilemap[] allMaps;
    private float savedHealth;
    private bool isAbilityActive = false;
    private Material screenMat;

    private float flashCooldown = 0f;
    private float flashCooldownMax = 2.5f;

    void Start()
    {
        if (screenEffectImage != null)
        {
            screenEffectImage.gameObject.SetActive(false);
            if (screenEffectImage.material != null)
            {
                screenMat = new Material(screenEffectImage.material);
                screenEffectImage.material = screenMat;
            }
        }
        if (defaultHeartSprite == null && playerHealth != null && playerHealth.hearts.Length > 0)
            if (playerHealth.hearts[0] != null) defaultHeartSprite = playerHealth.hearts[0].sprite;
        allMaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
    }

    void Update()
    {
        if (isAbilityActive) if (playerHealth.currentHealth <= savedHealth - 2.0f) DeactivateAbility();
        if (flashCooldown > 0f) flashCooldown -= Time.deltaTime;
    }

    public void ActivateAbility(string type)
    {
        if (playerHealth == null) return;
        allMaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
        isAbilityActive = true;
        savedHealth = playerHealth.currentHealth;
        flashCooldown = 0f;
        if (screenEffectImage != null) screenEffectImage.gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(AbilityLoop(type));
    }

    public void DeactivateAbility()
    {
        if (!isAbilityActive) return;
        isAbilityActive = false;
        if (screenEffectImage != null) screenEffectImage.gameObject.SetActive(false);
        if (playerHealth.hearts != null)
            foreach (var img in playerHealth.hearts)
                if (img != null) { img.color = Color.white; img.transform.localScale = Vector3.one; img.transform.localRotation = Quaternion.identity; if (defaultHeartSprite != null) img.sprite = defaultHeartSprite; }
        if (allMaps != null) foreach (var map in allMaps) if (map != null) map.color = Color.white;
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
            // 수정: radius를 높여 외각 얇은 서리만 표시
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
            effectType = 3f; targetCore = new Color(1.0f, 1.0f, 1.0f, 1.0f); targetEdge = new Color(0.0f, 0.5f, 1.0f, 0.85f);
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
            screenMat.SetFloat("_Softness", targetSoftness);
            screenMat.SetFloat("_DistortPower", distortPower);
            screenMat.SetColor("_CoreColor", targetCore);
            screenMat.SetColor("_EdgeColor", targetEdge);
            screenMat.SetFloat("_Progress", 0f);
            screenMat.SetFloat("_Radius", targetRadius);
            if (type == "Fire") { screenMat.SetFloat("_FireCenterY", 1.05f); screenMat.SetFloat("_FireSpread", 0f); }
            if (type == "Lightning") { screenMat.SetFloat("_LightningFlash", 0f); screenMat.SetFloat("_LightningStrike", 0f); screenMat.SetFloat("_BoomFlash", 0f); screenMat.SetFloat("_EdgeCurrent", 0f); }
            if (type == "Holy") { screenMat.SetFloat("_HolyFlash", 0f); screenMat.SetFloat("_HolyBreath", 0f); }
        }

        float tintStrength = GetMapTintStrength(type);
        if (type == "Lightning") yield return StartCoroutine(LightningIntro(targetRadius, targetCore));
        else if (type == "Poison") yield return StartCoroutine(PoisonIntro(targetRadius, targetCore));
        else if (type == "Holy") yield return StartCoroutine(HolyIntro(targetRadius, targetCore));
        else
        {
            float dur = 0.8f, timer = 0f;
            while (timer < dur)
            {
                timer += Time.deltaTime;
                float e = 1f - Mathf.Pow(1f - Mathf.Clamp01(timer / dur), 3f);
                if (screenMat != null)
                {
                    screenMat.SetFloat("_Progress", e);
                    screenMat.SetFloat("_Radius", Mathf.Lerp(type == "Ice" ? 1.0f : 0f, targetRadius, e));
                    if (type == "Fire") screenMat.SetFloat("_FireSpread", Mathf.Lerp(0f, 0.5f, e));
                }
                if (allMaps != null && tintStrength > 0f) { Color subtle = Color.Lerp(Color.white, GetMapColor(type, Time.time), e * tintStrength); foreach (var map in allMaps) if (map != null) map.color = subtle; }
                UpdateHeartVisuals(type, Time.time, Mathf.Lerp(1f, GetHeartScale(type), e));
                yield return null;
            }
        }

        while (isAbilityActive)
        {
            float t = Time.time;
            if (type == "Lightning" && screenMat != null)
            {
                float micro = Mathf.PerlinNoise(t * 6f, 0f) * 0.15f;
                if (Mathf.PerlinNoise(t * 2.5f, 77f) > 0.88f && flashCooldown <= 0f)
                { screenMat.SetFloat("_LightningFlash", 1.0f); screenMat.SetFloat("_EdgeCurrent", 1.8f); screenMat.SetColor("_CoreColor", Color.white); screenMat.SetFloat("_LightningStrike", 1.0f); flashCooldown = flashCooldownMax; }
                else
                { screenMat.SetFloat("_LightningFlash", micro); screenMat.SetFloat("_EdgeCurrent", 1.0f); screenMat.SetColor("_CoreColor", targetCore); screenMat.SetFloat("_LightningStrike", 0.0f); }
            }
            else if (type == "Poison" && screenMat != null)
            {
                float poisonPulse = (Mathf.Sin(t * 1.5f) + 1f) * 0.5f;
                Color mapColor = Color.Lerp(new Color(0.20f, 0.25f, 0.15f), new Color(0.35f, 0.45f, 0.20f), poisonPulse);
                if (allMaps != null) foreach (var map in allMaps) if (map != null) map.color = Color.Lerp(Color.white, mapColor, tintStrength);
            }
            else if (type == "Holy" && screenMat != null)
            {
                float breath = 0.55f + 0.45f * Mathf.Sin(t * 0.75f);
                screenMat.SetFloat("_HolyBreath", breath);
                Color warmGold = new Color(1.1f, 1.08f, 0.95f);
                if (allMaps != null) foreach (var map in allMaps) if (map != null) map.color = Color.Lerp(Color.white, warmGold, tintStrength * breath);
            }
            else if (type == "Grass" && screenMat != null)
            {
                float grassPulse = (Mathf.Sin(t * 1.2f) + 1f) * 0.5f;
                if (allMaps != null) foreach (var map in allMaps) if (map != null) map.color = Color.Lerp(Color.white, new Color(0.85f, 1.05f, 0.85f), tintStrength * grassPulse);
            }
            else if (allMaps != null && tintStrength > 0f)
            {
                Color subtle = Color.Lerp(Color.white, GetMapColor(type, t), tintStrength);
                foreach (var map in allMaps) if (map != null) map.color = subtle;
            }
            UpdateHeartVisuals(type, t, GetHeartScale(type));
            yield return null;
        }
    }

    // Holy 인트로
    IEnumerator HolyIntro(float targetRadius, Color targetCore)
    {
        float dur = 1.2f, elapsed = 0f;
        if (screenMat != null) { screenMat.SetFloat("_HolyFlash", 0f); screenMat.SetFloat("_HolyBreath", 1f); }
        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float ep = 1f - Mathf.Pow(1f - Mathf.Clamp01(elapsed / dur), 2f);
            if (screenMat != null) screenMat.SetFloat("_Progress", ep);
            Color warmLight = Color.Lerp(Color.white, new Color(1.1f, 1.05f, 0.9f), ep * 0.4f);
            if (allMaps != null) foreach (var map in allMaps) if (map != null) map.color = warmLight;
            UpdateHeartVisuals("Holy", Time.time, Mathf.Lerp(1f, GetHeartScale("Holy"), ep));
            yield return null;
        }
        if (screenMat != null) screenMat.SetFloat("_Progress", 1f);
    }

    IEnumerator PoisonIntro(float targetRadius, Color targetCore) { float dur = 0.8f; float elapsed = 0f; if (screenMat != null) screenMat.SetFloat("_Radius", 0f); while (elapsed < dur) { elapsed += Time.deltaTime; float ep = 1f - Mathf.Pow(1f - Mathf.Clamp01(elapsed / dur), 3f); if (screenMat != null) { screenMat.SetFloat("_Progress", ep); screenMat.SetFloat("_Radius", Mathf.Lerp(0f, targetRadius, ep)); } if (allMaps != null) { Color sickly = Color.Lerp(Color.white, new Color(0.3f, 0.4f, 0.2f), ep * 0.4f); foreach (var map in allMaps) if (map != null) map.color = sickly; } UpdateHeartVisuals("Poison", Time.time, Mathf.Lerp(1f, GetHeartScale("Poison"), ep)); yield return null; } if (screenMat != null) { screenMat.SetFloat("_Progress", 1f); screenMat.SetFloat("_Radius", targetRadius); } }
    IEnumerator LightningIntro(float targetRadius, Color targetCore) { float elapsed = 0f; while (elapsed < 0.15f) { elapsed += Time.deltaTime; float p = Mathf.Clamp01(elapsed / 0.15f); if (screenMat != null) { screenMat.SetFloat("_Progress", 0f); screenMat.SetFloat("_EdgeCurrent", 0f); } if (allMaps != null) { Color dark = Color.Lerp(Color.white, new Color(0.05f, 0.05f, 0.1f), p); foreach (var map in allMaps) if (map != null) map.color = dark; } yield return null; } if (screenMat != null) { screenMat.SetFloat("_BoomFlash", 1f); screenMat.SetFloat("_EdgeCurrent", 2.0f); screenMat.SetFloat("_Radius", targetRadius); screenMat.SetColor("_CoreColor", Color.white); screenMat.SetFloat("_LightningStrike", 1f); screenMat.SetFloat("_Progress", 1f); } if (allMaps != null) foreach (var map in allMaps) if (map != null) map.color = new Color(0.8f, 0.9f, 1f); yield return new WaitForSeconds(0.08f); elapsed = 0f; while (elapsed < 0.35f) { elapsed += Time.deltaTime; float ep = 1f - Mathf.Pow(1f - Mathf.Clamp01(elapsed / 0.35f), 2f); if (screenMat != null) { screenMat.SetFloat("_BoomFlash", Mathf.Lerp(1f, 0f, ep)); screenMat.SetFloat("_LightningStrike", Mathf.Lerp(1f, 0f, ep)); screenMat.SetFloat("_EdgeCurrent", Mathf.Lerp(2.0f, 1.0f, ep)); screenMat.SetColor("_CoreColor", Color.Lerp(Color.white, targetCore, ep)); } if (allMaps != null) { Color mapFinal = Color.Lerp(Color.white, new Color(0.2f, 0.22f, 0.35f), 0.25f); foreach (var map in allMaps) if (map != null) map.color = Color.Lerp(new Color(0.8f, 0.9f, 1f), mapFinal, ep); } UpdateHeartVisuals("Lightning", Time.time, Mathf.Lerp(1f, GetHeartScale("Lightning"), ep)); yield return null; } if (screenMat != null) { screenMat.SetFloat("_BoomFlash", 0f); screenMat.SetFloat("_LightningStrike", 0f); screenMat.SetFloat("_EdgeCurrent", 1.0f); screenMat.SetColor("_CoreColor", targetCore); } flashCooldown = 1.0f; }

    Color GetMapColor(string type, float t) { if (type == "Fire") return new Color(1f, 0.6f, 0.4f); if (type == "Ice") return new Color(0.4f, 0.8f, 1f); if (type == "Poison") return new Color(0.35f, 0.45f, 0.20f); if (type == "Lightning") return new Color(0.2f, 0.22f, 0.35f); if (type == "Holy") return new Color(1.05f, 0.95f, 0.75f); if (type == "Grass") return new Color(0.4f, 0.6f, 0.3f); return Color.white; }
    float GetHeartScale(string type) { if (type == "Fire") return 1.12f; if (type == "Ice") return 1.05f; if (type == "Lightning") return 1.08f; if (type == "Grass") return 1.06f; if (type == "Holy") return 1.05f; if (type == "Poison") return 1.04f; return 1.05f; }

    void UpdateHeartVisuals(string type, float time, float scale)
    {
        Sprite[] arr = type == "Fire" ? fireHeartSprites : type == "Ice" ? iceHeartSprites : type == "Poison" ? poisonHeartSprites : type == "Lightning" ? lightningHeartSprites : type == "Grass" ? grassHeartSprites : holyHeartSprites;
        int fi = 0; if (arr != null && arr.Length > 0) fi = (int)(time * animSpeed) % arr.Length;
        for (int i = 0; i < playerHealth.hearts.Length; i++)
        {
            Image img = playerHealth.hearts[i]; if (img == null || img.fillAmount <= 0) continue; float offset = i * 0.3f;
            if (arr != null && arr.Length > 0) { img.sprite = arr[fi]; img.color = Color.white; }
            else { if (defaultHeartSprite != null) img.sprite = defaultHeartSprite; img.color = Color.white; }
            if (type == "Fire") { img.transform.localScale = Vector3.one * (scale + Mathf.PerlinNoise(time * 10f + offset, 0f) * 0.06f); img.transform.localRotation = Quaternion.identity; }
            else if (type == "Ice") { img.transform.localScale = Vector3.one * (scale + Mathf.Sin(time * 2f + offset) * 0.02f); img.transform.localRotation = Quaternion.identity; }
            else if (type == "Lightning") { img.transform.localScale = Vector3.one * (scale + Mathf.PerlinNoise(time * 25f + offset, 0f) * 0.04f + (flashCooldown > 2.3f ? 0.08f : 0f)); img.transform.localRotation = Quaternion.identity; }
            else if (type == "Poison") { float sz = Mathf.PerlinNoise(time * 0.9f + offset * 3f, 77f); if (sz > 0.72f) { img.transform.localScale = Vector3.one * (scale + Mathf.PerlinNoise(time * 28f + offset, 0f) * 0.06f); img.transform.localRotation = Quaternion.Euler(0, 0, (Mathf.PerlinNoise(time * 32f + offset, 0f) - 0.5f) * 6f); } else { img.transform.localScale = Vector3.one * scale; img.transform.localRotation = Quaternion.identity; } }
            else if (type == "Holy") { float br = (Mathf.Sin(time * 0.85f + offset * 0.6f) + 1f) * 0.5f; img.transform.localScale = Vector3.one * (scale + br * 0.04f); img.transform.localRotation = Quaternion.identity; img.color = Color.Lerp(Color.white, new Color(1f, 0.95f, 0.6f), br * 0.35f); }
            else if (type == "Grass") { float blink = (Mathf.Sin(time * 3.0f + offset * 2.1f) + 1f) * 0.5f; img.transform.localScale = Vector3.one * (scale + blink * 0.04f); img.transform.localRotation = Quaternion.identity; img.color = Color.Lerp(new Color(0.6f, 1f, 0.4f), new Color(0.9f, 1f, 0.7f), blink); }
        }
    }
}