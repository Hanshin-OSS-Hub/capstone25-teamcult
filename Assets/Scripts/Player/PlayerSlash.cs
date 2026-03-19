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

    void Start()
    {
        stats = GetComponent<PlayerStats>();
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
        if (MusicDirector.Instance != null)
            MusicDirector.Instance.OnPlayerAttack();

        if (weapons.Length == 0 || stats == null) return;

        WeaponData currentWeapon = weapons[currentIndex];

        // 공격속도 배율 적용
        float speedMultiplier = stats.GetTotalAttackSpeed();
        float adjustedCooldown = currentWeapon.cooldown / speedMultiplier;
        nextAttackTime = Time.time + adjustedCooldown;

        //Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 mousePos = Input.mousePosition;
        Vector3 viewportPos = new Vector3(mousePos.x / Screen.width, mousePos.y / Screen.height, 0);
        Vector3 targetWorldPos = Camera.main.ViewportToWorldPoint(viewportPos);
        targetWorldPos.z = 0;

        // 이제 targetWorldPos를 사용하여 방향(direction)을 계산합니다.
        Vector2 direction = ((Vector2)targetWorldPos - (Vector2)transform.position).normalized;
        //Vector2 direction = (mousePos - transform.position).normalized;

        // 사거리 보너스 적용
        float finalDistance = distance + stats.bonusAttackRange;
        Vector3 spawnPos = transform.position + (Vector3)(direction * finalDistance);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        GameObject obj = Instantiate(currentWeapon.prefab, spawnPos, rotation);

        // 기본 데미지 계산
        float finalDamage = (currentWeapon.damage * stats.attackMultiplier) + stats.bonusDamage;

        // 공격력 % 보너스 적용
        finalDamage *= (1f + stats.bonusAttackPercent / 100f);

        // 치명타 체크
        if (stats.critChance > 0)
        {
            float roll = Random.Range(0f, 100f);
            if (roll < stats.critChance)
            {
                finalDamage *= stats.critMultiplier;
                Debug.Log($"[치명타!] 데미지: {finalDamage}");
            }
        }

        // 4번째 공격 데미지 2배
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

        // 슬래시 데미지 적용
        SlashDamage melee = obj.GetComponent<SlashDamage>();
        if (melee != null)
        {
            melee.damage = (int)finalDamage;
            melee.lifeTime = currentWeapon.lifeTime;
        }

        // 총알 데미지 적용
        PlayerBullet bullet = obj.GetComponent<PlayerBullet>();
        if (bullet != null)
        {
            bullet.damage = finalDamage;
            bullet.speed = currentWeapon.speed;
            Destroy(obj, currentWeapon.lifeTime);
        }
    }
}