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
    [Header(" 무기 설정")]
    public WeaponData[] weapons;

    [Header("상태")]
    public int currentIndex = 0;
    public float distance = 1.0f;
    private float nextAttackTime = 0f;

    private PlayerStats stats; //  [추가] 스탯 관리소 연결

    void Start()
    {
        stats = GetComponent<PlayerStats>(); // 내 몸에서 스탯 가져오기
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
        if (weapons.Length == 0 || stats == null) return;

        WeaponData currentWeapon = weapons[currentIndex];
        nextAttackTime = Time.time + currentWeapon.cooldown;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        Vector2 direction = (mousePos - transform.position).normalized;
        Vector3 spawnPos = transform.position + (Vector3)(direction * distance);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        GameObject obj = Instantiate(currentWeapon.prefab, spawnPos, rotation);

        //  [핵심 변경] 최종 데미지 = (무기데미지 x 배율) + 추가데미지
        float finalDamage = (currentWeapon.damage * stats.attackMultiplier) + stats.bonusDamage;

        // 1. 근접 무기
        SlashDamage melee = obj.GetComponent<SlashDamage>();
        if (melee != null)
        {
            melee.damage = (int)finalDamage;
            melee.lifeTime = currentWeapon.lifeTime;
        }

        // 2. 총알 무기
        PlayerBullet bullet = obj.GetComponent<PlayerBullet>();
        if (bullet != null)
        {
            bullet.damage = finalDamage;
            bullet.speed = currentWeapon.speed;
            Destroy(obj, currentWeapon.lifeTime);
        }
    }
}