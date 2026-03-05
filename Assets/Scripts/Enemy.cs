using System;
using UnityEngine;

public enum EnemyType
{
    Mystery = 0,  // bonus points
    TypeA   = 30, // 30 points — top row
    TypeB   = 20, // 20 points — middle rows
    TypeC   = 10  // 10 points — bottom rows
}

/// <summary>
/// 3D Enemy component.
/// Attach to a GameObject with:
///   - BoxCollider (Is Trigger = true)
///   - Rigidbody (Is Kinematic = true, Use Gravity = false,
///     Freeze Z pos, Freeze all rotation)
/// Tag the GameObject as "Enemy".
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Enemy : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private EnemyType enemyType       = EnemyType.TypeC;
    [SerializeField] private int       mysteryBonusScore = 100;

    // C# event — notifies EnemyFormation when any enemy dies
    public static event Action<Enemy> OnEnemyDied;

    public EnemyType Type => enemyType;

    private void Awake()
    {
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
        int score = enemyType == EnemyType.Mystery
            ? mysteryBonusScore
            : (int)enemyType;

        Debug.Log($"[Enemy] Die() called — type={enemyType}, score={score}, GameManager={GameManager.Instance != null}");
        GameManager.Instance?.AddScore(score);
        OnEnemyDied?.Invoke(this);
        Destroy(gameObject);
    }

    // 3D trigger
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[Enemy] OnTriggerEnter with tag={other.tag}");
        if (other.CompareTag("PlayerBullet"))
        {
            Destroy(other.gameObject);
            Die();
        }
    }
}