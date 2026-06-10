using System.Collections;
using UnityEngine;

public class SpeakerDevil : RangedEnemy
{
    [SerializeField] private GameObject soundWavePrefab;
    public float waveDelay = 0.2f;       
    [SerializeField] private string idleStateName = "Idle"; 

    private SoundWaveController _waveController;
    private Animator soundAnim;
    private Coroutine _attackRoutine;

    protected override void Start()
    {
        base.Start();
        soundAnim = GetComponent<Animator>();

        if (soundWavePrefab != null)
        {
            GameObject waveObj = Instantiate(soundWavePrefab, transform.position, Quaternion.identity);
            waveObj.transform.SetParent(this.transform);
            _waveController = waveObj.GetComponent<SoundWaveController>();
        }

        EnemyHealth enemyHealth = GetComponent<EnemyHealth>();
        //if (enemyHealth != null) {
        //    enemyHealth.OnDeath += RestoreMissChance;
        //}

        if (soundAnim != null)
            soundAnim.Play(idleStateName, 0, 0f);
    }

    protected override void Shoot()
    {
        if (_attackRoutine != null)
            StopCoroutine(_attackRoutine);

        _attackRoutine = StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        if (soundAnim != null)
            soundAnim.Play(attackStateName, 0, 0f);

        yield return new WaitForSeconds(waveDelay);

        if (_waveController != null)
            _waveController.CreateWave();

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySFX(SFXType.EnemyAttack_Mage);

        if (BattleStateBGM.Instance != null)
            BattleStateBGM.Instance.TriggerSonicWobble(1.5f);

        float clipLength = 0.5f; 
        if (soundAnim != null)
            clipLength = soundAnim.GetCurrentAnimatorStateInfo(0).length;

        float remain = clipLength - waveDelay;
        if (remain > 0f)
            yield return new WaitForSeconds(remain);

        if (soundAnim != null)
            soundAnim.Play(idleStateName, 0, 0f);

        _attackRoutine = null;
    }
}