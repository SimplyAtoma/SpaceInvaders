using System;
using UnityEngine;

public enum EnemyType
{
    Mystery = 0,
    TypeA   = 30,
    TypeB   = 20,
    TypeC   = 10
}

[RequireComponent(typeof(Rigidbody))]
public class Enemy : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private EnemyType enemyType         = EnemyType.TypeC;
    [SerializeField] private int       mysteryBonusScore = 100;

    [Header("Animation")]
    [SerializeField] private float explodeAnimDuration = 0.4f;

    public static event Action<Enemy> OnEnemyDied;
    public EnemyType Type => enemyType;

    private bool isDying = false;

    private void Awake()
    {
        //  rotates sprite to face the orthographic camera in a 3D project.
        // If your project is 2D, change this to Quaternion.identity.
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity  = false;
        rb.constraints = RigidbodyConstraints.FreezePositionZ
                       | RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationY
                       | RigidbodyConstraints.FreezeRotationZ;
    }

    public void Die()
    {
        if (isDying) return;
        isDying = true;

        // Play explosion sound
        AudioManager.Instance?.PlayEnemyExplode();

        int score = enemyType == EnemyType.Mystery
            ? mysteryBonusScore
            : (int)enemyType;

        GameManager.Instance?.AddScore(score);

        // Notify formation immediately so speed updates
        OnEnemyDied?.Invoke(this);

        // Disable collider immediately so it can't be hit again
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Trigger explode animation then destroy after it finishes
        var animator = GetComponent<EnemyAnimator>();
        if (animator != null)
        {
            animator.NotifyExplode();
            Destroy(gameObject, explodeAnimDuration);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDying) return;
        if (other.CompareTag("PlayerBullet"))
        {
            Destroy(other.gameObject);
            Die();
        }
    }
}