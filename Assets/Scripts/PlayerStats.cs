using UnityEngine;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    [Header("Statystyki")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float armor = 0f;

    [Header("UI Reference")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI armorText;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    public void TakeDamage(float amount)
    {
        // Najpierw obrażenia idą w pancerz 
        if (armor > 0)
        {
            armor -= amount;
            if (armor < 0) { currentHealth += armor; armor = 0; }
        }
        else
        {
            currentHealth -= amount;
        }

        UpdateUI();

        if (currentHealth <= 0)
        {
            
            Die();

        }
    }

    void UpdateUI()
    {
        if (healthText != null)
            healthText.text = "HP: " + currentHealth.ToString("F0");

        if (armorText != null)
            armorText.text = "Armor: " + armor.ToString("F0");
    }

    void Die()
    {
        Debug.Log("Gracz zginął!");
        // Tutaj później dodamy ekran Game Over
    }


    // Nowa sekcja dla ekonomii

    [Header("Ekonomia")]
    public int coins = 0;
    public TextMeshProUGUI coinsText; // Przeciągnij tu nowy tekst z UI

    public void AddCoins(int amount)
    {
        coins += amount;
        if (coinsText != null) coinsText.text = "Coins: " + coins;
    }


}