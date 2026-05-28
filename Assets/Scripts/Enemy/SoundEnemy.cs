using UnityEngine;

public class SoundEnemy : RangedEnemy {
    [SerializeField] private GameObject soundWavePrefab; // �̸� ������ ������
    private SoundWaveController _waveController; // ĳ���ص� ��Ʈ�ѷ�

    // �θ��� Start�� Ȯ���ؼ� �������� �̸� �����մϴ�.
    protected virtual void Start() {
        // �θ�(RangedEnemy)�� Start ����(�÷��̾� ã�� ��)�� ���� ����
        // ����: �θ��� Start�� private�̸� ȣ���� �� �Ǵ� �θ��ʵ� protected virtual�� �ٲ��ָ� �����ϴ�.
        base.Start();

        if (soundWavePrefab != null) {
            // ���� ���� �� �̸� ������ �ص�
            GameObject waveObj = Instantiate(soundWavePrefab, transform.position, Quaternion.identity);

            // �� ������Ʈ�� ���� ����ٳ�� �Ѵٸ� �θ� �� ��(transform)���� ����
            waveObj.transform.SetParent(this.transform);

            // ��Ʈ�ѷ� ������Ʈ�� �̸� ã�Ƽ� ����(ĳ��)
            _waveController = waveObj.GetComponent<SoundWaveController>();
        }
    }

    // ���� Ÿ�ָ̹��� ȣ��Ǵ� �Լ�
    protected override void Shoot() {
        // ���� �������� �ʰ�, �̸� ã�Ƶ� ��Ʈ�ѷ��� �Լ��� ����
        LogManager.Instance.AddLog("�ĵ� shot()");
        if (_waveController != null) {
            _waveController.CreateWave();
        }

        if (SFXManager.Instance != null) {
            SFXManager.Instance.PlaySFX(SFXType.EnemyAttack_Mage); 
        }

        if (BattleStateBGM.Instance != null) {
            BattleStateBGM.Instance.TriggerSonicWobble(1.5f); 
        }
    }
}