using UnityEngine;

public class SoundEnemy : RangedEnemy {
    [SerializeField] private GameObject soundWavePrefab; // 미리 생성할 프리팹
    private SoundWaveController _waveController; // 캐싱해둘 컨트롤러

    // 부모의 Start를 확장해서 프리팹을 미리 생성합니다.
    protected virtual void Start() {
        // 부모(RangedEnemy)의 Start 로직(플레이어 찾기 등)을 먼저 실행
        // 주의: 부모의 Start가 private이면 호출이 안 되니 부모쪽도 protected virtual로 바꿔주면 좋습니다.
        base.Start();

        if (soundWavePrefab != null) {
            // 게임 시작 시 미리 생성만 해둠
            GameObject waveObj = Instantiate(soundWavePrefab, transform.position, Quaternion.identity);

            // 이 오브젝트가 적을 따라다녀야 한다면 부모를 이 적(transform)으로 설정
            waveObj.transform.SetParent(this.transform);

            // 컨트롤러 컴포넌트를 미리 찾아서 저장(캐싱)
            _waveController = waveObj.GetComponent<SoundWaveController>();
        }
    }

    // 공격 타이밍마다 호출되는 함수
    protected override void Shoot() {
        // 새로 생성하지 않고, 미리 찾아둔 컨트롤러의 함수만 실행
        LogManager.Instance.AddLog("파동 shot()");
        if (_waveController != null) {
            _waveController.CreateWave();
        }
    }
}