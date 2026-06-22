using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
[DisallowMultipleComponent]
public class EnemyAI : MonoBehaviour
{
    private const float NavMeshSnapRadius = 5f;
    private const float DestinationSampleRadius = 4f;
    private const float StuckVelocityThreshold = 0.08f;
    private const float StuckCheckDelay = 1.25f;

    // Statyczny event, który powiadomi Spawner o śmierci wroga.
    public static System.Action OnEnemyDied;
    public event Action<EnemyAI> Died;

    [Header("Stats")]
    [SerializeField] private float health = 50f;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private int coinReward = 10;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 40f;
    [SerializeField] private float loseTargetRange = 48f;
    [SerializeField] private float attackRange = 2.2f;

    [Header("Combat")]
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float fallbackDamageDelay = 0.25f;
    [SerializeField] private bool damageByAnimationEvent = false;

    [Header("Movement")]
    [SerializeField] private float aiTickRate = 0.15f;
    [SerializeField] private float stoppingDistanceMultiplier = 0.85f;
    [SerializeField] private float rotationSpeed = 18f;
    [SerializeField] private bool rotateTowardsMovementWhenChasing = true;

    [Header("Animator Parameters")]
    [SerializeField] private string speedParameter = "Speed";
    [SerializeField] private string attackTrigger = "Attack";
    [SerializeField] private string dieTrigger = "Die";
    [SerializeField] private string hitTrigger = "Hit";

    [Header("Death")]
    [SerializeField] private float destroyAfterDeath = 5f;
    [SerializeField] private bool disableCollidersOnDeath = true;
    [SerializeField] private bool forceDisableCollidersOnDeath = true;
    [SerializeField] private bool useRandomDeathAnimation = true;
    [SerializeField] private float deathCrossFadeDuration = 0.05f;
    [SerializeField] private bool freezeDeathPose = true;
    [SerializeField] private float freezeDeathPoseNormalizedTime = 0.75f;
    [SerializeField] private float freezeDeathPoseFallbackDelay = 1.25f;
    [SerializeField] private float deathStateEnterTimeout = 0.5f;
    [SerializeField] private string[] deathStateNames = { "Z_FallingBack", "Z_FallingForward" };

    [Header("Attack Slots")]
    [SerializeField] private EnemyAttackSlotManager attackSlotManager;

    [Header("Audio")]
    [SerializeField] private EnemyVoiceAudio enemyVoiceAudio;

    private Transform _player;
    private PlayerStats _playerStats;
    private NavMeshAgent _agent;
    private Animator _animator;
    private Collider[] _colliders;

    private float _nextAttackTime;
    private float _aiTickTimer;
    private bool _isDead;
    private bool _isAttacking;
    private bool _deathNotified;
    private bool _hasTarget;
    private Coroutine _freezeDeathPoseRoutine;
    private float _stuckTimer;
    private float _fallbackMoveSpeed;
    private bool _warnedOffNavMesh;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _colliders = GetComponentsInChildren<Collider>();

        if (enemyVoiceAudio == null)
            enemyVoiceAudio = GetComponent<EnemyVoiceAudio>();

        if (enemyVoiceAudio == null)
            enemyVoiceAudio = GetComponentInParent<EnemyVoiceAudio>();

        if (enemyVoiceAudio == null)
            enemyVoiceAudio = GetComponentInChildren<EnemyVoiceAudio>();

        _agent.speed = speed;
        _agent.stoppingDistance = attackRange * stoppingDistanceMultiplier;
        _agent.angularSpeed = 720f;
        _agent.acceleration = 30f;
        _agent.autoBraking = true;
        _agent.avoidancePriority = UnityEngine.Random.Range(30, 70);

        // Obrót robimy ręcznie w RotateTowardsTarget(), więc NavMeshAgent nie obraca modelu sam.
        _agent.updateRotation = false;

