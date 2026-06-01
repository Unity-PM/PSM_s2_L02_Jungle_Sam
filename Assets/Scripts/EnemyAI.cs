using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
public class EnemyAI : MonoBehaviour
{
    // Statyczny event, który powiadomi Spawner o śmierci wroga bez używania drogich funkcji "Find"
    public static System.Action OnEnemyDied;

    [Header("Stats")]
    [SerializeField] private float health = 50f;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 1.5f;

    private Transform _player;
    private NavMeshAgent _agent;
    private Animator _animator;

    private float _nextAttackTime;
    private float _aiTickTimer;
    private const float AI_TICK_RATE = 0.2f; // Zombie aktualizuje drogę co 0.2 sekundy
    private bool _isDead = false;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();

        _agent.speed = speed;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] Nie znaleziono Gracza! Upewnij się, że obiekt ma Tag 'Player'.");
        }
    }

    void Update()
    {
        if (_player == null || _isDead) return;

        // 1. Aktualizacja animacji ruchu na podstawie fizycznej prędkości Agenta
        // W Blend Tree: 0 = Idle, ~2 = Walk, ~5 = Run
        _animator.SetFloat("Speed", _agent.velocity.magnitude);

        // 2. Optymalizacja AI: Nie pytamy NavMesha o drogę w każdej klatce
        _aiTickTimer += Time.deltaTime;
        if (_aiTickTimer >= AI_TICK_RATE)
        {
            _aiTickTimer = 0f;
            HandleAILogic();
        }
    }

    private void HandleAILogic()
    {
        float distance = Vector3.Distance(transform.position, _player.position);

        if (distance > attackRange)
        {
            // Jeśli agent był zatrzymany po ataku, pozwól mu iść
            if (_agent.isStopped) _agent.isStopped = false;
            _agent.SetDestination(_player.position);
        }
        else
        {
            // Jesteśmy blisko -> zatrzymaj agenta i atakuj
            _agent.isStopped = true;
            TryAttack();
        }
    }

    private void TryAttack()
    {
        if (Time.time >= _nextAttackTime)
        {
            // Wyzwolenie animacji ataku w Animatorze
            _animator.SetTrigger("Attack");

            // TODO: W idealnym świecie obrażenia zadawalibyśmy z eventu animacji (Animation Event), 
            // ale na potrzeby MVP robimy to natychmiast z poziomu kodu:
            PlayerStats playerStats = _player.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(damage);
            }

            _nextAttackTime = Time.time + attackCooldown;
        }
    }

    public void TakeDamage(float amount)
    {
        if (_isDead) return;

        health -= amount;
        // Tutaj Copilot Pro może Ci łatwo dopisać odtwarzanie animacji "Z_Hit" w przyszłości

        if (health <= 0) Die();
    }

    private void Die()
    {
        _isDead = true;
        _agent.isStopped = true;
        GetComponent<Collider>().enabled = false; // Wyłączamy collider, by trup nie blokował gracza

        // Odpalenie animacji śmierci
        _animator.SetTrigger("Die");

        // Nagroda dla gracza
        PlayerStats pStats = _player.GetComponent<PlayerStats>();
        if (pStats != null)
        {
            pStats.AddCoins(10);
        }

        // Informujemy WaveSpawner, że ten wróg poległ
        OnEnemyDied?.Invoke();

        // Usuwamy obiekt po 3 sekundach, żeby animacja śmierci zdążyła się odtworzyć
        Destroy(gameObject, 5f);
    }
}