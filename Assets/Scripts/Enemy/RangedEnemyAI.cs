using System.Collections;
using UnityEngine;
public class RangedEnemy : MonoBehaviour
{
    [Header("설정")]
    public float detectRange = 7f;   // 사거리 (이 안에 들어오면 감지)
    public float stopDistance = 3f;  // 너무 붙지 않는 최소 거리
    public float moveSpeed = 1.5f;   // 이동 속도
    public GameObject bulletPrefab;  // 발사할 투사체 프리팹
    public float attackCooldown = 2f; // 2초마다 발사
    [SerializeField] protected string attackStateName = "Attack"; // 공격 애니 상태 이름
    protected float lastAttackTime;
    protected Transform player;
    protected Animator anim;
    protected bool hasSpotted = false;

    protected virtual void Start()
    {
        anim = GetComponent<Animator>();
        if (anim != null) anim.enabled = false; // 평소엔 꺼두기

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null) return;
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= detectRange)
        {
            if (!hasSpotted)
            {
                hasSpotted = true;
                if (SFXManager.Instance != null)
                    SFXManager.Instance.PlaySFX(SFXType.EnemyEncounter);
            }

            if (Time.time > lastAttackTime + attackCooldown)
            {
                Shoot();
                lastAttackTime = Time.time;
            }

            if (distance > stopDistance)
            {
                transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
            }
        }
    }

    protected virtual void Shoot()
    {
        // 투사체 발사할 때만 애니 한 번 재생
        if (anim != null)
        {
            StopAllCoroutines();
            StartCoroutine(PlayOnce());
        }

        if (bulletPrefab == null) return;
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
        if (bulletScript != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            bulletScript.SetDirection(direction);
        }
    }

    protected IEnumerator PlayOnce()
    {
        anim.enabled = true;
        anim.Rebind();
        anim.Play(attackStateName, 0, 0f);
        anim.Update(0f);

        yield return null;
        float len = anim.GetCurrentAnimatorStateInfo(0).length;

        yield return new WaitForSeconds(len > 0 ? len : 0.5f);
        anim.enabled = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}