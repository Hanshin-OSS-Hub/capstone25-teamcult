using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public HealthBarManager hpBarManager;

    private int maxHP = 12;
    private int HP;

    void Start()
    {
        if (hpBarManager == null) {
            hpBarManager = FindAnyObjectByType<HealthBarManager>();
        }
        HP = maxHP;
        //hpBarManager.GenerateHearts();
        hpBarManager.ChangeHealth(HP);
    }
    private void Update() {
        // Z 키를 누르면 체력 1 감소
        if (Input.GetKeyDown(KeyCode.Z)) {
            // 체력이 0보다 클 때만 감소 (음수 방지)
            if (HP > 0) {
                HP -= 1;
                Debug.Log("체력 감소! 현재 체력 : " + HP);
            }
            else {
                Debug.Log("체력이 이미 0입니다.");
            }
        }

        // X 키를 누르면 체력 1 증가
        if (Input.GetKeyDown(KeyCode.X)) {
            if (HP < maxHP) { 
                HP += 1;
                Debug.Log("체력 증가! 현재 체력 : " + HP);
            }
            else{ 
                Debug.Log("체력 증가실패, 이미 최대 체력 : " + HP); 
            }

        }
        hpBarManager.ChangeHealth(HP); // 체력 바 UI 업데이트 함수 호출
    }

    // 데미지를 입는 함수 (다른 스크립트에서 호출)
    public void TakeDamage(int damage)
    {
        if (damage <= 0) { return; }
        HP -= damage;
        if (HP < 0) { HP = 0; }

        // 체력이 변경될 때마다 UI 업데이트 요청
        hpBarManager.ChangeHealth(HP);

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