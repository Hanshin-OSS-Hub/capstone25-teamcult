using System.Collections;
using UnityEngine;

public enum TrapType {
    Once,
    Repeat,
    Forever,
}

public class SpikeTrap:MonoBehaviour
{
    [SerializeField] Spike spike;
    [SerializeField] TrapType trapType;
    [SerializeField] private float cycleTime = 3f;
    [SerializeField] private float activeTime = 1f;

    private void Start() {
        spike.Init(trapType);
        // 반복형일 때만 자동으로 사이클 시작
        if (trapType == TrapType.Repeat) {
            if (cycleTime < activeTime) { cycleTime = activeTime; }
            StartCoroutine(TrapCycleRoutine());
        }
        else {
            spike.On(); // 일회이랑 영구는 킴
        }
    }

    // 반복 주기를 관리하는 코루틴
    private IEnumerator TrapCycleRoutine() {
        while (true) {
            spike.On(); // 가시 활성화
            yield return new WaitForSeconds(activeTime); // activeTime만큼 대기

            spike.Off(); // 가시 비활성화
            // 전체 주기에서 켜져있던 시간을 뺀 나머지 시간만큼 대기
            yield return new WaitForSeconds(cycleTime - activeTime);
        }
    }
}
