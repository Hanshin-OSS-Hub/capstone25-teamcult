using UnityEngine;

public class PlayerSlash : MonoBehaviour
{
    [Header("장착된 무기")]
    public Item equippedWeapon;
    public static PlayerSlash instance;

    [Header("설정")]
    public float distance = 1.0f;
    private float nextAttackTime = 0f;
    private PlayerStats stats;
    private ElementalManager elementalManager;
    private PlayerMovement playerMovement;

    [Header("발사 위치")]
    public Transform firePoint;

    [Header("번개 체인 설정")]
    public float lightningChainRadius = 4f;
    public float lightningChainDamageRatio = 0.5f;
    public float lightningDuration = 1.5f;

    private int gunShotCount = 0;
    private Animator anim;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        stats = GetComponent<PlayerStats>();
        elementalManager = GetComponent<ElementalManager>();
        anim = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (GameManager.instance.isUIOpen) return;

        if (Input.GetMouseButton(0) && Time.time >= nextAttackTime)
        {
            Attack();
        }
    }

    public void SetWeapon(Item newWeapon)
    {
        equippedWeapon = newWeapon;

        if (anim != null && newWeapon.weaponAnim != null)
        {
            anim.runtimeAnimatorController = newWeapon.weaponAnim;
        }

        gunShotCount = 0;
        Debug.Log($"[무기 장착] {newWeapon.itemName}");
    }

    void Attack()
    {
        if (equippedWeapon == null || stats == null) return;

        // 공격할 때 마우스 방향으로 바라보기
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookDir = ((Vector2)mousePos - (Vector2)transform.position).normalized;
        anim.SetFloat("DirX", lookDir.x);
        anim.SetFloat("DirY", lookDir.y);

        if (anim != null) anim.SetTrigger("Attack");

        if (SFXManager.Instance != null)
        {
            if (equippedWeapon.weaponType == WeaponType.Sword)
                SFXManager.Instance.PlaySFX(SFXType.PlayerAttack_1);
            else if (equippedWeapon.weaponType == WeaponType.Axe)
                SFXManager.Instance.PlaySFX(SFXType.PlayerAttack_2);
            else if (equippedWeapon.weaponType == WeaponType.Handgun)
            {
                SFXManager.Instance.PlaySFX(SFXType.PlayerAttack_3);
                gunShotCount++;
                if (gunShotCount >= 10)
                {
                    SFXManager.Instance.PlaySFX(SFXType.PlayerReload_3);
                    gunShotCount = 0;
                }
            }
        }

        float speedMultiplier = stats.GetTotalAttackSpeed();
        float adjustedCooldown = equippedWeapon.cooldown / speedMultiplier;
        nextAttackTime = Time.time + adjustedCooldown;

        Vector3 mousePos2 = Input.mousePosition;
        Vector3 viewportPos = new Vector3(mousePos2.x / Screen.width, mousePos2.y / Screen.height, 0);
        Vector3 targetWorldPos = Camera.main.ViewportToWorldPoint(viewportPos);
        targetWorldPos.z = 0;

        Vector2 direction = ((Vector2)targetWorldPos - (Vector2)firePoint.position).normalized;

        Vector3 spawnPos;
        if (firePoint != null)
        {
            float offsetX = targetWorldPos.x < transform.position.x ? -Mathf.Abs(firePoint.localPosition.x) : Mathf.Abs(firePoint.localPosition.x);
            firePoint.localPosition = new Vector3(offsetX, firePoint.localPosition.y, 0);
            spawnPos = firePoint.position;
        }
        else
        {
            float finalDistance = distance + stats.bonusAttackRange;
            spawnPos = transform.position + (Vector3)(direction * finalDistance);
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        GameObject obj = Instantiate(equippedWeapon.prefab, spawnPos, rotation);

        float finalDamage = (equippedWeapon.damage * stats.attackMultiplier) + stats.bonusDamage;
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
            melee.lifeTime = equippedWeapon.lifeTime;
        }

        PlayerBullet bullet = obj.GetComponent<PlayerBullet>();
        if (bullet != null)
        {
            bullet.damage = finalDamage;
            bullet.speed = equippedWeapon.speed;
            Destroy(obj, equippedWeapon.lifeTime);
        }

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