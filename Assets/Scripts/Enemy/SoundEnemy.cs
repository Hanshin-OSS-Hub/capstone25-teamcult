using System.Collections;
using UnityEngine;
public class SoundEnemy : RangedEnemy
{
    [SerializeField] private GameObject soundWavePrefab;
    public float waveDelay = 0.2f; // 애니 시작 후 음파 나오기까지 딜레이(초)
    private SoundWaveController _waveController;
    private Animator soundAnim;

    protected override void Start()
    {
        base.Start();

        soundAnim = GetComponent<Animator>();
        if (soundAnim != null) soundAnim.enabled = false;

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
        // 1. 애니메이션 먼저 (부모의 attackStateName 사용)
        if (soundAnim != null)
        {
            soundAnim.enabled = true;
            soundAnim.Rebind();
            soundAnim.Play(attackStateName, 0, 0f);
            soundAnim.Update(0f);
        }

        // 2. 잠깐 기다렸다가 음파 발사
        yield return new WaitForSeconds(waveDelay);

        //LogManager.Instance.AddLog("사운드 shot()");

        if (_waveController != null)
            _waveController.CreateWave();
        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySFX(SFXType.EnemyAttack_Mage);
        if (BattleStateBGM.Instance != null)
            BattleStateBGM.Instance.TriggerSonicWobble(1.5f);

        // 3. 애니 끝나면 끄기
        if (soundAnim != null)
        {
            float len = soundAnim.GetCurrentAnimatorStateInfo(0).length;
            float remaining = len - waveDelay;
            if (remaining > 0) yield return new WaitForSeconds(remaining);
            soundAnim.enabled = false;
        }
    }

}