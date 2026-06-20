using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

[RequireComponent(typeof(NavMeshAgent))]
[DisallowMultipleComponent]
public class MutantStalkerAI : MonoBehaviour, IDamageable
{
    public event Action<MutantStalkerAI> Died;

    [Header("References")]
    [SerializeField] private MutantStalkerAnimator mutantAnimator;
    [SerializeField] private Transform target;
    [SerializeField] private EnemyAttackSlotManager attackSlotManager;

    [Header("Stats")]
    [SerializeField] private float maxHealth = 180f;
    [SerializeField] private float walkSpeed = 2.4f;
    [SerializeField] private float runSpeed = 5.5f;
    [SerializeField] private float damage = 22f;
    [SerializeField] private int coinReward = 35;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 35f;
    [SerializeField] private float loseTargetRange = 48f;
    [SerializeField] private float attackRange = 2.7f;
    [SerializeField] private float attackStopBuffer = 0.35f;
    [SerializeField] private float aiTickRate = 0.12f;

    [Header("Combat")]
    [SerializeField] private float attackCooldown = 1.7f;
    [SerializeField] private float fallbackDamageDelay = 0.35f;
    [SerializeField] private bool damageByAnimationEvent = true;
    [SerializeField] private float attackAnimationLock = 1.1f;
    [SerializeField] private float hitReactionLock = 0.28f;
    [SerializeField] private float hitReactionCooldown = 0.35f;
    [SerializeField] private float rageAnimationLock = 1.2f;

    [Header("Rage")]
    [SerializeField] private bool canRage = true;
    [SerializeField] private float rageHealthThreshold = 0.45f;
    [SerializeField] private float rageSpeedMultiplier = 1.25f;
    [SerializeField] private float rageAttackCooldownMultiplier = 0.75f;

    [Header("Movement")]
    [SerializeField] private float stoppingDistanceMultiplier = 0.82f;
    [SerializeField] private float rotationSpeed = 9f;
    [SerializeField] private float rotationDeadZoneAngle = 3f;
    [SerializeField] private float immediateRotationAngle = 45f;
    [FormerlySerializedAs("rotateTowardsMovementWhenChasing")]
    [SerializeField] private bool rotateToMovementWhileChasing = true;

    [Header("Death")]
    [SerializeField] private float destroyAfterDeath = 7f;
    [SerializeField] private bool disableCollidersOnDeath = true;

    private NavMeshAgent _agent;
    private PlayerStats _targetStats;
    private Collider[] _colliders;
    private float _currentHealth;
    private float _aiTickTimer;
    private float _nextAttackTime;
    private float _actionLockUntil;
    private float _nextHitReactionTime;
    private float _desiredAnimatorSpeed;
    private bool _hasTarget;
    private bool _isAttacking;
    private bool _isDead;
    private bool _isRaging;
    private bool _deathNotified;
    private bool _isMovingToTarget;

    public bool IsDead => _isDead;
    public float CurrentHealth => _currentHealth;
    public float MaxHealth => maxHealth;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _colliders = GetComponentsInChildren<Collider>();

        if (mutantAnimator == null)
            mutantAnimator = GetComponentInChildren<MutantStalkerAnimator>();

        _currentHealth = maxHealth;

