using System.Collections;
using UnityEngine;

public class RangedEnemy : MonoBehaviour
{
    [Header("설정")]
    public float detectRange = 7f;
    public float stopDistance = 3f;
    public float moveSpeed = 1.5f;
    public GameObject bulletPrefab;
    public float attackCooldown = 2f;

    [Header("발사 위치")]
    [Tooltip("총알이 나갈 위치. 비워두면 적 위치 + 오프셋 사용")]
    public Transform firePoint;
    [Tooltip("firePoint 없을 때 적 기준 발사 위치 보정 (가슴/손 높이로)")]
    public Vector2 fireOffset = new Vector2(0f, 0.8f);

    [Header("방향 전환")]
    [Tooltip("스프라이트가 기본적으로 오른쪽을 보고 있으면 false, 왼쪽을 보고 있으면 true")]
    public bool defaultFacingLeft = false;

    [SerializeField] protected string attackStateName = "Attack";

    protected float lastAttackTime;
    protected Transform player;
    protected Animator anim;
    protected SpriteRenderer sr;
    protected bool hasSpotted = false;
    protected bool isAttacking = false;

    protected virtual void Start()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        bool isWalking = false;

        if (distance <= detectRange)
        {
            if (!hasSpotted)
            {
                hasSpotted = true;
                if (SFXManager.Instance != null)
                    SFXManager.Instance.PlaySFX(SFXType.EnemyEncounter);
            }

            FacePlayer();

            if (!isAttacking && Time.time > lastAttackTime + attackCooldown)
            {
                Shoot();
                lastAttackTime = Time.time;
            }

            if (distance > stopDistance)
            {
                transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
                isWalking = true;
            }
        }

        // 걷기/대기 전환은 컨트롤러가 isWalking 보고 알아서
        if (anim != null && !isAttacking)
            anim.SetBool("isWalking", isWalking);
    }

    protected void FacePlayer()
    {
        if (sr == null || player == null) return;

        bool playerIsRight = player.position.x > transform.position.x;
        sr.flipX = defaultFacingLeft ? playerIsRight : !playerIsRight;
    }

    protected Vector3 GetFirePosition()
    {
        if (firePoint != null) return firePoint.position;

        bool facingRight = (player != null) && (player.position.x > transform.position.x);
        float offsetX = facingRight ? fireOffset.x : -fireOffset.x;
        return transform.position + new Vector3(offsetX, fireOffset.y, 0f);
    }

    protected virtual void Shoot()
    {
        if (anim != null)
        {
            StopAllCoroutines();
            StartCoroutine(PlayOnce());
        }

        if (bulletPrefab == null) return;

        Vector3 firePos = GetFirePosition();
        GameObject bullet = Instantiate(bulletPrefab, firePos, Quaternion.identity);
        EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
        if (bulletScript != null)
        {
            Vector3 direction = (player.position - firePos).normalized;
            bulletScript.SetDirection(direction);
        }
    }

    protected IEnumerator PlayOnce()
    {
        isAttacking = true;

        anim.SetBool("isWalking", false);
        anim.Play(attackStateName, 0, 0f);
        anim.Update(0f);

        yield return null;
        float len = anim.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(len > 0 ? len : 0.5f);

        isAttacking = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}