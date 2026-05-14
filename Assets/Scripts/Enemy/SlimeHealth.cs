using UnityEngine;
using System.Collections;

public class SlimeHealth : EnemyHealth {
    [Header("Slime Split")]
    [SerializeField] private int splitCount = 1;

    // [SerializeField] private float childScaleMultiplier = 0.7f;
    [SerializeField] private float childHealthMultiplier = 0.5f;
    [SerializeField] private float splitOffset = 0.4f;

    [Header("Spawn Protection")]
    [SerializeField] private float spawnProtectionDuration = 1f;

    /*
    [Header("Visual")]
    [SerializeField] private Transform visualRoot;
    */

    private MoveMapBounds moveMapBounds;

    protected override void Start() {
        base.Start();

        moveMapBounds = Object.FindAnyObjectByType<MoveMapBounds>();

        /*
        if (visualRoot == null)
        {
            Debug.LogWarning($"{name}: SlimeHealth의 visualRoot가 비어 있습니다. 슬라임 몸통 SpriteRenderer 오브젝트를 연결하세요.");
        }
        */
    }

    protected override void Die() {
        if (isDead) {
            return;
        }

        Split();

        base.Die();
    }

    private void Split() {
        if (splitCount <= 0) {
            return;
        }

        for (int i = 0; i < 2; i++) {
            Vector2 randomOffset = Random.insideUnitCircle.normalized * splitOffset;
            Vector3 spawnPosition = transform.position + (Vector3)randomOffset;

            GameObject child = Instantiate(gameObject, spawnPosition, Quaternion.identity);

            SlimeHealth childSlime = child.GetComponent<SlimeHealth>();
            if (childSlime != null) {
                childSlime.SetSplitCount(splitCount - 1);

                // 크기 조절은 임시 비활성화
                // childSlime.ApplyChildStats(childScaleMultiplier, childHealthMultiplier);

                childSlime.ApplyChildStats(childHealthMultiplier);
                childSlime.StartSpawnProtection(spawnProtectionDuration);
            }

            if (moveMapBounds != null) {
                moveMapBounds.RegisterRoomEnemy(child);
            }
        }
    }

    private void SetSplitCount(int value) {
        splitCount = value;
    }

    private void ApplyChildStats(float healthMultiplier) {
        // 크기 조절은 임시 비활성화
        // ScaleVisual(scaleMultiplier);
        // ScaleCollider(scaleMultiplier);

        ScaleHealth(healthMultiplier);
        UpdateUI();
    }

    /*
    private void ScaleVisual(float scaleMultiplier)
    {
        if (visualRoot == null)
        {
            return;
        }

        visualRoot.localScale *= scaleMultiplier;
    }

    private void ScaleCollider(float scaleMultiplier)
    {
        Collider2D col = GetComponent<Collider2D>();

        if (col == null)
        {
            return;
        }

        if (col is CircleCollider2D circle)
        {
            circle.radius *= scaleMultiplier;
            circle.offset *= scaleMultiplier;
        }
        else if (col is BoxCollider2D box)
        {
            box.size *= scaleMultiplier;
            box.offset *= scaleMultiplier;
        }
        else if (col is CapsuleCollider2D capsule)
        {
            capsule.size *= scaleMultiplier;
            capsule.offset *= scaleMultiplier;
        }
        else if (col is PolygonCollider2D polygon)
        {
            Vector2[] points = polygon.points;

            for (int i = 0; i < points.Length; i++)
            {
                points[i] *= scaleMultiplier;
            }

            polygon.points = points;
            polygon.offset *= scaleMultiplier;
        }
    }
    */

    private void ScaleHealth(float healthMultiplier) {
        EnemyStats childStats = GetComponent<EnemyStats>();

        if (childStats != null) {
            childStats.maxHealth = Mathf.Max(1, Mathf.RoundToInt(childStats.maxHealth * healthMultiplier));
            currentHealth = childStats.maxHealth;
        }
        else {
            currentHealth = Mathf.Max(1, Mathf.RoundToInt(currentHealth * healthMultiplier));
        }

        if (hpSlider != null) {
            hpSlider.maxValue = currentHealth;
            hpSlider.value = currentHealth;
        }
    }

    public void StartSpawnProtection(float duration) {
        StartCoroutine(SpawnProtectionRoutine(duration));
    }

    private IEnumerator SpawnProtectionRoutine(float duration) {
        SetInvincible(true);
        SetEnemyBehavioursEnabled(false);

        yield return new WaitForSeconds(duration);

        SetEnemyBehavioursEnabled(true);
        SetInvincible(false);
    }

    private void SetEnemyBehavioursEnabled(bool value) {
        MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour behaviour in behaviours) {
            if (behaviour == null) {
                continue;
            }

            if (behaviour == this) {
                continue;
            }

            if (behaviour is EnemyHealth) {
                continue;
            }

            behaviour.enabled = value;
        }
    }
}