        // Ruch robi NavMeshAgent, animacje są in-place.
        _animator.applyRootMotion = false;
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj == null)
        {
            Debug.LogError($"[{name}] Nie znaleziono gracza. Upewnij się, że Player ma tag 'Player'.");
            enabled = false;
            return;
        }

        _player = playerObj.transform;
        _playerStats = playerObj.GetComponent<PlayerStats>();
        ResolveAttackSlotManager();
        TrySnapAgentToNavMesh();
        enemyVoiceAudio?.StartVoiceLoop();

        if (_playerStats == null)
            Debug.LogError($"[{name}] Obiekt Player nie ma komponentu PlayerStats.");
    }

    private void Update()
    {
        if (_isDead || _player == null || IsGameplayPaused())
            return;

        _fallbackMoveSpeed = 0f;
        UpdateAnimatorMovement();

        if (_hasTarget)
            RotateTowardsTarget();

        MonitorAndRecoverIfStuck();

        _aiTickTimer += Time.deltaTime;
        if (_aiTickTimer >= aiTickRate)
        {
            _aiTickTimer = 0f;
            HandleAILogic();
        }
    }

    private void HandleAILogic()
    {
        if (IsGameplayPaused())
            return;

        float distance = GetFlatDistanceToPlayer();

        if (!_hasTarget && distance <= detectionRange)
            _hasTarget = true;

        if (_hasTarget && distance > loseTargetRange)
        {
            _hasTarget = false;
            ReleaseAttackSlot();
            StopMoving();
            return;
        }

        if (!_hasTarget)
        {
            ReleaseAttackSlot();
            StopMoving();
            return;
        }

        if (distance <= attackRange)
        {
            StopMoving();
            TryAttack();
            return;
        }

        ChasePlayer();
    }

    private void ChasePlayer()
    {
        if (!_agent.enabled || !_agent.isOnNavMesh)
        {
            if (!TrySnapAgentToNavMesh())
                DirectChasePlayer();

            return;
        }

        _isAttacking = false;
        _agent.stoppingDistance = attackRange * stoppingDistanceMultiplier;
        _agent.isStopped = false;

        if (attackSlotManager != null)
        {
            Vector3 slotPosition = attackSlotManager.GetOrAssignSlotPosition(
                this,
                _player,
                Mathf.Max(0.1f, _agent.stoppingDistance)
            );

            SetDestinationOnNavMesh(slotPosition);
        }
        else if (NavMesh.SamplePosition(_player.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            SetDestinationOnNavMesh(hit.position);
        }
        else
        {
            SetDestinationOnNavMesh(_player.position);
        }
    }

    private bool TrySnapAgentToNavMesh()
    {
        if (_agent == null || !_agent.enabled || _agent.isOnNavMesh)
            return _agent != null && _agent.enabled && _agent.isOnNavMesh;

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, NavMeshSnapRadius, NavMesh.AllAreas))
        {
            _agent.Warp(hit.position);
            transform.position = hit.position;
            return _agent.isOnNavMesh;
        }

        if (!_warnedOffNavMesh)
        {
            _warnedOffNavMesh = true;
            Debug.LogWarning($"[{name}] Zombie is not on NavMesh. Using direct fallback chase. Move this enemy onto baked NavMesh for stable arena AI.", this);
        }

        return false;
    }

    private void DirectChasePlayer()
    {
        if (_player == null)
            return;

        Vector3 direction = _player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Vector3 moveDirection = direction.normalized;
        transform.position += moveDirection * speed * Time.deltaTime;

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        _fallbackMoveSpeed = speed;

        if (!string.IsNullOrWhiteSpace(speedParameter) && _animator != null)
            _animator.SetFloat(speedParameter, _fallbackMoveSpeed);
    }

    private void SetDestinationOnNavMesh(Vector3 destination)
    {
        if (!_agent.enabled || !_agent.isOnNavMesh)
        {
            if (!TrySnapAgentToNavMesh())
                DirectChasePlayer();

            return;
        }

        if (NavMesh.SamplePosition(destination, out NavMeshHit hit, DestinationSampleRadius, NavMesh.AllAreas))
        {
            if (!_agent.SetDestination(hit.position))
                RecoverAgentPath();

            return;
        }

        RecoverAgentPath();
    }

    private void MonitorAndRecoverIfStuck()
    {
        if (!_hasTarget || _isAttacking || _agent == null || !_agent.enabled || !_agent.isOnNavMesh || _agent.isStopped)
        {
            _stuckTimer = 0f;
            return;
        }

        bool wantsToMove = _agent.hasPath && !_agent.pathPending && _agent.remainingDistance > _agent.stoppingDistance + 0.35f;
        bool barelyMoving = _agent.velocity.sqrMagnitude < StuckVelocityThreshold * StuckVelocityThreshold;

        if (!wantsToMove || !barelyMoving)
        {
            _stuckTimer = 0f;
            return;
        }

        _stuckTimer += Time.deltaTime;

        if (_stuckTimer < StuckCheckDelay)
            return;

        RecoverAgentPath();
        _stuckTimer = 0f;
    }

    private void RecoverAgentPath()
    {
        if (_agent == null || !_agent.enabled || !_agent.isOnNavMesh)
            return;

        _agent.ResetPath();
        ReleaseAttackSlot();

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, NavMeshSnapRadius, NavMesh.AllAreas))
            _agent.Warp(hit.position);

        _aiTickTimer = aiTickRate;
    }

    private void StopMoving()
    {
        if (!_agent.enabled || !_agent.isOnNavMesh)
            return;

        _agent.isStopped = true;
        _agent.ResetPath();
    }

    private void TryAttack()
    {
        if (IsGameplayPaused())
            return;

        if (_isAttacking)
            return;

        if (Time.time < _nextAttackTime)
            return;

        _isAttacking = true;
        _nextAttackTime = Time.time + attackCooldown;

        RotateTowardsPlayer();

        if (!string.IsNullOrWhiteSpace(attackTrigger))
            _animator.SetTrigger(attackTrigger);

        if (!damageByAnimationEvent)
            Invoke(nameof(DealDamageToPlayer), fallbackDamageDelay);

        Invoke(nameof(UnlockAttack), Mathf.Max(0.2f, attackCooldown * 0.75f));
    }

    private void UnlockAttack()
    {
        _isAttacking = false;
    }

    // Możesz podpiąć tę metodę jako Animation Event w klatce kontaktu animacji ataku.
    public void DealDamageToPlayer()
    {
        if (IsGameplayPaused())
            return;

        if (_isDead || _player == null || _playerStats == null)
            return;

        if (GetFlatDistanceToPlayer() > attackRange + 0.35f)
            return;

        _playerStats.TakeDamage(damage);
    }

    public void TakeDamage(float amount)
    {
        if (_isDead)
            return;

        health -= amount;

        if (health <= 0f)
        {
            Die();
            return;
        }

        if (!string.IsNullOrWhiteSpace(hitTrigger))
            _animator.SetTrigger(hitTrigger);
    }

    private void Die()
    {
        if (_isDead)
            return;

        _isDead = true;
        CancelInvoke();
        ReleaseAttackSlot();
        enemyVoiceAudio?.SetDead();

        DisableDeathMovementAndPhysics();

        string deathStateName = PlayDeathAnimation();

        if (freezeDeathPose)
            _freezeDeathPoseRoutine = StartCoroutine(FreezeDeathPoseAfterAnimation(deathStateName));

        if (_playerStats != null)
            _playerStats.AddCoins(coinReward);

        NotifyDeath();
        Destroy(gameObject, destroyAfterDeath);
    }

    private string PlayDeathAnimation()
    {
        if (_animator == null)
            return null;

        _animator.enabled = true;
        _animator.speed = 1f;

        if (!string.IsNullOrWhiteSpace(speedParameter))
            _animator.SetFloat(speedParameter, 0f);

        if (!string.IsNullOrWhiteSpace(attackTrigger))
            _animator.ResetTrigger(attackTrigger);

        if (!string.IsNullOrWhiteSpace(hitTrigger))
            _animator.ResetTrigger(hitTrigger);

        if (useRandomDeathAnimation && deathStateNames != null && deathStateNames.Length > 0)
        {
            string deathStateName = deathStateNames[UnityEngine.Random.Range(0, deathStateNames.Length)];

            if (!string.IsNullOrWhiteSpace(deathStateName))
            {
                _animator.CrossFadeInFixedTime(deathStateName, deathCrossFadeDuration, 0);
                return deathStateName;
            }
        }

        if (!string.IsNullOrWhiteSpace(dieTrigger))
            _animator.SetTrigger(dieTrigger);

        return null;
    }

    private void DisableDeathMovementAndPhysics()
    {
        if (_agent != null)
        {
            if (_agent.enabled && _agent.isOnNavMesh)
            {
                _agent.isStopped = true;
                _agent.ResetPath();
            }

            _agent.enabled = false;
        }

        bool shouldDisableColliders = disableCollidersOnDeath || forceDisableCollidersOnDeath;

        if (shouldDisableColliders && _colliders != null)
        {
            foreach (Collider col in _colliders)
            {
                if (col != null)
                    col.enabled = false;
            }
        }

        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rb in rigidbodies)
        {
            if (rb == null)
                continue;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }

    private IEnumerator FreezeDeathPoseAfterAnimation(string deathStateName)
    {
        if (_animator == null)
            yield break;

        float freezeTime = Mathf.Clamp(freezeDeathPoseNormalizedTime, 0f, 0.75f);

        if (!string.IsNullOrWhiteSpace(deathStateName))
        {
            float enterTimeout = Time.time + Mathf.Max(0f, deathStateEnterTimeout);

            while (_isDead && Time.time < enterTimeout)
            {
                AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

                if (stateInfo.IsName(deathStateName))
                    break;

                yield return null;
            }

            while (_isDead)
            {
                AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

                if (!stateInfo.IsName(deathStateName))
                {
                    ForceDeathPose(deathStateName, freezeTime);
                    break;
                }

                if (stateInfo.normalizedTime >= freezeTime)
                    break;

                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(Mathf.Max(0f, freezeDeathPoseFallbackDelay));
        }

        if (!_isDead || _animator == null)
            yield break;

        ForceDeathPose(deathStateName, freezeTime);
        _animator.speed = 0f;
        _animator.enabled = false;
    }

    private void ForceDeathPose(string deathStateName, float normalizedTime)
    {
        if (_animator == null || string.IsNullOrWhiteSpace(deathStateName))
            return;

        _animator.Play(deathStateName, 0, normalizedTime);
        _animator.Update(0f);
    }

    private void NotifyDeath()
    {
        if (_deathNotified)
            return;

        _deathNotified = true;

        Died?.Invoke(this);
        OnEnemyDied?.Invoke();
    }

    private void ResolveAttackSlotManager()
    {
        if (attackSlotManager != null || _player == null)
            return;

        attackSlotManager = _player.GetComponentInParent<EnemyAttackSlotManager>();
    }

    private void ReleaseAttackSlot()
    {
        if (attackSlotManager != null)
            attackSlotManager.ReleaseSlot(this);
    }

    private static bool IsGameplayPaused()
    {
        return PauseMenuController.IsPaused || Mathf.Approximately(Time.timeScale, 0f);
    }

    private void OnDisable()
    {
        if (_freezeDeathPoseRoutine != null)
        {
            StopCoroutine(_freezeDeathPoseRoutine);
            _freezeDeathPoseRoutine = null;
        }

        ReleaseAttackSlot();
        enemyVoiceAudio?.StopVoiceLoop();
    }

    private void UpdateAnimatorMovement()
    {
        if (string.IsNullOrWhiteSpace(speedParameter))
            return;

        float currentSpeed = _fallbackMoveSpeed;

        if (_agent.enabled && _agent.isOnNavMesh)
            currentSpeed = _agent.velocity.magnitude;

        _animator.SetFloat(speedParameter, currentSpeed);
    }

    private float GetFlatDistanceToPlayer()
    {
        if (_player == null)
            return Mathf.Infinity;

        Vector3 enemyPosition = transform.position;
        Vector3 playerPosition = _player.position;

        enemyPosition.y = 0f;
        playerPosition.y = 0f;

        return Vector3.Distance(enemyPosition, playerPosition);
    }

    private Vector3 GetFlatDirectionToPlayer()
    {
        if (_player == null)
            return transform.forward;

        Vector3 direction = _player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return transform.forward;

        return direction.normalized;
    }

    private void RotateTowardsTarget()
    {
        if (_player == null)
            return;

        Vector3 direction;

        bool useAgentVelocity =
            rotateTowardsMovementWhenChasing &&
            _agent.enabled &&
            _agent.isOnNavMesh &&
            !_agent.isStopped &&
            _agent.velocity.sqrMagnitude > 0.05f;

        if (useAgentVelocity)
            direction = _agent.velocity;
        else
            direction = _player.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * rotationSpeed
        );
    }

    private void RotateTowardsPlayer()
    {
        Vector3 direction = GetFlatDirectionToPlayer();

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * rotationSpeed
        );
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
}
