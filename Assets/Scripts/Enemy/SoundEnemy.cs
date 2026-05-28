using UnityEngine;

public class SoundEnemy : RangedEnemy
{
    [SerializeField] private GameObject soundWavePrefab;
    private SoundWaveController _waveController;

    protected override void Start()
    {
        base.Start();
        if (soundWavePrefab != null)
        {
            GameObject waveObj = Instantiate(soundWavePrefab, transform.position, Quaternion.identity);
            waveObj.transform.SetParent(this.transform);
            _waveController = waveObj.GetComponent<SoundWaveController>();
        }

        // [팀원 작업 반영] 몬스터 사망 시 회피율 복구 이벤트 연결
        EnemyHealth enemyHealth = GetComponent<EnemyHealth>();
        if (enemyHealth != null)
            enemyHealth.OnDeath += RestoreMissChance;
    }

    protected override void Shoot()
    {
        LogManager.Instance.AddLog("사운드 shot()");
        
        if (_waveController != null)
        {
            _waveController.CreateWave();
        }

        // [유저님 작업 반영] 공격 사운드 및 공감각적 BGM 울렁임 효과
        if (SFXManager.Instance != null) {
            SFXManager.Instance.PlaySFX(SFXType.EnemyAttack_Mage); 
        }

        if (BattleStateBGM.Instance != null) {
            BattleStateBGM.Instance.TriggerSonicWobble(1.5f); 
        }
    }

    // [팀원 작업 반영] 회피율 복구 함수
    private void RestoreMissChance()
    {
        if (PlayerStats.instance == null || _waveController == null) return;

        float applied = _waveController.GetAppliedMissChance();
        if (applied > 0)
        {
            PlayerStats.instance.missChance = Mathf.Max(0f, PlayerStats.instance.missChance - applied);
            LogManager.Instance.AddLog($"[디버그] 회피율 복구됨: {PlayerStats.instance.missChance}%");
        }
    }
}