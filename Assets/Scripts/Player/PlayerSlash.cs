using UnityEngine;

[System.Serializable]
public class WeaponData
{
    public string weaponName;
    public GameObject prefab;
    public float damage;
    public float speed;
    public float lifeTime;
    public float cooldown;
}

public class PlayerSlash : MonoBehaviour
{
    [Header("무기 설정")]
    public WeaponData[] weapons;
    [Header("설정")]
    public int currentIndex = 0;
    public float distance = 1.0f;
    private float nextAttackTime = 0f;
    private PlayerStats stats;
    private ElementalManager elementalManager;

    [Header("번개 체인 설정")]
    public float lightningChainRadius = 4f;      // 체인 범위
    public float lightningChainDamageRatio = 0.5f; // 체인 데미지 비율 (0.5 = 50%)
    public float lightningDuration = 1.5f;         // 지속 시간

    void Start()
    {
        stats = GetComponent<PlayerStats>();
        elementalManager = GetComponent<ElementalManager>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) currentIndex = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2)) currentIndex = 1;
        if (Input.GetKeyDown(KeyCode.Alpha3)) currentIndex = 2;
        if (Input.GetMouseButton(0) && Time.time >= nextAttackTime)
        {
            Attack();
        }
    }

    void Attack()
    {
        if (BattleStateBGM.Instance != null)
        {
            BattleStateBGM.Instance.PlayAttackFX();
        }

        if (weapons.Length == 0 || stats == null) return;
        WeaponData currentWeapon = weapons[currentIndex];

        float speedMultiplier = stats.GetTotalAttackSpeed();
        float adjustedCooldown = currentWeapon.cooldown / speedMultiplier;
        nextAttackTime = Time.time + adjustedCooldown;

        Vector3 mousePos = Input.mousePosition;
        Vector3 viewportPos = new Vector3(mousePos.x / Screen.width, mousePos.y / Screen.height, 0);
        Vector3 targetWorldPos = Camera.main.ViewportToWorldPoint(viewportPos);
        targetWorldPos.z = 0;

        Vector2 direction = ((Vector2)targetWorldPos - (Vector2)transform.position).normalized;

        float finalDistance = distance + stats.bonusAttackRange;
        Vector3 spawnPos = transform.position + (Vector3)(direction * finalDistance);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        GameObject obj = Instantiate(currentWeapon.prefab, spawnPos, rotation);

        float finalDamage = (currentWeapon.damage * stats.attackMultiplier) + stats.bonusDamage;
        finalDamage *= (1f + stats.bonusAttackPercent / 100f);

        if (stats.critChance > 0)
        {
            float roll = Random.Range(0f, 100f);
            if (roll < stats.critChance)
            {
                finalDamage *= stats.critMultiplier;
                Debug.Log($"[치명타!] 데미지: {finalDamage}");
            }
        }

        if (stats.everyFourthAttackBonus)
        {
            stats.attackCounter++;
            if (stats.attackCounter >= 4)
            {
                finalDamage *= 2f;
                stats.attackCounter = 0;
                Debug.Log($"[4번째 공격!] 데미지 2배: {finalDamage}");
            }
        }

        SlashDamage melee = obj.GetComponent<SlashDamage>();
        if (melee != null)
        {
            melee.damage = (int)finalDamage;
            melee.lifeTime = currentWeapon.lifeTime;
        }

        PlayerBullet bullet = obj.GetComponent<PlayerBullet>();
        if (bullet != null)
        {
            bullet.damage = finalDamage;
            bullet.speed = currentWeapon.speed;
            Destroy(obj, currentWeapon.lifeTime);
        }

        // 속성 효과 부착
        if (elementalManager != null)
        {
            if (melee != null)
            {
                if (elementalManager.hasFireHeart)
                    melee.gameObject.AddComponent<BurnOnHit>().elementalManager = elementalManager;
                if (elementalManager.hasIceHeart)
                    melee.gameObject.AddComponent<SlowOnHit>().elementalManager = elementalManager;
                if (elementalManager.hasLightningHeart)
                {
                    LightningOnHit lo = melee.gameObject.AddComponent<LightningOnHit>();
                    lo.elementalManager = elementalManager;
                    lo.chainRadius = lightningChainRadius;
                    lo.chainDamageRatio = lightningChainDamageRatio;
                    lo.duration = lightningDuration;
                }
            }
            if (bullet != null)
            {
                if (elementalManager.hasFireHeart)
                    bullet.gameObject.AddComponent<BurnOnHit>().elementalManager = elementalManager;
                if (elementalManager.hasIceHeart)
                    bullet.gameObject.AddComponent<SlowOnHit>().elementalManager = elementalManager;
                if (elementalManager.hasLightningHeart)
                {
                    LightningOnHit lo = bullet.gameObject.AddComponent<LightningOnHit>();
                    lo.elementalManager = elementalManager;
                    lo.chainRadius = lightningChainRadius;
                    lo.chainDamageRatio = lightningChainDamageRatio;
                    lo.duration = lightningDuration;
                }
            }
        }
    }
}