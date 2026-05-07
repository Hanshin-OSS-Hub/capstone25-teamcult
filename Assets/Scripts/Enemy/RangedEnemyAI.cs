using UnityEngine;

public class RangedEnemy : MonoBehaviour
{
    [Header("����")]
    public float detectRange = 7f;   // ��Ÿ� (�� �ȿ� ������ ��)
    public float stopDistance = 3f;  // �ʹ� ������ ���� (�������� �ʰ�)
    public float moveSpeed = 1.5f;   // �̵� �ӵ�

    public GameObject bulletPrefab;  // �� �� �Ѿ� ������ �� ��������!
    public float attackCooldown = 2f; // 2�ʸ��� �߻�
    protected float lastAttackTime;

    protected Transform player;

    protected bool hasSpotted = false;


    protected virtual void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // ��Ÿ� �ȿ� ��������?
        if (distance <= detectRange)
        {
            
            if (!hasSpotted)
            {
                hasSpotted = true;
                if (SFXManager.Instance != null) 
                 SFXManager.Instance.PlaySFX(SFXType.EnemyEncounter);
            }
            
            
            // ���� ��Ÿ�� üũ -> �߻�
            if (Time.time > lastAttackTime + attackCooldown)
            {
                Shoot();
                lastAttackTime = Time.time;
            }

            // �ʹ� �� ���� �ʰ�, ������ �Ÿ������� �ٰ���
            if (distance > stopDistance)
            {
                transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
            }
        }
    }

    protected virtual void Shoot()
    {
        if (bulletPrefab == null) return;

        // �Ѿ� ����
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

        // �Ѿ˿��� "�÷��̾� ������ ���ư���" ����
        EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
        if (bulletScript != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            bulletScript.SetDirection(direction);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}