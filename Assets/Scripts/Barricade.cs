using UnityEngine;

/// <summary>
/// 3D Barricade with progressive damage stages.
/// Attach to a GameObject with:
///   - BoxCollider (Is Trigger = true)
///   - SpriteRenderer (still works fine in a 3D project)
/// Tag as "Barricade".
/// Assign barricade_0 through barricade_3 sprites to damageStages[].
/// </summary>
public class Barricade : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 4;

    [Header("Damage Stage Sprites")]
    [Tooltip("Index 0 = full health, index 3 = nearly destroyed")]
    [SerializeField] private Sprite[] damageStages;

    private int            currentHealth;
    private SpriteRenderer sr;

    private void Awake()
    {
        sr            = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
        UpdateVisual();
    }

    public void TakeDamage()
    {
        currentHealth--;
        if (currentHealth <= 0)
            Destroy(gameObject);
        else
            UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (damageStages == null || damageStages.Length == 0) return;

        int stage = maxHealth - currentHealth;
        stage = Mathf.Clamp(stage, 0, damageStages.Length - 1);
        sr.sprite = damageStages[stage];

        // Shrink slightly with each hit
        float t = (float)currentHealth / maxHealth;
        transform.localScale = Vector3.one * Mathf.Lerp(0.6f, 1f, t);
    }

    // 3D trigger
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerBullet") || other.CompareTag("EnemyBullet"))
        {
            Destroy(other.gameObject);
            TakeDamage();
        }
    }
}
