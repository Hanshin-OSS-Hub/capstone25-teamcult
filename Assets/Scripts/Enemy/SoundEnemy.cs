using UnityEngine;

public class SoundEnemy : RangedEnemy
{
    [SerializeField] private GameObject soundWavePrefab;
    private SoundWaveController _waveController;
    private EnemyHealth _enemyHealth;

    protected override void Start()
    {
        base.Start();
        if (soundWavePrefab != null)
        {
            GameObject waveObj = Instantiate(soundWavePrefab, transform.position, Quaternion.identity);
            _waveController = waveObj.GetComponent<SoundWaveController>();
            if (_waveController != null)
            {
                _waveController.SetOwnerTransform(transform);
            }
        }

        // [협업 작업 반영] 몬스터 사망 시 회피율 복구 이벤트 연결
        _enemyHealth = GetComponent<EnemyHealth>();
        if (_enemyHealth != null)
        {
            _enemyHealth.OnDeath += RestoreMissChance;
            _enemyHealth.OnDeath += NotifyWaveOwnerDead;
        }
    }

    protected override void Shoot()
    {
        LogManager.Instance.AddLog("사운드 shot()");

        if (_waveController != null)
        {
            _waveController.CreateWave();
        }

        // [협업 작업 반영] 공격 사운드 및 공격감각적 BGM 트리거 효과
        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.PlaySFX(SFXType.EnemyAttack_Mage);
        }

        if (BattleStateBGM.Instance != null)
        {
            BattleStateBGM.Instance.TriggerSonicWobble(1.5f);
        }
    }

    // [협업 작업 반영] 회피율 복구 함수
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

    private void NotifyWaveOwnerDead()
    {
        if (_waveController != null)
        {
            _waveController.NotifyOwnerDead();
        }
    }

    private void OnDestroy()
    {
        if (_enemyHealth != null)
        {
            _enemyHealth.OnDeath -= RestoreMissChance;
            _enemyHealth.OnDeath -= NotifyWaveOwnerDead;
        }
    }
}
