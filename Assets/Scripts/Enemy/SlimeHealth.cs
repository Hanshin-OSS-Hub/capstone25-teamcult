using UnityEngine;
using System.Collections;

public class SlimeHealth : EnemyHealth {
    [Header("Slime Split")]
    [SerializeField] private GameObject nextSlimePrefab;
    [SerializeField] private int splitSpawnCount = 2;
    [SerializeField] private float splitOffset = 0.4f;

    [Header("Spawn Protection")]
    [SerializeField] private float spawnProtectionDuration = 1f;

    private MoveMapBounds moveMapBounds;

    protected override void Start() {
        base.Start();
        moveMapBounds = Object.FindAnyObjectByType<MoveMapBounds>();
    }

   
    protected override void PlayHitSound() {
        if (SFXManager.Instance != null) SFXManager.Instance.PlaySFX(SFXType.EnemyHit_Slime);
    }

    protected override void PlayDeathSound() {
        if (SFXManager.Instance != null) SFXManager.Instance.PlaySFX(SFXType.EnemyHit_Slime);
    }

    protected override void Die() {
        if (isDead) {
            return;
        }

        Split();
        base.Die();
    }

    private void Split() {
        if (nextSlimePrefab == null) return;
        if (splitSpawnCount <= 0) return;

        for (int i = 0; i < splitSpawnCount; i++) {
            Vector2 randomOffset = Random.insideUnitCircle.normalized * splitOffset;
            Vector3 spawnPosition = transform.position + (Vector3)randomOffset;

            GameObject child = Instantiate(nextSlimePrefab, spawnPosition, Quaternion.identity);

            SlimeHealth childSlime = child.GetComponent<SlimeHealth>();
            if (childSlime != null) {
                childSlime.StartSpawnProtection(spawnProtectionDuration);
            }

            if (moveMapBounds != null) {
                moveMapBounds.RegisterRoomEnemy(child);
            }
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
            if (behaviour == null || behaviour == this || behaviour is EnemyHealth) continue;
            behaviour.enabled = value;
        }
    }
}