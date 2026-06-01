using System.Collections;
using UnityEngine;

public class ChargerEnemy : MonoBehaviour
{
    private Animator anim;
    public float prepareTime = 0.5f; // 돌진 전 기를 모으는 시간
    public float dashSpeed = 10f;
    public float dashDuration = 0.3f;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // 공격 패턴 시작 시 호출
    public IEnumerator ChargeAttackCoroutine()
    {
        // 1. 돌격 준비 애니메이션 실행 (1~2 프레임)
        anim.SetTrigger("PrepareAttack");

        // 조준 및 대기 시간
        yield return new WaitForSeconds(prepareTime);

        // 2. 실제 돌격 애니메이션 실행 (2~4 프레임)
        anim.SetTrigger("ExecuteAttack");

        // 3. 실제 돌진 물리 로직 처리
        float startTime = Time.time;
        while (Time.time < startTime + dashDuration)
        {
            // TODO: 플레이어를 향해 돌진하는 Rigidbody2D 또는 Transform 이동 로직 구현
            yield return null;
        }

        // 돌진 종료 (애니메이션은 Has Exit Time에 의해 자동으로 Idle로 복귀)
    }
}