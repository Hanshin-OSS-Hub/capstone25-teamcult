using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public HealthBarManager hpBarManager;

    //private int maxHP = 12;
    private int HP;
    

    void Start()
    {
        if (hpBarManager == null) {
            hpBarManager = FindAnyObjectByType<HealthBarManager>();
        }
        HP = 12;
        //hpBarManager.GenerateHearts();
        //hpBarManager.ChangeHealth(HP);
    }

    private void ChangeHeartType(HeartAttribute type, int index = 0) {
        hpBarManager.ChangeHeartType(type, index);
    }
    public void GetFlameHeart() {
        ChangeHeartType(HeartAttribute.Fire, hpBarManager.heart - 1);
    }

    // 데미지를 입는 함수 (다른 스크립트에서 호출)
    public void TakeDamage(int damage)
    {
        if (damage <= 0) { return; }
        HP -= damage;
        if (HP < 0) { HP = 0; }

        // 체력이 변경될 때마다 UI 업데이트 요청
        //hpBarManager.ChangeHealth(HP);

        if (HP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Player Died!");
        // 사망 처리 로직
    }
}