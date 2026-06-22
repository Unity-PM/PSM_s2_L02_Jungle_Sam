using TMPro;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static event System.Action<PlayerStats, float> PlayerDamaged;
    public static event System.Action<PlayerStats> PlayerStatsChanged;

    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float maxArmor = 100f;
    [SerializeField] private float armor = 0f;

    [Header("Economy")]
    [SerializeField] private int coins = 0;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI armorText;
    [SerializeField] private TextMeshProUGUI coinsText;

    private float _currentHealth;
    private bool _isDead;
    private PlayerHealth _playerHealth;

    public float CurrentHealth => _currentHealth;
    public float MaxHealth => _playerHealth != null ? _playerHealth.MaxHealth : maxHealth;
    public float MaxArmor => maxArmor;
    public float CurrentArmor => armor;
    public float Armor => armor;
    public int Coins => coins;
    public bool IsDead => _playerHealth != null ? _playerHealth.IsDead : _isDead;

    private void Start()
    {
        _playerHealth = GetComponent<PlayerHealth>();

        if (_playerHealth != null)
        {
            _currentHealth = _playerHealth.CurrentHealth;
            _playerHealth.OnHealthChanged += OnPlayerHealthChanged;
            _playerHealth.OnDied += OnPlayerHealthDied;
        }
        else
        {
            _currentHealth = maxHealth;
        }

        maxArmor = Mathf.Max(1f, maxArmor);
        armor = Mathf.Clamp(armor, 0f, maxArmor);
        UpdateUI();
        NotifyStatsChanged();
    }

    private void OnDestroy()
    {
        if (_playerHealth == null)
            return;

        _playerHealth.OnHealthChanged -= OnPlayerHealthChanged;
        _playerHealth.OnDied -= OnPlayerHealthDied;
    }

    public void TakeDamage(float amount)
    {
        if (PauseMenuController.IsPaused || Mathf.Approximately(Time.timeScale, 0f))
            return;

        if (IsDead || amount <= 0f)
            return;

        float remainingDamage = amount;

        if (armor > 0f)
        {
            float absorbed = Mathf.Min(armor, remainingDamage);
            armor -= absorbed;
            remainingDamage -= absorbed;
        }

        if (_playerHealth != null)
        {
            if (remainingDamage > 0f)
                _playerHealth.TakeDamage(remainingDamage);
            else
                UpdateUI();

            PlayerDamaged?.Invoke(this, amount);
            NotifyStatsChanged();
            return;
        }

        if (remainingDamage > 0f)
            _currentHealth -= remainingDamage;

        _currentHealth = Mathf.Clamp(_currentHealth, 0f, maxHealth);
        armor = Mathf.Max(0f, armor);

        UpdateUI();
        PlayerDamaged?.Invoke(this, amount);
        NotifyStatsChanged();

        if (_currentHealth <= 0f)
            Die();
    }

    public bool Heal(float amount)
    {
        if (IsDead || amount <= 0f)
            return false;

        float currentMaxHealth = MaxHealth;

        if (_currentHealth >= currentMaxHealth)
            return false;

        if (_playerHealth != null)
        {
            _playerHealth.Heal(amount);
            _currentHealth = Mathf.Clamp(_playerHealth.CurrentHealth, 0f, currentMaxHealth);
            UpdateUI();
            NotifyStatsChanged();
            return true;
        }

        _currentHealth = Mathf.Clamp(_currentHealth + amount, 0f, maxHealth);
        UpdateUI();
        NotifyStatsChanged();
        return true;
    }

    public bool AddArmor(float amount)
    {
        if (IsDead || amount <= 0f)
            return false;

        maxArmor = Mathf.Max(1f, maxArmor);

        if (armor >= maxArmor)
            return false;

        armor = Mathf.Clamp(armor + amount, 0f, maxArmor);
        UpdateUI();
        NotifyStatsChanged();
        return true;
    }

    public void SetArmor(float value)
    {
        maxArmor = Mathf.Max(1f, maxArmor);
        armor = Mathf.Clamp(value, 0f, maxArmor);
        UpdateUI();
        NotifyStatsChanged();
    }

    public void RestoreFullArmor()
    {
        SetArmor(maxArmor);
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0)
            return;

        coins += amount;
        UpdateUI();
        NotifyStatsChanged();
    }

    private void Die()
    {
        if (_isDead)
            return;

        _isDead = true;
        Debug.Log("Gracz zginął!");
        // TODO: GameManager.GameOver()
    }

    private void UpdateUI()
    {
        if (healthText != null)
            healthText.text = $"HP: {_currentHealth:F0}";

        if (armorText != null)
            armorText.text = $"Armor: {armor:F0}";

        if (coinsText != null)
            coinsText.text = $"Coins: {coins}";
    }

    private void OnPlayerHealthChanged(float current, float max)
    {
        _currentHealth = current;
        _isDead = current <= 0f;
        UpdateUI();
        NotifyStatsChanged();
    }

    private void OnPlayerHealthDied()
    {
        _isDead = true;
        Debug.Log("Gracz zginął!");
    }

    private void NotifyStatsChanged()
    {
        PlayerStatsChanged?.Invoke(this);
    }
}
