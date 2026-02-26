using UnityEngine;
using System.Collections;

public class DamageFlash : MonoBehaviour
{
    private Material mat;

    // 셰이더 그래프에서 만든 변수 이름입니다.
    // 만약 작동 안 하면 셰이더 그래프 Blackboard에서 "Reference" 이름을 확인해야 합니다.
    // 보통 기본값은 _HitAmount 입니다.
    private string hitParam = "_HitAmount";

    void Start()
    {
        // 내 스프라이트 렌더러에 있는 재질(Material)을 가져옵니다.
        // .material을 쓰면 나만의 복사본을 쓰기 때문에, 다른 적은 안 번쩍거리고 나만 번쩍입니다.
        mat = GetComponent<SpriteRenderer>().material;
    }

    // 외부(총알 등)에서 이 함수를 부르면 번쩍입니다.
    public void Flash()
    {
        // 이미 번쩍이는 중일 수도 있으니 코루틴을 멈추고 새로 시작합니다.
        StopAllCoroutines();
        StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        // 1. 하얗게 켠다 (HitAmount를 1로)
        mat.SetFloat(hitParam, 1f);

        // 2. 0.1초 기다린다 (번쩍하는 시간)
        yield return new WaitForSeconds(0.1f);

        // 3. 다시 끈다 (HitAmount를 0으로)
        mat.SetFloat(hitParam, 0f);
    }

    // 테스트용: 마우스 우클릭하면 번쩍!
    void Update()
    {
        if (Input.GetMouseButtonDown(1)) // 마우스 오른쪽 버튼
        {
            Flash();
        }
    }
}