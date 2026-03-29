using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Stats")]
    public float health = 50f;
    public float speed = 5f;
    public float damage = 10f;
    public float attackRange = 2f;

    protected Transform _player;
    protected NavMeshAgent _agent;
    private float _nextAttackTime;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.speed = speed;
        // Znajdź gracza po tagu (pamiętaj, aby ustawić Tag "Player" na swojej kapsule!)
        _player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (_player == null) return;

        float distance = Vector3.Distance(transform.position, _player.position);

        if (distance > attackRange)
        {
            _agent.SetDestination(_player.position);
        }
        else
        {
            Attack();
        }
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        Debug.Log(gameObject.name + " dostał obrażenia. HP: " + health);

        if (health <= 0) Die();
    }

    
    void Attack()
    {
        if (Time.time >= _nextAttackTime)
        {
            // TA LINIA JEST KLUCZOWA - musisz zdefiniować zmienną playerStats
            PlayerStats playerStats = _player.GetComponent<PlayerStats>();

            if (playerStats != null)
            {
                playerStats.TakeDamage(damage);
                Debug.Log("Zadano obrażenia graczowi! Aktualne HP: " + playerStats.currentHealth);
            }

            _nextAttackTime = Time.time + 1.5f; // Odstęp między atakami
        }
    }

    void Die()
    {
        // Tu dodamy później system monet i Gore
        Destroy(gameObject);
    }
}