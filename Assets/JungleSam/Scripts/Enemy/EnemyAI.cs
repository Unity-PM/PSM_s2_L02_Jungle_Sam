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
    private const float DestinationRepathThreshold = 0.35f;
    private const float PlayerMovedRepathThreshold = 0.45f;
    private const float StuckVelocityThreshold = 0.08f;
    private const float StuckCheckDelay = 0.75f;
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
    [SerializeField] private float attackStartRangeMultiplier = 0.72f;

    [Header("Movement")]
    [SerializeField] private float aiTickRate = 0.15f;
    [SerializeField] private float chasingTickRate = 0.06f;
    [SerializeField] private float closeCombatTickRate = 0.05f;
    [SerializeField] private float rotationSpeed = 18f;
    [SerializeField] private bool rotateTowardsMovementWhenChasing = true;
    [SerializeField] private float destinationRefreshDistance = DestinationRepathThreshold;
    [SerializeField] private float minDestinationRefreshInterval = 0.06f;
    [SerializeField] private float animatorSpeedSmoothing = 18f;
    [SerializeField] private ObstacleAvoidanceType obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
    [SerializeField] private bool useAttackSlots = false;
    [SerializeField] private float agentRadius = 0.45f;

    [Header("Rotation")]
    [SerializeField] private float rotationDeadZoneAngle = 3f;
    [SerializeField] private float maxTurnSpeed = 900f;
    [SerializeField] private float lookDirectionSmoothing = 35f;

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
    private Vector3 _lastDestination;
    private Vector3 _lastPlayerPositionForDestination;
    private float _lastDestinationSetTime;
    private bool _hasLastDestination;
    private Vector3 _smoothedLookDirection;
    private float _animatorMoveSpeed;
    private bool _initializedAfterSpawn;

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
        _agent.radius = Mathf.Max(0.1f, agentRadius);
        _agent.stoppingDistance = GetChaseStoppingDistance();
        _agent.angularSpeed = 720f;
        _agent.acceleration = Mathf.Max(30f, speed * 10f);
        _agent.autoBraking = false;
        _agent.autoRepath = true;
        _agent.obstacleAvoidanceType = obstacleAvoidanceType;
        _agent.avoidancePriority = UnityEngine.Random.Range(30, 70);

        // Obrót robimy ręcznie w RotateTowardsTarget(), więc NavMeshAgent nie obraca modelu sam.
        _agent.updateRotation = false;

        // Ruch robi NavMeshAgent, animacje są in-place.
        _animator.applyRootMotion = false;
    }

    private void Start()
    {
        ResolvePlayerTarget();

        if (_player == null)
        {
            Debug.LogError($"[{name}] Nie znaleziono gracza. Upewnij się, że Player ma tag 'Player'.");
            enabled = false;
            return;
        }

        TrySnapAgentToNavMesh();

        if (!_initializedAfterSpawn)
            _aiTickTimer = UnityEngine.Random.Range(0f, Mathf.Max(0.01f, aiTickRate));

        enemyVoiceAudio?.StartVoiceLoop();

        if (_playerStats == null)
            Debug.LogError($"[{name}] Obiekt Player nie ma komponentu PlayerStats.");
    }

    public void InitializeAfterSpawn(Transform target)
    {
        if (_isDead)
            return;

        ResolvePlayerTarget(target);
        TrySnapAgentToNavMesh();

        _initializedAfterSpawn = true;
        _hasTarget = _player != null;
        _isAttacking = false;
        _stuckTimer = 0f;
        _hasLastDestination = false;
        _aiTickTimer = GetEffectiveTickRate();

        if (_hasTarget)
            ChasePlayer();
    }

    private void Update()
    {
        if (!_isDead && _player == null)
            ResolvePlayerTarget();

        if (!_isDead && useAttackSlots && attackSlotManager == null)
            ResolveAttackSlotManager();

        if (_isDead || _player == null || IsGameplayPaused())
            return;

        _fallbackMoveSpeed = 0f;
        UpdateAnimatorMovement();

        if (_hasTarget)
            RotateTowardsTarget();

        MonitorAndRecoverIfStuck();

        float effectiveTickRate = GetEffectiveTickRate();
        _aiTickTimer += Time.deltaTime;
        if (_aiTickTimer >= effectiveTickRate)
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

        float effectiveLoseTargetRange = Mathf.Max(loseTargetRange, detectionRange);

        if (_hasTarget && distance > effectiveLoseTargetRange)
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

        if (distance <= Mathf.Max(GetAttackStartRange(), GetChaseStoppingDistance()))
        {
            HandleCloseCombat();
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

        ChasePlayer(false);
    }

    private void ChasePlayer(bool pressIntoMelee)
    {
        if (!_agent.enabled || !_agent.isOnNavMesh)
        {
            if (!TrySnapAgentToNavMesh())
                DirectChasePlayer();

            return;
        }

        _isAttacking = false;
        _agent.stoppingDistance = pressIntoMelee ? 0.1f : GetChaseStoppingDistance();
        _agent.isStopped = false;
        if (useAttackSlots)
            ResolveAttackSlotManager();

        if (!pressIntoMelee && ShouldUseAttackSlot())
        {
            Vector3 slotPosition = attackSlotManager.GetOrAssignSlotPosition(
                this,
                _player,
                Mathf.Max(0.45f, _agent.stoppingDistance)
            );

            SetDestinationOnNavMesh(slotPosition);
            return;
        }

        if (useAttackSlots)
            ReleaseAttackSlot();

        SetPlayerDestination(pressIntoMelee);
    }

    private void SetPlayerDestination(bool forceRefresh = false)
    {
        if (_player == null)
            return;

        if (NavMesh.SamplePosition(_player.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            SetDestinationOnNavMesh(hit.position, forceRefresh);
        }
        else
        {
            SetDestinationOnNavMesh(_player.position, forceRefresh);
        }
    }

    private bool ShouldUseAttackSlot()
    {
        return useAttackSlots && attackSlotManager != null && _player != null;
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
        Vector3 nextPosition = transform.position + moveDirection * speed * Time.deltaTime;

        transform.position = nextPosition;
        RotateTowardsDirection(moveDirection);

        _fallbackMoveSpeed = speed;

        if (!string.IsNullOrWhiteSpace(speedParameter) && _animator != null)
            _animator.SetFloat(speedParameter, _fallbackMoveSpeed);
    }

    private void SetDestinationOnNavMesh(Vector3 destination, bool forceRefresh = false)
    {
        if (!_agent.enabled || !_agent.isOnNavMesh)
        {
            if (!TrySnapAgentToNavMesh())
                DirectChasePlayer();

            return;
        }

        if (NavMesh.SamplePosition(destination, out NavMeshHit hit, DestinationSampleRadius, NavMesh.AllAreas))
        {
            Vector3 finalDestination = hit.position;

            if (!forceRefresh && CanReuseCurrentDestination(finalDestination))
                return;

            if (!TrySetCompleteDestination(finalDestination))
            {
                RecoverAgentPath();
                return;
            }

            _lastDestination = finalDestination;
            _lastPlayerPositionForDestination = _player != null ? _player.position : finalDestination;
            _lastDestinationSetTime = Time.time;
            _hasLastDestination = true;

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
        _hasLastDestination = false;
        ReleaseAttackSlot();

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, NavMeshSnapRadius, NavMesh.AllAreas))
            _agent.Warp(hit.position);

        _aiTickTimer = aiTickRate;
    }

    private float GetEffectiveTickRate()
    {
        float baseTickRate = Mathf.Max(0.01f, aiTickRate);

        if (!_hasTarget || _player == null)
            return baseTickRate;

        float distance = GetFlatDistanceToPlayer();

        if (distance <= attackRange + 0.8f)
            return Mathf.Min(baseTickRate, Mathf.Max(0.01f, closeCombatTickRate));

        return Mathf.Min(baseTickRate, Mathf.Max(0.01f, chasingTickRate));
    }

    private bool TrySetCompleteDestination(Vector3 destination)
    {
        return _agent.SetDestination(destination);
    }

    private float GetAttackStartRange()
    {
        return Mathf.Clamp(
            attackRange * Mathf.Clamp01(attackStartRangeMultiplier),
            0.55f,
            Mathf.Max(0.55f, attackRange)
        );
    }

    private float GetChaseStoppingDistance()
    {
        return Mathf.Max(0.8f, attackRange - 0.35f);
    }

    private void StopMoving()
    {
        if (!_agent.enabled || !_agent.isOnNavMesh)
            return;

        _agent.isStopped = true;
        _agent.ResetPath();
        _hasLastDestination = false;
    }

    private void StopForAttack()
    {
        if (!_agent.enabled || !_agent.isOnNavMesh)
            return;

        _agent.isStopped = true;
        _agent.velocity = Vector3.zero;
    }

    private bool CanReuseCurrentDestination(Vector3 nextDestination)
    {
        if (!_hasLastDestination || (!_agent.hasPath && !_agent.pathPending))
            return false;

        if (_player != null)
        {
            Vector3 playerDelta = _player.position - _lastPlayerPositionForDestination;
            playerDelta.y = 0f;

            if (playerDelta.sqrMagnitude > PlayerMovedRepathThreshold * PlayerMovedRepathThreshold)
                return false;
        }

        if (_agent.hasPath && !_agent.pathPending && _agent.velocity.sqrMagnitude < 0.01f)
            return false;

        float refreshDistance = Mathf.Max(DestinationRepathThreshold, destinationRefreshDistance);
        if ((nextDestination - _lastDestination).sqrMagnitude <= refreshDistance * refreshDistance)
            return true;

        float refreshInterval = Mathf.Max(0f, minDestinationRefreshInterval);
        return refreshInterval > 0f && Time.time - _lastDestinationSetTime < refreshInterval;
    }

    private void HandleCloseCombat()
    {
        RotateTowardsPlayer();

        float distance = GetFlatDistanceToPlayer();

        if (distance > attackRange)
        {
            ChasePlayer(true);
            return;
        }

        if (_isAttacking)
        {
            StopForAttack();
            return;
        }

        if (Time.time >= _nextAttackTime)
        {
            StopForAttack();
            StartAttack();
            return;
        }

        // Cooldown nie powinien zamrażać zombie. Przeciwnik dociska bliżej,
        // zamiast stać na zewnętrznym slocie i czekać.
        ChasePlayer(true);
    }

    private void StartAttack()
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

    private void ResolvePlayerTarget(Transform explicitTarget = null)
    {
        if (explicitTarget != null)
            _player = explicitTarget;

        if (_player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                _player = playerObj.transform;
        }

        if (_playerStats == null && _player != null)
        {
            _playerStats = _player.GetComponent<PlayerStats>();
            if (_playerStats == null)
                _playerStats = _player.GetComponentInParent<PlayerStats>();
        }

        if (useAttackSlots)
            ResolveAttackSlotManager();
    }

    private void ResolveAttackSlotManager()
    {
        if (attackSlotManager != null || _player == null)
            return;

        attackSlotManager = _player.GetComponentInParent<EnemyAttackSlotManager>();
    }

    private void ReleaseAttackSlot()
    {
        if (useAttackSlots && attackSlotManager != null)
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
        {
            currentSpeed = _agent.velocity.magnitude;

            if (_agent.hasPath || _agent.pathPending)
                currentSpeed = Mathf.Max(currentSpeed, _agent.desiredVelocity.magnitude);
        }

        float smoothing = Mathf.Max(0f, animatorSpeedSmoothing);

        if (smoothing > 0f)
            _animatorMoveSpeed = Mathf.MoveTowards(_animatorMoveSpeed, currentSpeed, smoothing * Time.deltaTime);
        else
            _animatorMoveSpeed = currentSpeed;

        _animator.SetFloat(speedParameter, _animatorMoveSpeed);
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

        Vector3 direction = Vector3.zero;

        bool useAgentVelocity =
            rotateTowardsMovementWhenChasing &&
            _agent.enabled &&
            _agent.isOnNavMesh &&
            !_agent.isStopped &&
            (_agent.desiredVelocity.sqrMagnitude > 0.05f || _agent.velocity.sqrMagnitude > 0.05f);

        if (useAgentVelocity)
            direction = _agent.desiredVelocity.sqrMagnitude > 0.05f ? _agent.desiredVelocity : _agent.velocity;

        if (direction.sqrMagnitude < 0.001f)
            direction = _player.position - transform.position;

        RotateTowardsDirection(direction);
    }

    private void RotateTowardsPlayer()
    {
        RotateTowardsDirection(GetFlatDirectionToPlayer());
    }

    private void RotateTowardsDirection(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Vector3 targetDirection = direction.normalized;

        if (_smoothedLookDirection.sqrMagnitude < 0.001f)
            _smoothedLookDirection = transform.forward;

        float blend = 1f - Mathf.Exp(-Mathf.Max(0.01f, lookDirectionSmoothing) * Time.deltaTime);
        _smoothedLookDirection = Vector3.Slerp(_smoothedLookDirection, targetDirection, blend);
        _smoothedLookDirection.y = 0f;

        if (_smoothedLookDirection.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(_smoothedLookDirection.normalized, Vector3.up);
        float angle = Quaternion.Angle(transform.rotation, targetRotation);

        float deadZoneAngle = Mathf.Max(3f, rotationDeadZoneAngle);
        if (angle < deadZoneAngle)
            return;

        float turnSpeed = maxTurnSpeed > 0f
            ? maxTurnSpeed
            : Mathf.Max(1f, rotationSpeed * 60f);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            turnSpeed * Time.deltaTime
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
