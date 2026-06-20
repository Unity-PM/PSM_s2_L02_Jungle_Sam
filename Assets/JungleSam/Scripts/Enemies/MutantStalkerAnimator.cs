using UnityEngine;

[DisallowMultipleComponent]
public class MutantStalkerAnimator : MonoBehaviour
{
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsDeadHash = Animator.StringToHash("IsDead");
    private static readonly int IsActiveHash = Animator.StringToHash("IsActive");
    private static readonly int IsRagingHash = Animator.StringToHash("IsRaging");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int GetHitHash = Animator.StringToHash("GetHit");
    private static readonly int DieHash = Animator.StringToHash("Die");
    private static readonly int RageHash = Animator.StringToHash("Rage");
    private static readonly int IdleChangeHash = Animator.StringToHash("IdleChange");
    private static readonly int AttackIndexHash = Animator.StringToHash("AttackIndex");
    private static readonly int HitIndexHash = Animator.StringToHash("HitIndex");
    private static readonly int DeathIndexHash = Animator.StringToHash("DeathIndex");
    private static readonly int IdleIndexHash = Animator.StringToHash("IdleIndex");

    private static readonly int IdleStateHash = Animator.StringToHash("idle1");
    private static readonly int WalkStateHash = Animator.StringToHash("walk2");
    private static readonly int RunStateHash = Animator.StringToHash("run1");
    private static readonly int IdleStateFullPathHash = Animator.StringToHash("Base Layer.idle1");
    private static readonly int WalkStateFullPathHash = Animator.StringToHash("Base Layer.walk2");
    private static readonly int RunStateFullPathHash = Animator.StringToHash("Base Layer.run1");

    private static readonly int[] ActionStateHashes =
    {
        Animator.StringToHash("attack1"),
        Animator.StringToHash("attack1LSpike"),
        Animator.StringToHash("attack1RSpike"),
        Animator.StringToHash("attack2"),
        Animator.StringToHash("attack2LSpike"),
        Animator.StringToHash("attack2RLSpike"),
        Animator.StringToHash("attack3"),
        Animator.StringToHash("attack3RSpike"),
        Animator.StringToHash("attack4"),
        Animator.StringToHash("attack4RSpike"),
        Animator.StringToHash("attack5"),
        Animator.StringToHash("attack5LSpike"),
        Animator.StringToHash("gethit1"),
        Animator.StringToHash("gethit2"),
        Animator.StringToHash("gethit3"),
        Animator.StringToHash("gethit4"),
        Animator.StringToHash("rage"),
        Animator.StringToHash("death1"),
        Animator.StringToHash("death2"),
        Animator.StringToHash("death3"),
        Animator.StringToHash("death4")
    };

    [SerializeField] private Animator animator;

    [Header("Variant Counts")]
    [SerializeField] private int idleVariants = 4;
    [SerializeField] private int attackVariants = 12;
    [SerializeField] private int hitVariants = 4;
    [SerializeField] private int deathVariants = 4;

    [Header("Cross Fade")]
    [SerializeField] private float locomotionFadeTime = 0.03f;

    private int _lastForcedLocomotionStateHash;
    private bool _isDead;

    private void Reset()
    {
        animator = GetComponentInChildren<Animator>();
    }

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    public void SetSpeed(float speed)
    {
        if (animator == null || _isDead)
            return;

        animator.SetFloat(SpeedHash, speed);
    }

    public void SetActive(bool isActive)
    {
        if (animator == null || _isDead)
            return;

        animator.SetBool(IsActiveHash, isActive);
    }

    public void PlayIdle()
    {
        CrossFadeLocomotion(IdleStateFullPathHash, IdleStateHash);
    }

    public void PlayWalk()
    {
        CrossFadeLocomotion(WalkStateFullPathHash, WalkStateHash);
    }

    public void PlayRun()
    {
        CrossFadeLocomotion(RunStateFullPathHash, RunStateHash);
    }

    public void PlayRandomIdle()
    {
        if (animator == null || _isDead || IsInActionState())
            return;

        animator.SetInteger(IdleIndexHash, GetRandomIndex(idleVariants));
        ResetAllTriggers();
        animator.SetTrigger(IdleChangeHash);
    }

    public void PlayRandomAttack()
    {
        if (animator == null || _isDead)
            return;

        animator.SetInteger(AttackIndexHash, GetRandomIndex(attackVariants));
        ResetAllTriggers();
        animator.SetTrigger(AttackHash);
        _lastForcedLocomotionStateHash = 0;
    }

    public void PlayRandomHit()
    {
        if (animator == null || _isDead)
            return;

        animator.SetInteger(HitIndexHash, GetRandomIndex(hitVariants));
        ResetAllTriggers();
        animator.SetTrigger(GetHitHash);
        _lastForcedLocomotionStateHash = 0;
    }

    public void PlayRage()
    {
        if (animator == null || _isDead)
            return;

        animator.SetBool(IsRagingHash, true);
        ResetAllTriggers();
        animator.SetTrigger(RageHash);
        _lastForcedLocomotionStateHash = 0;
    }

    public void PlayRandomDeath()
    {
        if (animator == null || _isDead)
            return;

        _isDead = true;
        animator.SetBool(IsDeadHash, true);
        animator.SetInteger(DeathIndexHash, GetRandomIndex(deathVariants));
        ResetAllTriggers();
        animator.SetTrigger(DieHash);
        _lastForcedLocomotionStateHash = 0;
    }

    private void CrossFadeLocomotion(int fullPathHash, int shortNameHash)
    {
        if (animator == null || _isDead || _lastForcedLocomotionStateHash == fullPathHash || IsInActionState())
            return;

        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
        if (currentState.shortNameHash == shortNameHash)
        {
            _lastForcedLocomotionStateHash = fullPathHash;
            return;
        }

        _lastForcedLocomotionStateHash = fullPathHash;
        animator.CrossFadeInFixedTime(fullPathHash, locomotionFadeTime, 0);
    }

    private bool IsInActionState()
    {
        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
        if (IsActionStateHash(currentState.shortNameHash))
            return true;

        if (!animator.IsInTransition(0))
            return false;

        AnimatorStateInfo nextState = animator.GetNextAnimatorStateInfo(0);
        return IsActionStateHash(nextState.shortNameHash);
    }

    private static bool IsActionStateHash(int stateHash)
    {
        for (int i = 0; i < ActionStateHashes.Length; i++)
        {
            if (ActionStateHashes[i] == stateHash)
                return true;
        }

        return false;
    }

    private void ResetAllTriggers()
    {
        animator.ResetTrigger(AttackHash);
        animator.ResetTrigger(GetHitHash);
        animator.ResetTrigger(DieHash);
        animator.ResetTrigger(RageHash);
        animator.ResetTrigger(IdleChangeHash);
    }

    private static int GetRandomIndex(int variantCount)
    {
        return Random.Range(1, Mathf.Max(1, variantCount) + 1);
    }
}
