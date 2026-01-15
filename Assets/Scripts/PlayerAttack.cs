using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("스텟")]
    public float baseDamage = 10f;      // 기본 공격력
    public float addedDamage = 0f;      // 아이템으로 추가된 공격력
    public float damageMultiplier = 1f; // 배율 (1.0 = 100%)

    [Header("연결")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    void Update()
    {
        LookAtMouse();
        if (Input.GetMouseButtonDown(0)) Shoot();
    }

    void LookAtMouse()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        Vector2 direction = mousePos - firePoint.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // ★ 최종 데미지 = (기본 + 추가) * 배율
        float finalDamage = (baseDamage + addedDamage) * damageMultiplier;

        bullet.GetComponent<PlayerBullet>().SetDamage(finalDamage);
    }

    public void AddDamage(float amount) { addedDamage += amount; }
    public void AddMultiplier(float amount) { damageMultiplier += amount; }
}