        _agent.speed = walkSpeed;
        _agent.stoppingDistance = attackRange * stoppingDistanceMultiplier;
        _agent.angularSpeed = 720f;
        _agent.acceleration = 28f;
        _agent.autoBraking = true;
        _agent.updateRotation = false;
        _agent.updateUpAxis = true;
        _agent.avoidancePriority = UnityEngine.Random.Range(30, 70);
    }

    private void Start()
    {
        ResolveTarget();
        SetActive(false);
    }

    private void Update()
    {
        if (_isDead)
            return;

        ResolveTarget();

        if (IsActionLocked())
            StopAgentOnly();
        else
            UpdateAnimatorSpeed();

        if (_hasTarget || _isAttacking)
            RotateTowardsTarget();

        _aiTickTimer += Time.deltaTime;
        if (_aiTickTimer < aiTickRate)
            return;

        _aiTickTimer = 0f;
        TickAI();
    }

    private void TickAI()
    {
        if (target == null)
        {
            SetActive(false);
            StopMoving();
            return;
        }

        float distance = GetFlatDistanceToTarget();

        TryEnterRage();

        if (_isAttacking || IsActionLocked())
        {
            StopAgentOnly();
            RotateTowardsTargetImmediate();
            return;
        }

        if (!_hasTarget && distance <= detectionRange)
            SetActive(true);

        if (_hasTarget && distance > loseTargetRange)
        {
            SetActive(false);
            StopMoving();
            return;
        }

        if (!_hasTarget)
        {
            StopMoving();
            TryRandomIdle();
            return;
        }

        if (distance <= attackRange)
        {
            StopMoving();
            TryAttack();
            return;
        }

        ChaseTarget();
    }

    private void ResolveTarget()
    {
        if (target != null)
        {
            if (_targetStats == null)
                _targetStats = target.GetComponentInParent<PlayerStats>();

            ResolveAttackSlotManager();
            return;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
            return;

        target = playerObj.transform;
        _targetStats = playerObj.GetComponentInParent<PlayerStats>();
        ResolveAttackSlotManager();
    }

    private void ChaseTarget()
    {
        if (!_agent.enabled || !_agent.isOnNavMesh || target == null)
            return;

        _agent.speed = _isRaging ? runSpeed * rageSpeedMultiplier : runSpeed;
        _desiredAnimatorSpeed = _agent.speed;
        _isMovingToTarget = true;
        _agent.stoppingDistance = Mathf.Max(0.1f, attackRange - attackStopBuffer);
        _agent.isStopped = false;

        if (attackSlotManager != null)
        {
            Vector3 slotPosition = attackSlotManager.GetOrAssignSlotPosition(
                this,
                target,
                Mathf.Max(0.1f, attackRange - attackStopBuffer)
            );

            _agent.SetDestination(slotPosition);
        }
        else if (NavMesh.SamplePosition(target.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            _agent.SetDestination(hit.position);
        }
        else
        {
            _agent.SetDestination(target.position);
        }

        if (mutantAnimator != null)
        {
            mutantAnimator.SetSpeed(_desiredAnimatorSpeed);
            mutantAnimator.PlayRun();
        }
    }

    private void StopMoving()
    {
        StopAgentOnly();

        if (mutantAnimator != null)
            mutantAnimator.PlayIdle();
    }

    private void StopAgentOnly()
    {
        _desiredAnimatorSpeed = 0f;
        _isMovingToTarget = false;

        if (mutantAnimator != null)
            mutantAnimator.SetSpeed(0f);

        if (!_agent.enabled || !_agent.isOnNavMesh)
            return;

        _agent.isStopped = true;
        _agent.ResetPath();
    }

    private void TryAttack()
    {
        if (_isAttacking || target == null)
            return;

        if (Time.time < _nextAttackTime)
            return;

        _isAttacking = true;
        StopAgentOnly();

        float cooldown = _isRaging ? attackCooldown * rageAttackCooldownMultiplier : attackCooldown;
        _nextAttackTime = Time.time + Mathf.Max(0.2f, cooldown);
        LockAction(Mathf.Max(attackAnimationLock, cooldown * 0.75f));

        RotateTowardsTargetImmediate();

        if (mutantAnimator != null)
            mutantAnimator.PlayRandomAttack();

        if (!damageByAnimationEvent)
            Invoke(nameof(DealDamageToTarget), fallbackDamageDelay);

        Invoke(nameof(UnlockAttack), Mathf.Max(0.2f, cooldown * 0.85f));
    }

    private bool IsActionLocked()
    {
        return Time.time < _actionLockUntil;
    }

    private void LockAction(float duration)
    {
        _actionLockUntil = Mathf.Max(_actionLockUntil, Time.time + duration);
    }

    private void UnlockAttack()
    {
        _isAttacking = false;
    }

    public void DealDamageToTarget()
    {
        if (_isDead || target == null || _targetStats == null)
            return;

        if (GetFlatDistanceToTarget() > attackRange + attackStopBuffer)
            return;

        _targetStats.TakeDamage(damage);
    }

    public void TakeDamage(float amount)
    {
        if (_isDead || amount <= 0f)
            return;

        _currentHealth = Mathf.Max(0f, _currentHealth - amount);
        SetActive(true);

        if (_currentHealth <= 0f)
        {
            Die();
            return;
        }

        if (TryEnterRage())
            return;

        if (_isAttacking || IsActionLocked())
            return;

        if (Time.time < _nextHitReactionTime)
            return;

        _nextHitReactionTime = Time.time + hitReactionCooldown;
        StopAgentOnly();
        LockAction(hitReactionLock);

        if (mutantAnimator != null)
            mutantAnimator.PlayRandomHit();
    }

    private bool TryEnterRage()
    {
        if (!canRage || _isRaging || maxHealth <= 0f)
            return false;

        if (_currentHealth / maxHealth > rageHealthThreshold)
            return false;

        _isRaging = true;
        StopAgentOnly();
        LockAction(rageAnimationLock);

        if (mutantAnimator != null)
            mutantAnimator.PlayRage();

        return true;
    }

    private void Die()
    {
        if (_isDead)
            return;

        _isDead = true;
        CancelInvoke();
        ReleaseAttackSlot();
        StopAgentOnly();

        if (_agent.enabled)
            _agent.enabled = false;

        if (disableCollidersOnDeath)
        {
            foreach (Collider col in _colliders)
                col.enabled = false;
        }

        if (_targetStats != null)
            _targetStats.AddCoins(coinReward);

        if (mutantAnimator != null)
            mutantAnimator.PlayRandomDeath();

        NotifyDeath();
        Destroy(gameObject, destroyAfterDeath);
    }

    private void NotifyDeath()
    {
        if (_deathNotified)
            return;

        _deathNotified = true;
        Died?.Invoke(this);
    }

    private void SetActive(bool isActive)
    {
        if (_hasTarget == isActive)
            return;

        _hasTarget = isActive;

        if (!isActive)
            ReleaseAttackSlot();

        if (mutantAnimator != null)
            mutantAnimator.SetActive(isActive);
    }

    private void ResolveAttackSlotManager()
    {
        if (attackSlotManager != null || target == null)
            return;

        attackSlotManager = target.GetComponentInParent<EnemyAttackSlotManager>();
    }

    private void ReleaseAttackSlot()
    {
        if (attackSlotManager != null)
            attackSlotManager.ReleaseSlot(this);
    }

    private void TryRandomIdle()
    {
        if (mutantAnimator == null || UnityEngine.Random.value > 0.03f)
            return;

        mutantAnimator.PlayRandomIdle();
    }

    private void UpdateAnimatorSpeed()
    {
        if (mutantAnimator == null)
            return;

        float speed = 0f;

        if (_agent.enabled && _agent.isOnNavMesh && !_agent.isStopped)
            speed = _agent.velocity.magnitude;

        if (_isMovingToTarget)
            speed = Mathf.Max(speed, _desiredAnimatorSpeed);

        mutantAnimator.SetSpeed(speed);
    }

    private float GetFlatDistanceToTarget()
    {
        if (target == null)
            return Mathf.Infinity;

        Vector3 selfPosition = transform.position;
        Vector3 targetPosition = target.position;

        selfPosition.y = 0f;
        targetPosition.y = 0f;

        return Vector3.Distance(selfPosition, targetPosition);
    }

    private void RotateTowardsTarget()
    {
        if (target == null)
            return;

        Vector3 direction = Vector3.zero;
        bool canRotateToMovement =
            rotateToMovementWhileChasing &&
            _isMovingToTarget &&
            !_isAttacking &&
            !IsActionLocked();

        if (canRotateToMovement)
            direction = GetPlanarMovementDirection();

        if (direction.sqrMagnitude < 0.001f)
            direction = GetPlanarDirectionToTarget();

        RotateTowardsDirection(direction, rotationSpeed, false);
    }

    private void RotateTowardsTargetImmediate()
    {
        if (target == null)
            return;

        RotateTowardsDirection(GetPlanarDirectionToTarget(), immediateRotationAngle, true);
    }

    private Vector3 GetPlanarDirectionToTarget()
    {
        if (target == null)
            return Vector3.zero;

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        return direction;
    }

    private Vector3 GetPlanarMovementDirection()
    {
        if (!_agent.enabled || !_agent.isOnNavMesh)
            return Vector3.zero;

        Vector3 direction = _agent.desiredVelocity;

        if (direction.sqrMagnitude < 0.001f)
            direction = _agent.velocity;

        direction.y = 0f;

        return direction;
    }

    private void RotateTowardsDirection(Vector3 direction, float speed, bool allowImmediate)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        float angle = Quaternion.Angle(transform.rotation, targetRotation);

        if (angle < rotationDeadZoneAngle)
            return;

        float rotationStep = speed * (allowImmediate ? 120f : 60f) * Time.deltaTime;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationStep);
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, loseTargetRange);
    }

    private void OnDisable()
    {
        ReleaseAttackSlot();
    }
}
