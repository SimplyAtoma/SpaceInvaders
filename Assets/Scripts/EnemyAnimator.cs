using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyAnimator : MonoBehaviour
{
    [Header("Animator Parameter Names")]
    [Tooltip("Must match EXACTLY what you typed in the Animator Parameters tab (case-sensitive)")]
    [SerializeField] private string stepParam    = "Step";
    [SerializeField] private string shootParam   = "Shoot";
    [SerializeField] private string explodeParam = "Explode";

    [Header("Debug")]
    [SerializeField] private bool logParameterNames = true;

    private Animator anim;
    private bool hasStep, hasShoot, hasExplode;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        ValidateParameters();
    }

    private void ValidateParameters()
    {
        if (anim == null || anim.runtimeAnimatorController == null)
        {
            Debug.LogWarning($"[EnemyAnimator] No AnimatorController assigned on {name}.", this);
            return;
        }

        // Log every parameter found so you can copy-paste the exact names
        if (logParameterNames)
        {
            string found = "";
            foreach (var p in anim.parameters)
                found += $"\n  '{p.name}' ({p.type})";
            Debug.Log($"[EnemyAnimator] Parameters found on {name}:{found}", this);
        }

        hasStep    = HasParameter(stepParam);
        hasShoot   = HasParameter(shootParam);
        hasExplode = HasParameter(explodeParam);

        if (!hasStep)
            Debug.LogWarning($"[EnemyAnimator] '{stepParam}' not found on {name}. " +
                             "Check the log above for exact parameter names.", this);
        if (!hasShoot)
            Debug.LogWarning($"[EnemyAnimator] '{shootParam}' not found on {name}. " +
                             "Check the log above for exact parameter names.", this);
        if (!hasExplode)
            Debug.LogWarning($"[EnemyAnimator] '{explodeParam}' not found on {name}. " +
                             "Check the log above for exact parameter names.", this);
    }

    private bool HasParameter(string paramName)
    {
        foreach (var p in anim.parameters)
            if (p.name == paramName) return true;
        return false;
    }

    public void NotifyStep()
    {
        if (hasStep) anim.SetTrigger(stepParam);
    }

    public void NotifyShoot()
    {
        if (hasShoot) anim.SetTrigger(shootParam);
    }

    public void NotifyExplode()
    {
        if (hasExplode) anim.SetTrigger(explodeParam);
    }
}