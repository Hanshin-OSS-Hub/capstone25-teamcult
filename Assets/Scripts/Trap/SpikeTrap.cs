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
        if (trapType == TrapType.Repeat) {
            if (cycleTime < activeTime) { cycleTime = activeTime; }
            StartCoroutine(TrapCycleRoutine());
        }
        else {
            spike.On();
        }
    }

    private IEnumerator TrapCycleRoutine() {
        while (true) {
            spike.On(); 
            yield return new WaitForSeconds(activeTime); 

            spike.Off(); 
            yield return new WaitForSeconds(cycleTime - activeTime);
        }
    }
}
