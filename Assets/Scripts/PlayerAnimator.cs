using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls player sprite animations via an Animator component.
/// 
/// Required Animator states and transitions:
///   - Idle        (looping, 2+ keyframes)
///   - Shooting    (3 keyframes, transitions back to Idle on exit)
///   - Exploding   (3 keyframes, does NOT loop)
/// 
/// Required Animator Parameters:
///   - bool  isShooting
///   - trigger Explode
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private Animator anim;
    private bool     isDead = false;

    // Animator parameter hashes — faster than string lookup
    private static readonly int IsShooting = Animator.StringToHash("isShooting");
    private static readonly int Explode    = Animator.StringToHash("Explode");

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        GameManager.OnPlayerDied += TriggerExplode;
    }

    private void OnDisable()
    {
        GameManager.OnPlayerDied -= TriggerExplode;
    }

    private void Update()
    {
        if (isDead) return;

        var kb = Keyboard.current;
        var gp = Gamepad.current;

        bool shooting = (kb != null && kb.spaceKey.isPressed)
                     || (gp != null && gp.buttonSouth.isPressed);

        anim.SetBool(IsShooting, shooting);
    }

    private void TriggerExplode()
    {
        isDead = true;
        anim.SetBool(IsShooting, false);
        anim.SetTrigger(Explode);
    }
}