using UnityEngine;

/// <summary>
/// Part 2 update: plays enemy shoot SFX on spawn.
/// </summary>
public class Bullet : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float speed          = 12f;
    [SerializeField] private bool  isPlayerBullet = true;

    private Vector3 direction;

    private void Start()
    {
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        direction = isPlayerBullet ? Vector3.up : Vector3.down;

        // Enemy bullets play shoot SFX on spawn
        if (!isPlayerBullet)
            AudioManager.Instance?.PlayEnemyShoot();

        Destroy(gameObject, 4f);
    }

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        Vector3 pos = transform.position;
        pos.z = 0f;
        transform.position = pos;
    }

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
        else
        {
            if (other.CompareTag("Barricade"))
            {
                other.GetComponent<Barricade>()?.TakeDamage();
                Destroy(gameObject);
            }
        }
    }
}