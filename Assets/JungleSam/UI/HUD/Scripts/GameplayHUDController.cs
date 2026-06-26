using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class GameplayHUDController : MonoBehaviour
{
    public static GameplayHUDController Instance { get; private set; }

    [Header("Objective")]
    [SerializeField] private TMP_Text objectiveHeaderText;
    [SerializeField] private TMP_Text mainObjectiveText;
    [SerializeField] private TMP_Text secondaryObjectiveText;

    [Header("Player Stats")]
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private Image healthBar;
    [SerializeField] private TMP_Text armorText;
    [SerializeField] private Image armorBar;

    [Header("Ammo")]
    [SerializeField] private TMP_Text weaponNameText;
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private TMP_Text reserveAmmoText;
    [SerializeField] private TMP_Text ammoTypeText;

    [Header("Prompt / Notification")]
    [SerializeField] private InteractionPromptUI interactionPrompt;
    [SerializeField] private HUDNotificationUI notification;

    [Header("Defaults")]
    [SerializeField] private bool applyDefaultValuesOnStart = false;

    public string CurrentObjectiveText => mainObjectiveText != null ? mainObjectiveText.text : string.Empty;
    public string CurrentSecondaryObjectiveText => secondaryObjectiveText != null && secondaryObjectiveText.gameObject.activeSelf ? secondaryObjectiveText.text : string.Empty;

    private void Awake()
    {
        if (Instance == null || Instance == this)
            Instance = this;

        AutoWireFromChildren();
        ApplyStartupState();
    }

    private void OnEnable()
    {
        if (Instance == null || Instance == this)
            Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        AutoWireFromChildren();
        ApplyStartupState();
    }

    public void ApplyDefaultState()
    {
        AutoWireFromChildren();
        HideInteractionPrompt();
        HideNotification();
        SetObjective("Znajdź źródło sygnału", "Przedostań się przez nabrzeże");
        SetHealth(100f, 100f);
        SetArmor(0f, 100f);
        SetAmmo("AK-47", 30, 120, "7.62x39mm");
    }

    private void ApplyStartupState()
    {
        AutoWireFromChildren();
        HideInteractionPrompt();
        HideNotification();

        if (applyDefaultValuesOnStart)
            ApplyDefaultState();
    }

    [ContextMenu("Auto Wire From Children")]
    public void AutoWireFromChildren()
    {
        objectiveHeaderText ??= FindTMP("SafeArea/ObjectivePanel/ObjectiveHeaderText");
        mainObjectiveText ??= FindTMP("SafeArea/ObjectivePanel/MainObjectiveText");
        secondaryObjectiveText ??= FindTMP("SafeArea/ObjectivePanel/SecondaryObjectiveText");

        healthText ??= FindTMP("SafeArea/PlayerStatsPanel/HealthText");
        healthBar = ResolveFillImage(healthBar, "SafeArea/PlayerStatsPanel/HealthBar/Fill");
        armorText ??= FindTMP("SafeArea/PlayerStatsPanel/ArmorText");
        armorBar = ResolveFillImage(armorBar, "SafeArea/PlayerStatsPanel/ArmorBar/Fill");

        weaponNameText ??= FindTMP("SafeArea/AmmoPanel/WeaponNameText");
        ammoText ??= FindTMP("SafeArea/AmmoPanel/AmmoText");
        reserveAmmoText ??= FindTMP("SafeArea/AmmoPanel/ReserveAmmoText");
        ammoTypeText ??= FindTMP("SafeArea/AmmoPanel/AmmoTypeText");

        interactionPrompt ??= GetComponentInChildren<InteractionPromptUI>(true);
        notification ??= GetComponentInChildren<HUDNotificationUI>(true);
    }

    public void SetObjective(string main, string secondary)
    {
        if (objectiveHeaderText != null)
            objectiveHeaderText.text = "CEL MISJI";

        if (mainObjectiveText != null)
            mainObjectiveText.text = string.IsNullOrWhiteSpace(main) ? "-" : main;

        if (secondaryObjectiveText != null)
        {
            bool hasSecondary = !string.IsNullOrWhiteSpace(secondary);
            secondaryObjectiveText.gameObject.SetActive(hasSecondary);
            secondaryObjectiveText.text = hasSecondary ? secondary : string.Empty;
        }
    }

    public void SetHealth(float current, float max)
    {
        float normalized = max > 0f ? Mathf.Clamp01(current / max) : 0f;
        float displayCurrent = max > 0f ? Mathf.Clamp(current, 0f, max) : 0f;

        if (healthText != null)
            healthText.text = $"HP {Mathf.CeilToInt(displayCurrent)}";

        if (healthBar != null)
        {
            ConfigureFilledBar(healthBar);
            healthBar.fillAmount = normalized;
        }
    }

    public void SetArmor(float current, float max)
    {
        float normalized = max > 0f ? Mathf.Clamp01(current / max) : 0f;
        float displayCurrent = max > 0f ? Mathf.Clamp(current, 0f, max) : 0f;

        if (armorText != null)
            armorText.text = $"PANCERZ {Mathf.CeilToInt(displayCurrent)}";

        if (armorBar != null)
        {
            ConfigureFilledBar(armorBar);
            armorBar.fillAmount = normalized;
        }
    }

    public void SetAmmo(string weaponName, int magazineAmmo, int reserveAmmo, string ammoType)
    {
        if (weaponNameText != null)
            weaponNameText.text = string.IsNullOrWhiteSpace(weaponName) ? "BROŃ" : weaponName;

        if (ammoText != null)
            ammoText.text = $"{magazineAmmo} / {reserveAmmo}";

        if (reserveAmmoText != null)
            reserveAmmoText.text = $"RESERVE {reserveAmmo}";

        if (ammoTypeText != null)
            ammoTypeText.text = string.IsNullOrWhiteSpace(ammoType) ? "AMMO" : ammoType;
    }

    public void ShowInteractionPrompt(string key, string text)
    {
        AutoWireFromChildren();

        if (interactionPrompt != null)
            interactionPrompt.Show(key, text);
        else
            Debug.Log($"Interaction prompt: [{key}] - {text}");
    }

    public void HideInteractionPrompt()
    {
        AutoWireFromChildren();

        if (interactionPrompt != null)
            interactionPrompt.Hide();
    }

    public void ShowNotification(string text, float duration = 2f)
    {
        AutoWireFromChildren();

        if (notification != null)
            notification.Show(text, duration);
        else
            Debug.Log($"HUD notification: {text}");
    }

    public void HideNotification()
    {
        AutoWireFromChildren();

        if (notification != null)
            notification.Hide();
    }

#if UNITY_EDITOR
    [ContextMenu("Debug Radio Prompt")]
    private void DebugRadioPrompt()
    {
        ShowInteractionPrompt("E", "PODNIEŚ RADIO");
    }

    [ContextMenu("Debug Hide Prompt")]
    private void DebugHidePrompt()
    {
        HideInteractionPrompt();
    }

    [ContextMenu("Debug Objective Updated")]
    private void DebugObjectiveUpdated()
    {
        ShowNotification("CEL ZAKTUALIZOWANY");
    }

    [ContextMenu("Debug Combat Objective")]
    private void DebugCombatObjective()
    {
        SetObjective("Przetrwaj atak zainfekowanych", "Utrzymaj pozycję przy nabrzeżu");
        ShowNotification("CEL ZAKTUALIZOWANY");
    }

    [ContextMenu("Debug Ammo AK")]
    private void DebugAmmoAK()
    {
        SetAmmo("AK-47", 30, 120, "7.62x39mm");
    }

    [ContextMenu("Debug Damage HP")]
    private void DebugDamageHP()
    {
        SetHealth(42f, 100f);
    }

    [ContextMenu("Debug Full HP")]
    private void DebugFullHP()
    {
        SetHealth(100f, 100f);
    }

    [ContextMenu("Debug Half HP")]
    private void DebugHalfHP()
    {
        SetHealth(50f, 100f);
    }

    [ContextMenu("Debug Empty HP")]
    private void DebugEmptyHP()
    {
        SetHealth(0f, 100f);
    }

    [ContextMenu("Debug Armor 25")]
    private void DebugArmor25()
    {
        SetArmor(25f, 100f);
    }

    [ContextMenu("Debug Empty Armor")]
    private void DebugEmptyArmor()
    {
        SetArmor(0f, 100f);
    }
#endif

    private TMP_Text FindTMP(string path)
    {
        Transform target = transform.Find(path);
        return target != null ? target.GetComponent<TMP_Text>() : null;
    }

    private Image FindImage(string path)
    {
        Transform target = transform.Find(path);
        return target != null ? target.GetComponent<Image>() : null;
    }

    private Image ResolveFillImage(Image current, string fillPath)
    {
        Image targetFill = FindImage(fillPath);

        if (targetFill != null && (current == null || current.name != "Fill"))
            current = targetFill;

        if (current != null)
            ConfigureFilledBar(current);

        return current;
    }

    private static void ConfigureFilledBar(Image image)
    {
        if (image == null)
            return;

        image.type = Image.Type.Filled;
        image.fillMethod = Image.FillMethod.Horizontal;
        image.fillOrigin = 0;
    }
}
