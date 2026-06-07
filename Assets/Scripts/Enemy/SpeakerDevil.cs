using System.Collections;
using UnityEngine;

public class SpeakerDevil : RangedEnemy
{
    [SerializeField] private GameObject soundWavePrefab;
    public float waveDelay = 0.2f;        // 애니 시작 후 음파 나오기까지 딜레이(초)
    [SerializeField] private string idleStateName = "Idle"; // 평소 기본 스프라이트 상태

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

        // 시작할 때 기본 스프라이트로
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
        // 1. 공격 모션 재생 (처음부터)
        if (soundAnim != null)
            soundAnim.Play(attackStateName, 0, 0f);

        // 2. 딜레이 후 음파 발사
        yield return new WaitForSeconds(waveDelay);

        if (_waveController != null)
            _waveController.CreateWave();

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySFX(SFXType.EnemyAttack_Mage);

        if (BattleStateBGM.Instance != null)
            BattleStateBGM.Instance.TriggerSonicWobble(1.5f);

        // 3. attack 클립 남은 길이만큼 기다렸다가 기본으로 복귀
        float clipLength = 0.5f; // 기본값 (클립 정보 못 읽을 때 대비)
        if (soundAnim != null)
            clipLength = soundAnim.GetCurrentAnimatorStateInfo(0).length;

        float remain = clipLength - waveDelay;
        if (remain > 0f)
            yield return new WaitForSeconds(remain);

        // 4. 기본 스프라이트로 복귀
        if (soundAnim != null)
            soundAnim.Play(idleStateName, 0, 0f);

        _attackRoutine = null;
    }
}