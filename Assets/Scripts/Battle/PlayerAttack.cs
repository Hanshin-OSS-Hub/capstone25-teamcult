using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("기본 스텟")]
    public float baseDamage = 10f;      // 내 원래 공격력 (맨몸)

    [Header("아이템으로 오르는 스텟")]
    public float addedDamage = 0f;      // + (더하기) 공격력
    public float damageMultiplier = 1f; // x (곱하기) 배율 (기본 1.0)

    [Header("연결")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    void Update()
    {
        LookAtMouse();

        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    void LookAtMouse()
    {
        // (회전 코드 기존과 동일)
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        Vector2 direction = mousePos - firePoint.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // ★ [가장 기본적인 로그라이크 공식]
        // (기본 + 추가) * 배율
        float finalDamage = (baseDamage + addedDamage) * damageMultiplier;

        // 소수점 둘째 자리까지만 쓰거나 반올림 (깔끔하게)
        // finalDamage = Mathf.Round(finalDamage);

        bullet.GetComponent<PlayerBullet>().SetDamage(finalDamage);

        Debug.Log($"⚔️ 공격! ({baseDamage} + {addedDamage}) x {damageMultiplier} = {finalDamage}");
    }

    // 아이템 1: 깡공 증가 (+1, +5 등)
    public void AddDamage(float amount)
    {
        addedDamage += amount;
        Debug.Log($"💪 공격력 +{amount} 증가!");
    }

    // 아이템 2: 배율 증가 (+0.1은 10% 증가, +0.5는 50% 증가)
    public void AddMultiplier(float amount)
    {
        damageMultiplier += amount;
        Debug.Log($"🔥 배율 +{amount * 100}% 증가!");
    }
}