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
    [Header(" ���� ����")]
    public WeaponData[] weapons;

    [Header("����")]
    public int currentIndex = 0;
    public float distance = 1.0f;
    private float nextAttackTime = 0f;

    private PlayerStats stats; //  [�߰�] ���� ������ ����

    void Start()
    {
        stats = GetComponent<PlayerStats>(); // �� ������ ���� ��������
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
        {
            MusicDirector.Instance.OnPlayerAttack();
        }
        
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

        //  [�ٽ� ����] ���� ������ = (���ⵥ���� x ����) + �߰�������
        float finalDamage = (currentWeapon.damage * stats.attackMultiplier) + stats.bonusDamage;

        // 1. ���� ����
        SlashDamage melee = obj.GetComponent<SlashDamage>();
        if (melee != null)
        {
            melee.damage = (int)finalDamage;
            melee.lifeTime = currentWeapon.lifeTime;
        }

        // 2. �Ѿ� ����
        PlayerBullet bullet = obj.GetComponent<PlayerBullet>();
        if (bullet != null)
        {
            bullet.damage = finalDamage;
            bullet.speed = currentWeapon.speed;
            Destroy(obj, currentWeapon.lifeTime);
        }
    }
}