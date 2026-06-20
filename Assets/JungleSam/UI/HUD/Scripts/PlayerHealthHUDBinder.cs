using UnityEngine;

[DisallowMultipleComponent]
public class PlayerHealthHUDBinder : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private GameplayHUDController hud;
    [SerializeField] private float fallbackArmorCurrent = 0f;
    [SerializeField] private float fallbackArmorMax = 100f;

    private bool _subscribedToHealth;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();

        if (playerHealth != null && !_subscribedToHealth)
        {
            playerHealth.OnHealthChanged += OnHealthChanged;
            _subscribedToHealth = true;
        }

        PlayerStats.PlayerDamaged += OnPlayerStatsDamaged;
        PlayerStats.PlayerStatsChanged += OnPlayerStatsChanged;

        Refresh();
    }

    private void OnDisable()
    {
        if (playerHealth != null && _subscribedToHealth)
        {
            playerHealth.OnHealthChanged -= OnHealthChanged;
            _subscribedToHealth = false;
        }

        PlayerStats.PlayerDamaged -= OnPlayerStatsDamaged;
        PlayerStats.PlayerStatsChanged -= OnPlayerStatsChanged;
    }

    [ContextMenu("Refresh")]
    public void Refresh()
    {
        ResolveReferences();

        if (hud == null)
            return;

        if (playerHealth != null)
            hud.SetHealth(playerHealth.CurrentHealth, playerHealth.MaxHealth);

        if (playerStats != null)
            hud.SetArmor(playerStats.Armor, Mathf.Max(1f, playerStats.MaxArmor));
        else
            hud.SetArmor(fallbackArmorCurrent, fallbackArmorMax);
    }

    private void OnHealthChanged(float current, float max)
    {
        if (hud != null)
            hud.SetHealth(current, max);

        RefreshArmor();
    }

    private void ResolveReferences()
    {
        if (hud == null)
            hud = GameplayHUDController.Instance;

        if (hud == null)
            hud = FindFirstObjectByType<GameplayHUDController>();

        if (playerHealth == null)
            playerHealth = GetComponentInParent<PlayerHealth>();

        if (playerHealth == null)
            playerHealth = GetComponentInChildren<PlayerHealth>();

        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (playerStats == null)
            playerStats = GetComponentInParent<PlayerStats>();

        if (playerStats == null)
            playerStats = GetComponentInChildren<PlayerStats>();

        if (playerStats == null)
            playerStats = FindFirstObjectByType<PlayerStats>();
    }

    private void RefreshArmor()
    {
        if (hud == null)
            return;

        if (playerStats != null)
            hud.SetArmor(playerStats.Armor, Mathf.Max(1f, playerStats.MaxArmor));
        else
            hud.SetArmor(fallbackArmorCurrent, fallbackArmorMax);
    }

    private void OnPlayerStatsDamaged(PlayerStats stats, float damage)
    {
        if (playerStats != null && stats != playerStats)
            return;

        if (playerStats == null)
            playerStats = stats;

        Refresh();
    }

    private void OnPlayerStatsChanged(PlayerStats stats)
    {
        if (playerStats != null && stats != playerStats)
            return;

        if (playerStats == null)
            playerStats = stats;

        Refresh();
    }
}
