using UnityEngine;

/// <summary>
/// 3D Bullet for both player and enemy projectiles.
/// Uses Transform.Translate along world Y (up/down).
/// Attach to a GameObject with:
///   - BoxCollider (Is Trigger = true)
///   - NO Rigidbody needed (kinematic movement via Translate)
/// Tag player bullets "PlayerBullet", enemy bullets "EnemyBullet".
/// </summary>
public class Bullet : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float speed         = 12f;
    [SerializeField] private bool  isPlayerBullet = true;

    private Vector3 direction;

    private void Start()
    {
        // Move up for player, down for enemies — along world Y axis
        direction = isPlayerBullet ? Vector3.up : Vector3.down;

        // Safety destroy in case bullet flies off screen
        Destroy(gameObject, 4f);
    }

    private void Update()
    {
        // Space.World so rotation of the bullet doesn't affect direction
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    // 3D trigger — OnTriggerEnter (not 2D)
    private void OnTriggerEnter(Collider other)
    {
        if (isPlayerBullet)
        {
            if (other.CompareTag("Enemy"))
            {
                other.GetComponent<Enemy>()?.Die();
                Destroy(gameObject);
            }
            else if (other.CompareTag("Barricade"))
            {
                other.GetComponent<Barricade>()?.TakeDamage();
                Destroy(gameObject);
            }
        }
        else // enemy bullet
        {
            if (other.CompareTag("Barricade"))
            {
                other.GetComponent<Barricade>()?.TakeDamage();
                Destroy(gameObject);
            }
            // Player hit handled in PlayerController.OnTriggerEnter
        }
    }
}
