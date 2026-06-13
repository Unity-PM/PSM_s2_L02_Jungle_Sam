using TMPro;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float armor = 0f;

    [Header("Economy")]
    [SerializeField] private int coins = 0;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI armorText;
    [SerializeField] private TextMeshProUGUI coinsText;

    private float _currentHealth;
    private bool _isDead;

    public float CurrentHealth => _currentHealth;
    public float MaxHealth => maxHealth;
    public float Armor => armor;
    public int Coins => coins;
    public bool IsDead => _isDead;

    private void Start()
    {
        _currentHealth = maxHealth;
        UpdateUI();
    }

    public void TakeDamage(float amount)
    {
        if (_isDead || amount <= 0f)
            return;

        float remainingDamage = amount;

        if (armor > 0f)
        {
            float absorbed = Mathf.Min(armor, remainingDamage);
            armor -= absorbed;
            remainingDamage -= absorbed;
        }

        if (remainingDamage > 0f)
            _currentHealth -= remainingDamage;

        _currentHealth = Mathf.Clamp(_currentHealth, 0f, maxHealth);
        armor = Mathf.Max(0f, armor);

        UpdateUI();

        if (_currentHealth <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        if (_isDead || amount <= 0f)
            return;

        _currentHealth = Mathf.Clamp(_currentHealth + amount, 0f, maxHealth);
        UpdateUI();
    }

    public void AddArmor(float amount)
    {
        if (_isDead || amount <= 0f)
            return;

        armor += amount;
        UpdateUI();
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0)
            return;

        coins += amount;
        UpdateUI();
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
}