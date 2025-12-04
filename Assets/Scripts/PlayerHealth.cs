using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public HealthBarManager hpBarManager;
    private HeatController heatController;

    private int HP;

    // 현재 불타고 있는 하트가 몇 번째인지 기억하는 변수 (-1이면 없음)
    private int fireHeartIndex = -1;

    void Start()
    {
        if (hpBarManager == null) hpBarManager = FindFirstObjectByType<HealthBarManager>();
        heatController = FindFirstObjectByType<HeatController>();
    }

    // 아이템(FlameHeartItem) 먹었을 때
    public void GetFlameHeart()
    {
        int lastIndex = hpBarManager.heart - 1;

        if (lastIndex >= 0)
        {
            if (fireHeartIndex != -1 && fireHeartIndex != lastIndex)
            {
                hpBarManager.ChangeHeartType(HeartAttribute.Normal, fireHeartIndex);
            }

            hpBarManager.ChangeHeartType(HeartAttribute.Fire, lastIndex);
            fireHeartIndex = lastIndex;

            if (heatController != null) heatController.TriggerEffect();
        }
    }

    // ★ [수정된 부분] 충돌 감지
    private void OnTriggerEnter2D(Collider2D other)
    {
        // "Damage" 태그 또는 "Enemy" 태그 둘 중 하나라도 닿으면 데미지!
        if (other.gameObject.CompareTag("Damage") || other.gameObject.CompareTag("Enemy"))
        {
            TakeDamage();
        }
    }

    // 데미지 입을 때
    public void TakeDamage(int damage = 1)
    {
        hpBarManager.LoseHP(damage);

        if (fireHeartIndex != -1)
        {
            int remainingHP = hpBarManager.GetHeartHP(fireHeartIndex);

            if (remainingHP <= 0)
            {
                if (heatController != null) heatController.StopEffect();

                hpBarManager.ChangeHeartType(HeartAttribute.Normal, fireHeartIndex);
                fireHeartIndex = -1;
            }
        }
    }
}