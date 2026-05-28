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

        EnemyHealth enemyHealth = GetComponent<EnemyHealth>();
        if (enemyHealth != null)
            enemyHealth.OnDeath += RestoreMissChance;
    }

    protected override void Shoot()
    {
        LogManager.Instance.AddLog("파동 shot()");
        if (_waveController != null)
        {
            _waveController.CreateWave();
        }
    }

    private void RestoreMissChance()
    {
        if (PlayerStats.instance == null || _waveController == null) return;

        float applied = _waveController.GetAppliedMissChance();
        if (applied > 0)
        {
            PlayerStats.instance.missChance = Mathf.Max(0f, PlayerStats.instance.missChance - applied);
            LogManager.Instance.AddLog($"[음파] 명중률 회복: {PlayerStats.instance.missChance}%");
        }
    }
}