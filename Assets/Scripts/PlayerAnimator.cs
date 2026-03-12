using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    [Header("Animator Parameter Names")]
    [Tooltip("Must match EXACTLY what you typed in the Animator Parameters tab")]
    [SerializeField] private string isShootingParam = "isShooting";
    [SerializeField] private string explodeParam    = "Explode";

    [Header("Debug")]
    [SerializeField] private bool logParameterNames = true;

    private Animator anim;
    private bool     isDead;
    private bool     hasIsShooting;
    private bool     hasExplode;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        ValidateParameters();
    }

    private void ValidateParameters()
    {
        if (anim == null || anim.runtimeAnimatorController == null)
        {
            Debug.LogWarning("[PlayerAnimator] No AnimatorController assigned.", this);
            return;
        }

        if (logParameterNames)
        {
            string found = "";
            foreach (var p in anim.parameters)
                found += $"\n  '{p.name}' ({p.type})";
            Debug.Log($"[PlayerAnimator] Parameters found on {name}:{found}", this);
        }

        hasIsShooting = HasParameter(isShootingParam);
        hasExplode    = HasParameter(explodeParam);

        if (!hasIsShooting)
            Debug.LogWarning($"[PlayerAnimator] '{isShootingParam}' not found. Check Animator Parameters tab.", this);
        if (!hasExplode)
            Debug.LogWarning($"[PlayerAnimator] '{explodeParam}' not found. Check Animator Parameters tab.", this);
    }

    private bool HasParameter(string paramName)
    {
        foreach (var p in anim.parameters)
            if (p.name == paramName) return true;
        return false;
    }

    private void OnEnable()  { GameManager.OnPlayerDied += TriggerExplode; }
    private void OnDisable() { GameManager.OnPlayerDied -= TriggerExplode; }

    private void Update()
    {
        if (isDead || !hasIsShooting) return;

        var kb = Keyboard.current;
        var gp = Gamepad.current;
        bool shooting = (kb != null && kb.spaceKey.isPressed)
                     || (gp != null && gp.buttonSouth.isPressed);

        anim.SetBool(isShootingParam, shooting);
    }

    private void TriggerExplode()
    {
        isDead = true;
        if (hasIsShooting) anim.SetBool(isShootingParam, false);
        if (hasExplode)    anim.SetTrigger(explodeParam);
    }
}