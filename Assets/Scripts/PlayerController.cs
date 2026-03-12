using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Part 2 update:
/// - Plays shoot SFX via AudioManager on each shot
/// - Triggers scene transition (Credits) on death
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed         = 8f;
    [SerializeField] private float horizontalBoundary = 8f;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform  firePoint;
    [SerializeField] private float      fireRate = 0.5f;

    private float     nextFireTime = 0f;
    private bool      isDead       = false;
    private Rigidbody rb;
    private Keyboard  kb;
    private Gamepad   gp;

    private void Awake()
    {
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        rb = GetComponent<Rigidbody>();
        rb.useGravity  = false;
        rb.constraints = RigidbodyConstraints.FreezePositionZ
                       | RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationY
                       | RigidbodyConstraints.FreezeRotationZ;
    }

    private void Update()
    {
        if (isDead) return;
        kb = Keyboard.current;
        gp = Gamepad.current;
        HandleShooting();
    }

    private void FixedUpdate()
    {
        if (isDead) return;
        HandleMovement();
    }

    private void HandleMovement()
    {
        float input = 0f;
        if (kb != null)
        {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  input = -1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) input =  1f;
        }
        if (gp != null)
        {
            float stickX = gp.leftStick.x.ReadValue();
            if (Mathf.Abs(stickX) > 0.2f) input = stickX;
        }

        Vector3 pos = rb.position;
        pos.x += input * moveSpeed * Time.fixedDeltaTime;
        pos.x  = Mathf.Clamp(pos.x, -horizontalBoundary, horizontalBoundary);
        pos.z  = 0f;
        rb.MovePosition(pos);
    }

    private void HandleShooting()
    {
        bool firePressed = (kb != null && kb.spaceKey.isPressed)
                        || (gp != null && gp.buttonSouth.isPressed);

        if (firePressed && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;

            if (bulletPrefab == null)
            {
                Debug.LogError("PlayerController: bulletPrefab is null! Assign the prefab from the Project window.", this);
                return;
            }

            Transform spawnPoint = firePoint != null ? firePoint : transform;
            Instantiate(bulletPrefab, spawnPoint.position, Quaternion.identity);

            // Play shoot sound
            AudioManager.Instance?.PlayPlayerShoot();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;
        if (other.CompareTag("EnemyBullet") || other.CompareTag("Enemy"))
            Die();
    }

    private void Die()
    {
        isDead = true;

        // Play explosion sound
        AudioManager.Instance?.PlayPlayerExplode();

        GameManager.Instance?.NotifyPlayerDied();

        // Go to credits after a short delay so explode anim can play
        StartCoroutine(DelayedSceneChange());
    }

    private System.Collections.IEnumerator DelayedSceneChange()
    {
        yield return new WaitForSeconds(0.6f);
        SceneController.Instance?.GoToCredits();
        gameObject.SetActive(false);
    }
}