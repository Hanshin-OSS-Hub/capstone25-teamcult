using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public HealthBarManager hpBarManager;
    private HeatController heatController;

    private int fireHeartIndex = -1;

    void Start()
    {
        if (hpBarManager == null) hpBarManager = FindFirstObjectByType<HealthBarManager>();
        heatController = FindFirstObjectByType<HeatController>();
    }

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

            if (MusicDirector.Instance != null) MusicDirector.Instance.SetFlameMode(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Damage") || other.gameObject.CompareTag("Enemy"))
        {
            TakeDamage();
        }
    }

    public void TakeDamage(int damage = 1)
    {
        if (MusicDirector.Instance != null) MusicDirector.Instance.TriggerDamageEffect();

        hpBarManager.LoseHP(damage);

        if (fireHeartIndex != -1)
        {
            int remainingHP = hpBarManager.GetHeartHP(fireHeartIndex);

            if (remainingHP <= 0)
            {
                if (heatController != null) heatController.StopEffect();

                hpBarManager.ChangeHeartType(HeartAttribute.Normal, fireHeartIndex);
                fireHeartIndex = -1;

                if (MusicDirector.Instance != null) MusicDirector.Instance.SetFlameMode(false);
            }
        }
    }
}