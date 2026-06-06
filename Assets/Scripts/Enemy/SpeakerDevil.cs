using System.Collections;
using UnityEngine;
public class SpeakerDevil : RangedEnemy
{
    [SerializeField] private GameObject soundWavePrefab;
    public float waveDelay = 0.2f; // 애니 시작 후 음파 나오기까지 딜레이(초)
    private SoundWaveController _waveController;
    private Animator soundAnim;
    protected override void Start()
    {
        base.Start();
        soundAnim = GetComponent<Animator>();
        // 애니는 항상 켜둠 (평소엔 idle, 공격 시 attack)

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
    }
    protected override void Shoot()
    {
        StopAllCoroutines();
        StartCoroutine(AttackRoutine());
    }
    IEnumerator AttackRoutine()
    {
        // 1. 공격 애니 재생
        if (soundAnim != null)
        {
            soundAnim.Play(attackStateName, 0, 0f);
        }
        // 2. 잠깐 기다렸다가 음파 발사
        yield return new WaitForSeconds(waveDelay);
        if (_waveController != null)
            _waveController.CreateWave();
        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySFX(SFXType.EnemyAttack_Mage);
        if (BattleStateBGM.Instance != null)
            BattleStateBGM.Instance.TriggerSonicWobble(1.5f);
        // 공격 끝나면 자동으로 idle로 돌아감 (Animator 전이에 맡김)
    }
}