using System;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerHealth : MonoBehaviour, IDamageable
{
    public event Action OnDied;
    public event Action<float, float> OnHealthChanged;

    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;

    private bool _isDead;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public bool IsDead => _isDead;





    private void Reset()
    {
        currentHealth = maxHealth;
    }

    private void OnValidate()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }

    private void Awake()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        _isDead = currentHealth <= 0f;
    }

    private void Start()
    {
        NotifyHealthChanged();
    }

    public void TakeDamage(float amount)
    {
        if (_isDead || amount <= 0f)
            return;

        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
        NotifyHealthChanged();

        if (currentHealth <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        if (_isDead || amount <= 0f)
            return;

        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
        NotifyHealthChanged();
    }

    public void RestoreFullHealth()
    {
        _isDead = false;
        currentHealth = maxHealth;
        NotifyHealthChanged();
    }

    [ContextMenu("Debug Kill")]
    private void DebugKill()
    {
        TakeDamage(maxHealth);
    }

    private void Die()
    {
        if (_isDead)
            return;

        _isDead = true;
        OnDied?.Invoke();
    }

    private void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
